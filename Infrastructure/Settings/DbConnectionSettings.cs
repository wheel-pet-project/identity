namespace Infrastructure.Settings;

public class DbConnectionSettings
{
    public required string Host { get; init; }

    public required  int Port { get; init; }
    
    public required string Database { get; init; }
    
    public required string Username { get; init; }
    
    public required string Password { get; init; }
}