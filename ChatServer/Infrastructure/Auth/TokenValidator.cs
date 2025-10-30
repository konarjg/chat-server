namespace Infrastructure.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Entities;
using Domain.Ports.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class JwtTokenValidator(IConfiguration configuration) : ITokenValidator {
  public AuthenticatedUser? ValidateToken(string accessToken) {
    ClaimsPrincipal? claimsPrincipal = ValidateAndDecodeJwt(accessToken);

    if (claimsPrincipal is null) {
      return null;
    }

    Claim? userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
    Claim? userNameClaim = claimsPrincipal.FindFirst(ClaimTypes.Name);

    if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out int userId)) {
      return null;
    }

    return new AuthenticatedUser(userId, userNameClaim?.Value ?? string.Empty);
  }
  
  private ClaimsPrincipal? ValidateAndDecodeJwt(string token) {
    if (string.IsNullOrEmpty(token)) {
      return null;
    }

    JwtSecurityTokenHandler tokenHandler = new();
    byte[] key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);

    try {
      TokenValidationParameters validationParameters = new() {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = configuration["Jwt:Audience"],
        ClockSkew = TimeSpan.Zero
      };

      ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
      return principal;
    }
    catch (Exception) {
      return null;
    }
  }
}
