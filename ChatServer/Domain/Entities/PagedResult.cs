namespace Domain.Entities;

public record PagedResult<T>(IEnumerable<T> Items);
