using System.Text.Json.Serialization;

namespace AU.Domain.ValueObjects;

public sealed class EmailChannelConfig : ChannelConfig
{
    public IReadOnlyList<string> To { get; }
    
    [JsonConstructor]
    private EmailChannelConfig(IReadOnlyList<string> to)
    {
        To = to;
    }
    
    public static EmailChannelConfig Create(IEnumerable<string> to)
    {
        var addresses = to?.Where(a => !string.IsNullOrWhiteSpace(a)).ToList() ?? [];

        if (addresses.Count == 0)
            throw new ArgumentException("Kanał email musi mieć co najmniej jednego adresata.", nameof(to));

        //foreach (var address in addresses)
        //{
        //    if (!IsValidEmail(address))
        //        throw new ArgumentException($"Nieprawidłowy adres email: {address}", nameof(to));
        //}

        return new EmailChannelConfig(addresses);
    }
}