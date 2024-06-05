using System.Collections.Generic;
using Moneyman.Domain;
using Moneyman.Persistence;

namespace Moneyman.Interfaces
{
    public interface IBankAccountRepository : IRepository<BankAccount>
    {
    }
}
