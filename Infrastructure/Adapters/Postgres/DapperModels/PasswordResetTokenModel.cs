namespace Infrastructure.Adapters.Postgres.DapperModels;

public record PasswordResetTokenModel(
    string reset_token, 
    bool is_already_applied,
    DateTime expires_in);