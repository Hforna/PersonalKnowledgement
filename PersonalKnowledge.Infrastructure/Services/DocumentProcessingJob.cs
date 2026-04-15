using Microsoft.Extensions.Logging;
using PersonalKnowledge.Domain.Entities;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Services;
using PersonalKnowledge.Domain.Exceptions;
using Serilog;

namespace PersonalKnowledge.Infrastructure.Services;

public class DocumentProcessingJob : IDocumentProcessing
{
    private readonly IUnitOfWork _uow;
    private readonly IStorageService _storageService;
    private readonly IEmbeddingsHandlerService _embeddingsHandlerService;
    private readonly IVectorDatabaseService _vectorDatabaseService;
    private readonly ILogger<DocumentProcessingJob> _logger;

    public DocumentProcessingJob(IUnitOfWork uow, IStorageService storageService, 
        IEmbeddingsHandlerService embeddingsHandlerService, IVectorDatabaseService vectorDatabaseService, ILogger<DocumentProcessingJob> logger)
    {
        _uow = uow;
        _storageService = storageService;
        _embeddingsHandlerService = embeddingsHandlerService;
        _vectorDatabaseService = vectorDatabaseService;
        _logger = logger;       
    }

    public async Task ProcessDocument(Guid documentId)
    {
        var document = await _uow.GenericRepository.GetByIdAsync<Document>(documentId)
            ?? throw new EntityNotFoundException(nameof(Document), documentId);

        var chunks = await _uow.DocumentRepository.GetDocumentChunksAsync(documentId);

        foreach (var chunk in chunks)
        {
            var embedding = await _embeddingsHandlerService.GenerateEmbedding(chunk.Text);
            
            _logger.LogInformation($"Embedding generated for chunk {chunk.Id}, {embedding}");

            await _vectorDatabaseService.InsertEmbedding(chunk.Id, embedding, new() { { "text", chunk.Text }, { "document_id", document.Id.ToString() } });
        }
        
        document.ProcessDocument();
        
        _uow.GenericRepository.Update(document);
        await _uow.CommitAsync();
    }
}