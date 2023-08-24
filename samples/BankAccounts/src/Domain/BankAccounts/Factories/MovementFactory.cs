﻿namespace BankAccounts.Domain.BankAccounts.Factories
{
    internal static class MovementFactory
    {
        public static Result<Movement> CreateMovement(Guid id, string concept, decimal amount, DateTime date)
        {
            return new Movement(id, concept, amount, date);
        }
    }
}
