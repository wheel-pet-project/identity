namespace Infrastructure.Repositories.Implementations.DapperModels;

public record RefreshTokenInfoModel(
    Guid AccountId, 
    bool IsRevoked);