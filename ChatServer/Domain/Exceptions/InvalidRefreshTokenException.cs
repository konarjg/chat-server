namespace Domain.Exceptions;

public class InvalidRefreshTokenException(string message) : Exception(message) {
  
}
