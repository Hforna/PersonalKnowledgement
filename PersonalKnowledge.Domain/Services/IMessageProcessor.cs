using PersonalKnowledge.Domain.Dtos;

namespace PersonalKnowledge.Domain.Services;

public interface IMessageProcessor
{
    Task ProcessMessage(ReceiveDto receiveDto, Guid userId, ConversationSource source);
}
