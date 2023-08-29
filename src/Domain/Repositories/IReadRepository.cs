﻿namespace SharedKernel.Domain.Repositories;

/// <summary>  </summary>
public interface IReadRepository<out TAggregate> where TAggregate : IAggregateRoot
{
    /// <summary>  </summary>
    TAggregate? GetById<TId>(TId key);

    /// <summary>  </summary>
    bool Any();

    /// <summary>  </summary>
    bool NotAny();

    /// <summary>  </summary>
    bool Any<TId>(TId key);

    /// <summary>  </summary>
    bool NotAny<TId>(TId key);
}
