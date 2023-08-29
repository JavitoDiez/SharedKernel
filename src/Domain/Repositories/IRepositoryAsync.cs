﻿namespace SharedKernel.Domain.Repositories;

/// <summary> An asynchronous generic repository pattern with all crud actions. </summary>
public interface IRepositoryAsync<TAggregateRoot> :
    IRepository<TAggregateRoot>,
    ICreateRepositoryAsync<TAggregateRoot>,
    IReadRepositoryAsync<TAggregateRoot>,
    IUpdateRepositoryAsync<TAggregateRoot>,
    IDeleteRepositoryAsync<TAggregateRoot>,
    IReadSpecificationRepositoryAsync<TAggregateRoot>
    where TAggregateRoot : class, IAggregateRoot
{
}
