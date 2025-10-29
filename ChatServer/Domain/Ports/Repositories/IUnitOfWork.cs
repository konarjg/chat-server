namespace Domain.Ports.Repositories;

public interface IUnitOfWork {
  Task CompleteAsync();
}
