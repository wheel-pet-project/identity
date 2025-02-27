using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Domain.AccountAggregate;
using Core.Infrastructure.Interfaces.JwtProvider;
using FluentResults;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.JwtProvider;

public class JwtProvider(IOptions<JwtOptions> jwtOptions) : IJwtProvider
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public string GenerateJwtAccessToken(Account account)
    {
        Claim[] claims =
        [
            new("acc_id", account.Id.ToString()),
            new("role_id", account.Role.Id.ToString()),
            new("status_id", account.Status.Id.ToString())
        ];

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var accessToken = new JwtSecurityToken(
            claims: claims,
            issuer: _jwtOptions.Issuer,
            signingCredentials: signingCredentials,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes));

        return new JwtSecurityTokenHandler().WriteToken(accessToken);
    }

    public string GenerateJwtRefreshToken(Guid refreshTokenId)
    {
        Claim[] claims = [new("token_id", refreshTokenId.ToString())];

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var refreshToken = new JwtSecurityToken(
            claims: claims,
            issuer: _jwtOptions.Issuer,
            signingCredentials: signingCredentials,
            expires: DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays));

        return new JwtSecurityTokenHandler().WriteToken(refreshToken);
    }

    public async Task<Result<(Guid accountId, Role role, Status status)>> VerifyJwtAccessToken(
        string accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var result = await tokenHandler.ValidateTokenAsync(accessToken,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    _jwtOptions.SecretKey))
            });

        if (result.IsValid == false)
            return Result.Fail("Invalid access token");

        if (result.SecurityToken.ValidTo < DateTime.UtcNow)
            return Result.Fail("Access token expired");

        var claims = result.Claims;
        var accountId = Guid.Parse(claims.First(c => c.Key == "acc_id").Value.ToString()!);
        var role = Role.FromId(int.Parse(claims.First(c => c.Key == "role_id").Value.ToString()!));
        var status = Status.FromId(int.Parse(claims.First(c => c.Key == "status_id").Value.ToString()!));

        return Result.Ok((accountId, role, status));
    }

    public async Task<Result<Guid>> VerifyJwtRefreshToken(string refreshToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var result = await tokenHandler.ValidateTokenAsync(refreshToken,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtOptions.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    _jwtOptions.SecretKey))
            });

        if (result.IsValid == false)
            return Result.Fail("Invalid refresh token");

        if (result.SecurityToken.ValidTo < DateTime.UtcNow)
            return Result.Fail("Refresh token expired");

        var claims = result.Claims;
        var refreshTokenId = Guid.Parse(claims.First(c => c.Key == "token_id").Value.ToString()!);

        return Result.Ok(refreshTokenId);
    }
}