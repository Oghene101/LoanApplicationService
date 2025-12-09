using System.Linq.Expressions;
using CharityDonationsApp.Application.Common.Contracts.Abstractions.Repositories;
using LoanApplication.Domain.Entities;
using LoanApplication.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace LoanApplication.Infrastructure.Persistence.Repositories;

public class Repository<TEntity>(
    AppDbContext context) : IRepository<TEntity> where TEntity : EntityBase
{
    private readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public async Task<TEntity?> FindAsync(object id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync([id], cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity, params Expression<Func<TEntity, object>>[] updatedProperties)
    {
        context.Attach(entity);

        foreach (var property in updatedProperties)
        {
            context.Entry(entity).Property(property).IsModified = true;
        }
    }
}