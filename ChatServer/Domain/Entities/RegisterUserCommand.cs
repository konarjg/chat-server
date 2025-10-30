namespace Domain.Entities;

public record RegisterUserCommand(string Name, string Password, string PublicKey);
