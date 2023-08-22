﻿using Microsoft.EntityFrameworkCore;
using SharedKernel.Domain.Aggregates;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SharedKernel.Infrastructure.EntityFrameworkCore.Data.DbContexts;

/// <summary> Shared kernel DbContext. </summary>
public class DbContextBase : DbContext, IQueryableUnitOfWork
{
    #region Members

    private readonly Assembly _assemblyConfigurations;
    private readonly IAuditableService? _auditableService;
    private readonly IValidatableObjectService? _validatableObjectService;

    #endregion

    #region Constructors

    /// <summary> Constructor. </summary>
    public DbContextBase(DbContextOptions options, string schema, Assembly assemblyConfigurations,
        IValidatableObjectService? validatableObjectService, IAuditableService? auditableService) : base(options)
    {
        _assemblyConfigurations = assemblyConfigurations;
        _auditableService = auditableService;
        _validatableObjectService = validatableObjectService;
        Schema = schema;
        // ReSharper disable once VirtualMemberCallInConstructor
        ChangeTracker.LazyLoadingEnabled = false;
    }

    #endregion

    #region Properties

    /// <summary>  </summary>
    public string Schema { get; }

    /// <summary>  </summary>
    public IDbConnection GetConnection => Database.GetDbConnection();

    #endregion

    #region IUnitOfWorkAsync Methods

    /// <summary>  </summary>
    public int Rollback()
    {
        return RollbackAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>  </summary>
    public override int SaveChanges()
    {
        return SaveChangesAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>  </summary>
    public Task<int> SaveChangesAsync()
    {
        return SaveChangesAsync(CancellationToken.None);
    }

    /// <summary>  </summary>
    public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            _validatableObjectService?.ValidateDomainEntities(this);
            _validatableObjectService?.Validate(this);
            _auditableService?.Audit(this);
            return await base.SaveChangesAsync(cancellationToken);
        }
#if !NET462 && !NET47 && !NET471
        catch (DbUpdateException exUpdate)
        {
            throw new Exception(string.Join(", ", exUpdate.Entries.Select(e => e.ToString())), exUpdate);
        }
#endif
        catch (Exception)
        {
            await RollbackAsync(cancellationToken);
            throw;// new SharedKernelInfrastructureException(nameof(ExceptionCodes.EF_CORE_SAVE_CHANGES), ex);
        }
    }

    /// <inheritdoc />
    /// <summary> Rollback all changes. </summary>
    public Task<int> RollbackAsync(CancellationToken cancellationToken)
    {
        var changedEntries = ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged).ToList();

        foreach (var entry in changedEntries)
        {
            switch (entry.State)
            {
                case EntityState.Modified:
                    entry.CurrentValues.SetValues(entry.OriginalValues);
                    entry.State = EntityState.Unchanged;
                    break;
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Unchanged;
                    break;
            }
        }

        return Task.FromResult(changedEntries.Count);
    }

    #endregion

    #region IQueryableUnitOfWork Members

    /// <summary>  </summary>
    public DbSet<TAggregateRoot> SetAggregate<TAggregateRoot>() where TAggregateRoot : class, IAggregateRoot
    {
        return base.Set<TAggregateRoot>();
    }

    /// <summary>  </summary>
    public IQueryable<object> Set(Type type)
    {
        var x = GetType()
            .GetMethod("Set", Type.EmptyTypes)!
            .MakeGenericMethod(type);

        return (IQueryable<object>)x.Invoke(this, null)!;
    }

    #endregion

    #region Protected Methods

    /// <summary>  </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        modelBuilder.ApplyConfigurationsFromAssembly(_assemblyConfigurations);
    }

    #endregion
}
