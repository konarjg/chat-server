namespace Infrastructure.Auth;

using System.Security.Cryptography;
using System.Text;
using Domain.Ports.Auth;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Configuration;

public class ArgonPasswordHasher(IConfiguration configuration) : IPasswordHasher {
  private const int SaltSize = 16;
  private const int HashSize = 32;

  public string HashPassword(string password) {
    byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

    Argon2id argon2 = new(passwordBytes) {
      Salt = salt,
      DegreeOfParallelism = int.Parse(configuration["Argon2:DegreeOfParallelism"]),
      MemorySize =  int.Parse(configuration["Argon2:MemorySizeKB"]),
      Iterations = int.Parse(configuration["Argon2:Iterations"])
    };

    byte[] hashBytes = argon2.GetBytes(HashSize);

    return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hashBytes)}";
  }

  public bool VerifyPassword(string password, string hash) {
    string[] parts = hash.Split(':');
    
    if (parts.Length != 2) {
      return false;
    }

    try {
      byte[] salt = Convert.FromBase64String(parts[0]);
      byte[] expectedHash = Convert.FromBase64String(parts[1]);

      byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

      Argon2id argon2 = new(passwordBytes) {
        Salt = salt,
        DegreeOfParallelism = int.Parse(configuration["Argon2:DegreeOfParallelism"]),
        MemorySize = int.Parse(configuration["Argon2:MemorySizeKB"]),
        Iterations = int.Parse(configuration["Argon2:Iterations"])
      };

      byte[] actualHash = argon2.GetBytes(HashSize);

      return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
    catch (FormatException) {
      return false;
    }
  }
}
