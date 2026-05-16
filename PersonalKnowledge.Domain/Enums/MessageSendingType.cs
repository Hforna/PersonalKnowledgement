using System.ComponentModel;
using System.Reflection;

namespace PersonalKnowledge.Domain.Enums;

public enum MessageSendingType
{
    [Description("A action that the user will click and send or will be redirect to some place")]
    Action,
    [Description("A normal message containing a text body will be send to the user")]
    Message,
    [Description("Means that the system will send a message of media type to the user")]
    Media,
}

public static class MessageSendingTypeHelper
{
    public static string GetMessageSendingTypesDescription()
    {
        return string.Join(", ", Enum.GetValues<MessageSendingType>()
            .Select(type =>
            {
                var field = type.GetType().GetField(type.ToString());
                var descriptionAttribute = field?.GetCustomAttribute<DescriptionAttribute>();

                return $"{type}: {descriptionAttribute?.Description ?? type.ToString()}";
            }));
    }
}