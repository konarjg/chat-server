namespace Infrastructure.Repositories;

using Domain.Ports.Repositories;

public class UnitOfWork(ChatDatabaseContext databaseContext) : IUnitOfWork{

  public async Task CompleteAsync() {
    await databaseContext.SaveChangesAsync();
  }
}
