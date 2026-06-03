namespace Aliyar.Web.Models;

public static class PaymentMethodExtensions
{
    public static string ToDisplayNameRu(this PaymentMethod method) =>
        method switch
        {
            PaymentMethod.Cash => "Наличка",
            PaymentMethod.Card => "Карта",
            PaymentMethod.Qr => "QR",
            _ => method.ToString(),
        };
}

