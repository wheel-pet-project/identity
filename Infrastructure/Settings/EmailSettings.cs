namespace Infrastructure.Settings;

public class EmailSettings
{
    public string DefaultFromEmail { get; init; } = null!;

    public string SmtpHost { get; init; } = null!;

    public int SmtpPort { get; init; }
}