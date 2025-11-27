using EcomShopping.Domain.Entities;

namespace EcomShopping.Domain.Interfaces;

public interface IImportJobRepository : IRepository<ImportJob>
{
    Task<IEnumerable<ImportJob>> GetRecentJobsAsync(int count);
    Task<IEnumerable<ImportJob>> GetByStatusAsync(ImportJobStatus status);
    Task<ImportJob?> GetJobWithDetailsAsync(int id);
}
