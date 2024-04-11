namespace MoFTaxRSS.Configs;

public record EmailServiceOptions
{
    public string Host { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public int Port { get; init; }
    public bool EnableSSL { get; init; }
    public string FromAddress { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
}
