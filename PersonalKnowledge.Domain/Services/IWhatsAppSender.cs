namespace PersonalKnowledge.Domain.Services;

public interface IWhatsAppSender
{
    public Task SendLinkWithButton(string to, string label, string urlParam);
}