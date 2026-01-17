using System.Linq.Expressions;
using LoanApplication.Domain.Entities;

namespace LoanApplication.Application.Common.Contracts.Abstractions.Repositories;

public interface IRepository<TEntity> where TEntity : EntityBase
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity, params Expression<Func<TEntity, object>>[] updatedProperties);
}