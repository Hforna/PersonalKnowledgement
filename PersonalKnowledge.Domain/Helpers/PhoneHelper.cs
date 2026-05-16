using System.Text.RegularExpressions;

namespace PersonalKnowledge.Domain.Helpers;

public static class PhoneHelper
{
    public static string NormalizePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return string.Empty;
        }

        // Remove "whatsapp:" prefix if present
        var normalized = phoneNumber.StartsWith("whatsapp:", StringComparison.OrdinalIgnoreCase) 
            ? phoneNumber.Substring(9) 
            : phoneNumber;

        // Remove all non-numeric characters
        normalized = Regex.Replace(normalized, @"[^\d]", "");

        if (string.IsNullOrEmpty(normalized))
        {
            return string.Empty;
        }

        return "+" + normalized;
    }
}
