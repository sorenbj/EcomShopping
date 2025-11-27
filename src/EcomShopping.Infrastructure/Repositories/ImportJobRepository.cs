using EcomShopping.Domain.Entities;
using EcomShopping.Domain.Interfaces;
using EcomShopping.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EcomShopping.Infrastructure.Repositories;

public class ImportJobRepository : IImportJobRepository
{
    private readonly ApplicationDbContext _context;

    public ImportJobRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ImportJob?> GetByIdAsync(int id)
    {
        return await _context.ImportJobs.FindAsync(id);
    }

    public async Task<IEnumerable<ImportJob>> GetAllAsync()
    {
        return await _context.ImportJobs
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }

    public async Task<ImportJob> AddAsync(ImportJob entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _context.ImportJobs.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(ImportJob entity)
    {
        _context.ImportJobs.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var job = await _context.ImportJobs.FindAsync(id);
        if (job != null)
        {
            _context.ImportJobs.Remove(job);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<ImportJob>> GetRecentJobsAsync(int count)
    {
        return await _context.ImportJobs
            .OrderByDescending(j => j.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<ImportJob>> GetByStatusAsync(ImportJobStatus status)
    {
        return await _context.ImportJobs
            .Where(j => j.Status == status)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
    }
}
