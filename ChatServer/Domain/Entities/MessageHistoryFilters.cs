namespace Domain.Entities;

public record MessageHistoryFilters(int ChatId,int PageSize,int? LastId = null); 
