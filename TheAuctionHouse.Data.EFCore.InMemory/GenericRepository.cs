




using Microsoft.EntityFrameworkCore;
using TheAuctionHouse.Data.EFCore.InMemory;
using TheAuctionHouse.Domain.DataContracts;

namespace SKUApp.Data.EFCore.InMemory;

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly InMemoryAppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    // FIX: Make _context accessible to derived classes
    protected InMemoryAppDbContext Context => _context;

    public GenericRepository(IAppDbContext context)
    {
        if (context is not InMemoryAppDbContext)
            throw new ArgumentException("Invalid context type. Expected InMemoryAppDbContext.");
        _context = (InMemoryAppDbContext)context;
        _dbSet = context.GetDbSet<T>()!;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync(); // FIX: Ensure changes persist
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Attach(entity);
        (_context as DbContext)!.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync(); // FIX: Ensure update is committed
    }

    public virtual async Task DeleteAsync(int id)
    {
        T? entity = await _dbSet.FindAsync(id);
        if (entity == null)
            throw new KeyNotFoundException();

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(); // FIX: Ensure delete is committed
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate)
    {
        List<T>? result = _dbSet.Where(predicate).ToList();
        return await Task.FromResult(result) ?? throw new InvalidOperationException("Entity not found.");
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(); // FIX: Ensure delete is committed
    }
}