namespace Infrastructure.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Entities;
using Domain.Ports.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class TokenGenerator(IConfiguration configuration) : ITokenGenerator {
  private const int RefreshTokenSize = 64;

  public string GenerateAccessToken(User user) {
    JwtSecurityTokenHandler tokenHandler = new();
    byte[] key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);
    double validityMinutes = double.Parse(configuration["Jwt:AccessTokenValidityMinutes"]);

    Claim[] claims = [
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
      new(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
      new(JwtRegisteredClaimNames.Name, user.Name)
    ];

    SecurityTokenDescriptor tokenDescriptor = new() {
      Subject = new ClaimsIdentity(claims),
      Expires = DateTime.UtcNow.AddMinutes(validityMinutes),
      Issuer = configuration["Jwt:Issuer"],
      Audience = configuration["Jwt:Audience"],
      SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
  }

  public RefreshToken GenerateRefreshToken(User user) {
    byte[] randomNumber = RandomNumberGenerator.GetBytes(RefreshTokenSize);
    string token = Convert.ToBase64String(randomNumber);
    double validityDays = double.Parse(configuration["Jwt:RefreshTokenValidityDays"]);

    return new() {
      UserId = user.Id,
      Token = token,
      Expires = DateTime.UtcNow.AddDays(validityDays),
      Revoked = null
    };
  }
}
