﻿using Microsoft.EntityFrameworkCore;
using SharedKernel.Infrastructure.EntityFrameworkCore.Data.DbContexts;

namespace SharedKernel.Integration.Tests.Data.EntityFrameworkCore.DbContexts;

public class SharedKernelDbContext : DbContextBase, ISharedKernelUnitOfWork
{
    public SharedKernelDbContext(DbContextOptions<SharedKernelDbContext> options,
        IValidatableObjectService? validatableObjectService = default, IAuditableService? auditable = default)
        : base(options, "skr", typeof(SharedKernelDbContext).Assembly, validatableObjectService, auditable)
    {
    }
}
