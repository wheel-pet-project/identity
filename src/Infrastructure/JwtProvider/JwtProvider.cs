using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Domain.AccountAggregate;
using Core.Domain.RefreshTokenAggregate;
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

        var signingCredentials = CreateSigningCredentials();

        var accessToken = CreateJwtToken(
            claims,
            signingCredentials,
            TimeSpan.FromMinutes(_jwtOptions.AccessTokenExpirationMinutes));

        return new JwtSecurityTokenHandler().WriteToken(accessToken);
    }

    public string GenerateJwtRefreshToken(RefreshToken refreshToken)
    {
        Claim[] claims = [new("token_id", refreshToken.Id.ToString())];

        var signingCredentials = CreateSigningCredentials();

        var jwtToken = CreateJwtToken(
            claims,
            signingCredentials,
            TimeSpan.FromDays(_jwtOptions.RefreshTokenExpirationDays));

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    private SigningCredentials CreateSigningCredentials()
    {
        return new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256);
    }

    private JwtSecurityToken CreateJwtToken(Claim[] claims, SigningCredentials signingCredentials, TimeSpan duration)
    {
        return new JwtSecurityToken(
            claims: claims,
            issuer: _jwtOptions.Issuer,
            signingCredentials: signingCredentials,
            expires: DateTime.UtcNow.Add(duration));
    }

    public async Task<Result<(Guid accountId, Role role, Status status)>> VerifyJwtAccessToken(string accessToken)
    {
        var validatingResult = await ValidateTokenAndGetClaims(accessToken);
        if (validatingResult.IsFailed) return Result.Fail(validatingResult.Errors);
        var claims = validatingResult.Value;

        var accountId = Guid.Parse(claims.First(c => c.Type == "acc_id").Value.ToString());
        var role = Role.FromId(int.Parse(claims.First(c => c.Type == "role_id").Value.ToString()));
        var status = Status.FromId(int.Parse(claims.First(c => c.Type == "status_id").Value.ToString()));

        return Result.Ok((accountId, role, status));
    }

    public async Task<Result<Guid>> VerifyJwtRefreshToken(string refreshToken)
    {
        var validatingResult = await ValidateTokenAndGetClaims(refreshToken);
        if (validatingResult.IsFailed) return Result.Fail(validatingResult.Errors);
        var claims = validatingResult.Value;

        var refreshTokenId = Guid.Parse(claims.First(c => c.Type == "token_id").Value.ToString());

        return Result.Ok(refreshTokenId);
    }

    private async Task<Result<Claim[]>> ValidateTokenAndGetClaims(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var result = await tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOptions.Issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _jwtOptions.SecretKey))
        });

        return result.IsValid == false 
            ? Result.Fail("Invalid token") 
            : result.ClaimsIdentity.Claims.ToArray();
    }
}