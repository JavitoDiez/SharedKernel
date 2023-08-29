﻿namespace SharedKernel.Domain.Repositories;

/// <summary>  </summary>
public interface IRepository<TAggregateRoot> :
    ICreateRepository<TAggregateRoot>,
    IReadRepository<TAggregateRoot>,
    IUpdateRepository<TAggregateRoot>,
    IDeleteRepository<TAggregateRoot>,
    IReadSpecificationRepository<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot
{
}
