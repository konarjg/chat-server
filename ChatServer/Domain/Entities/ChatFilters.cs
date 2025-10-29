namespace Domain.Entities;

public record ChatFilters(int UserId, int PageSize, int? LastId = null);
