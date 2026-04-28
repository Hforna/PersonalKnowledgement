using PersonalKnowledge.Domain.Dtos;

namespace PersonalKnowledge.Domain.Services;

public interface IAssetSenderService
{
    public Task Send(ChatResponseToSenderDto dto);
}