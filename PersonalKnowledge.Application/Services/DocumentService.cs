using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Exceptions;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Application.Services;

public interface IDocumentService
{
    public Task UploadFile(List<IFormFile> files, Guid userId);   
}

public class DocumentService : IDocumentService
{
    private readonly IFileHandlerService _fileHandlerService;
    private readonly IEnumerable<IDocumentHandlerService> _documentHandlerServices;
    private readonly ILogger<DocumentService> _logger;
    private readonly IUnitOfWork _uow;
    private readonly IStorageService _storageService;
    
    public DocumentService(IFileHandlerService fileHandlerService, IEnumerable<IDocumentHandlerService> parserService,
        ILogger<DocumentService> logger, IUnitOfWork unitOfWork)
    {
        _fileHandlerService = fileHandlerService;
        _documentHandlerServices = parserService;
        _logger = logger;
        _uow = unitOfWork;       
    }

    public async Task UploadFile(List<IFormFile> files, Guid userId)
    {
        await _uow.BeginTransactionAsync();
        var createdDocuments = new List<Document>();
        
        var streamFiles = files.ToDictionary(d => d.OpenReadStream(), f => f.FileName).ToList();

        foreach (var stream in streamFiles)
        {
            var (isValid, type) = _fileHandlerService.IsValidFile(stream.Key, stream.Value);
            
            if(type is null || !isValid)
                throw new InvalidDocumentFormatException(stream.Value);
            
            _logger.LogInformation($"Document type: {type}");

            var documentHandlerService = _documentHandlerServices.FirstOrDefault(d => d.DocumentParsingType == type)
                ?? throw new DocumentHandlerNotFoundException((DocumentType)type);

            var documentText = await documentHandlerService.GetDocumentText(stream.Key);

            var textChunks = _fileHandlerService.Chunk(documentText, 20, 30);

            var document = new Document()
            {
                FileName = stream.Value,
                FileType = (DocumentType)type,
                TotalChunks = textChunks.Length,
                UserId = userId
            };

            await _uow.GenericRepository.AddAsync(document);

            var chunkIndex = 0;
            foreach (var textChunk in textChunks)
            {
                var chunk = new Chunk()
                {
                    Text = textChunk,
                    ChunkIndex = chunkIndex++,
                    DocumentId = document.Id,
                    Document = document
                };

                await _uow.GenericRepository.AddAsync(chunk);
            }

            //await _storageService.UploadAsync(stream.Key, stream.Value);

            createdDocuments.Add(document);
        }
        
        await _uow.GenericRepository.AddRangeAsync<Document>(createdDocuments);
        await _uow.CommitAsync();
        
        foreach (var document in createdDocuments)
        {
            BackgroundJob.Enqueue<IDocumentProcessing>(d => d.ProcessDocument(document.Id));
        }
    }
}