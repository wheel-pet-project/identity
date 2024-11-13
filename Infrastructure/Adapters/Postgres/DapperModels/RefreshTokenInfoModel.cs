namespace Infrastructure.Adapters.Postgres.DapperModels;

public record RefreshTokenInfoModel(
    Guid account_id, 
    bool is_revoked);