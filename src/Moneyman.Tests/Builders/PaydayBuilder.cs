using System;
using Moneyman.Domain;

namespace Moneyman.Tests.Builders
{
    public class PaydayBuilder
    {
        private readonly Payday _transaction;

        public PaydayBuilder()
        {
            _transaction = GenerateDefaultPayday();
        }

        public PaydayBuilder(Payday transaction)
        {
            _transaction = transaction;
        }

        private Payday GenerateDefaultPayday()
        {
            return new Payday
            {
                Id = 0,
                Date = DateTime.Today                
            };
        }

        public Payday Build()
        {
            return _transaction;
        }

        public PaydayBuilder WithId(int id)
        {
            _transaction.Id = id;
            return this;
        }

        public PaydayBuilder WithDate(DateTime date)
        {
            _transaction.Date = date;
            return this;
        }
    }
}