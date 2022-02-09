﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace BankAccounts.Domain.BankAccounts.Repository
{
    public interface IBankAccountRepository
    {
        Task Add(BankAccount bankAccount, CancellationToken cancellationToken = default);

        Task Update(BankAccount bankAccount, CancellationToken cancellationToken = default);

        Task<BankAccount> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
