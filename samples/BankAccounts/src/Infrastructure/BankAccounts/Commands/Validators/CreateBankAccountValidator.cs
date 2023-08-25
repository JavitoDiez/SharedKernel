﻿using BankAccounts.Application.BankAccounts.Commands;
using BankAccounts.Domain.BankAccounts;
using BankAccounts.Domain.BankAccounts.Repository;

namespace BankAccounts.Infrastructure.BankAccounts.Commands.Validators
{
    internal class CreateBankAccountValidator : AbstractValidator<CreateBankAccount>
    {
        public CreateBankAccountValidator(IBankAccountRepository bankAccountRepository)
        {
            RuleFor(e => e.Id)
                .NotEmpty()
                .MustAsync(async (prop, c) => !await bankAccountRepository.AnyAsync(BankAccountId.Create(prop), c));

            RuleFor(e => e.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(e => e.Surname)
                .MinimumLength(1)
                .MaximumLength(100);
        }
    }
}
