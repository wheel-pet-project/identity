namespace Infrastructure.Repositories.Implementations.DapperModels;

public record RefreshTokenInfoModel(
    Guid account_id, 
    bool is_revoked);