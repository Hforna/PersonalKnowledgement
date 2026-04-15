using PersonalKnowledge.Domain.Enums;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace PersonalKnowledge.Application;

public interface IDocumentHandlerService
{
    public DocumentType DocumentParsingType { get; }   
    public Task<string> GetDocumentText(Stream pdfStream);   
}

public class PdfHandlerService : IDocumentHandlerService
{
    public DocumentType DocumentParsingType { get; } = DocumentType.Pdf;

    public Task<string> GetDocumentText(Stream pdfStream)
    {
        using var pdfDocument = PdfDocument.Open(pdfStream);
        
        var pdfWords = new List<string>();
        foreach (var page in pdfDocument.GetPages())
        {
            var pageWords = page.GetWords(NearestNeighbourWordExtractor.Instance);
            pdfWords.AddRange(pageWords.Select(d => d.Text));
        }

        return Task.FromResult(string.Join(' ', pdfWords));
    }
}