using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Infrastructure.Interfaces.JwtProvider;
using Domain.AccountAggregate;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.JwtProvider;

public class JwtProvider(IOptions<JwtOptions> jwtOptions) : IJwtProvider
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    public string GenerateToken(Account account)
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
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.Expiration));

        return new JwtSecurityTokenHandler().WriteToken(accessToken);
    }

    public async Task<(bool isValid, Guid accId, int role, int status)> 
        VerifyToken(string accessToken)
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

        if (result.IsValid)
        {
            var claims = result.Claims;
            return (isValid: true,
                    accId: Guid.Parse(claims.First(c => c.Key == "acc_id").Value.ToString()!),
                    role: int.Parse(claims.First(c => c.Key == "role_id").Value.ToString()!),
                    status: int.Parse(claims.First(x => x.Key == "status_id").Value.ToString()!));
        }

        return (isValid: false, 
            accId: Guid.Empty, 0, 0);
    }
}