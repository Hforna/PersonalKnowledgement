using PersonalKnowledge.Domain.Enums;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace PersonalKnowledge.Application;

public interface IAssetHandlerService
{
    public FileExtension AssetParsingType { get; }   
    public Task<string> GetAssetText(Stream stream);   
}

public class PdfHandlerService : IAssetHandlerService
{
    public FileExtension AssetParsingType { get; } = FileExtension.Pdf;

    public Task<string> GetAssetText(Stream pdfStream)
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