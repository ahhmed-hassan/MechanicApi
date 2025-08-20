using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.Identity.DTOs;
using MechanicApplication.Features.Identity.Queries;
using MechanicDomain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MechanicInfrastructure.Identity;

internal class TokenProvider(
    IConfiguration configuration, 
    IAppDbContext appDbContext
    ) : ITokenProvidere
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IAppDbContext _context = appDbContext;
    public async Task<ErrorOr<TokenResponse>> GenerateJwtTokenAsync(AppUserDTO user, CancellationToken ct = default)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var issuer = jwtSettings["Issuer"]!;
        var audience = jwtSettings["Audience"]!;
        var key = jwtSettings["Secret"]!;

        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["TokenExpirationInMinutes"]!));

        if(user.UserId is null || user.Email is null)
            return Error.Validation("Invalid_User", "User ID or Email is null.");

        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.UserId),
            new (JwtRegisteredClaimNames.Email, user.Email),
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256Signature),
        };


        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(descriptor);

        var oldRefreshTokens = await _context.RefreshTokens
              .Where(rt => rt.UserId == user.UserId)
              .ExecuteDeleteAsync(ct);

        var refreshTokenResult = RefreshToken.Create(
            Guid.NewGuid(),
            GenerateRefreshToken(),
            user.UserId,
            DateTime.UtcNow.AddDays(7));

    
        return await refreshTokenResult.ThenDoAsync(async refreshTokenResult => 
        {
            _context.RefreshTokens.Add(refreshTokenResult);
           await _context.SaveChangesAsync(ct);
        }).Then(token =>
        new TokenResponse(
            tokenHandler.WriteToken(securityToken),
            token.Token,
            expires));
        
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!)),
            ValidateIssuer = true,
            ValidIssuer = _configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = _configuration["JwtSettings:Audience"],
            ValidateLifetime = false, // Ignore token expiration
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token.");
        }

        return principal;
    }
    private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    
}
