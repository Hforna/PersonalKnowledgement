namespace PersonalKnowledge.Domain.Services;

public interface IWhatsAppSender
{
    public Task SendLinkWithButton(string to, string text, string buttonText, string url);
}