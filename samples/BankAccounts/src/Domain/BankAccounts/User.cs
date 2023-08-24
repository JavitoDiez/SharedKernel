﻿namespace BankAccounts.Domain.BankAccounts
{
    internal class User : Entity<Guid>
    {
        protected User() { }

        internal User(Guid id, string name, string surname, DateTime birthdate) : base(id)
        {
            Name = name;
            Surname = surname;
            Birthdate = birthdate;
        }

        public string Name { get; private set; }

        public string Surname { get; private set; }

        public DateTime Birthdate { get; private set; }

        public IEnumerable<string> Emails { get; private set; }
    }
}
