using System;
using Moneyman.Domain;

namespace Moneyman.Tests.Builders
{
    public class TransactionBuilder
    {
        private Transaction _transaction;

        public TransactionBuilder()
        {
            _transaction = GenerateDefaultTransaction();
        }

        public TransactionBuilder(Transaction transaction)
        {
            _transaction = transaction;
        }

        private Transaction GenerateDefaultTransaction()
        {
            return new Transaction()
            {
                Id = 0,
                Name = "Transaction 1",
                StartDate = DateTime.Today,
                Amount = 0,
                Active = true,
                Frequency = Frequency.Monthly
                
            };
        }

        public Transaction Build()
        {
            return _transaction;
        }

        public TransactionBuilder WithId(int id)
        {
            _transaction.Id = id;
            return this;
        }

        public TransactionBuilder WithName(string name)
        {
            _transaction.Name = name;
            return this;
        }

        public TransactionBuilder WithAmount(decimal amount)
        {
            _transaction.Amount = amount;
            return this;
        }

        public TransactionBuilder WithFrequency(Frequency frequency)
        {
            _transaction.Frequency = frequency;
            return this;
        }

        public TransactionBuilder WithStartDate(DateTime startDate)
        {
            _transaction.StartDate = startDate;
            return this;
        }

        public TransactionBuilder WithActive(bool active)
        {
            _transaction.Active = active;
            return this;
        }
    }
}