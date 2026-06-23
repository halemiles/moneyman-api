using Moneyman.Domain.MapperProfiles;
using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Moneyman.Domain;

namespace Moneyman.Services
{
	public class BankAccountService : IBankAccountService
	{
    private readonly IBankAccountRepository bankAccountRepository;
    private readonly BankAccountMapper bankAccountMapper;

		public BankAccountService(
      IBankAccountRepository bankAccountRepository,
      BankAccountMapper bankAccountMapper
    )
		{
			this.bankAccountRepository = bankAccountRepository;
      this.bankAccountMapper = bankAccountMapper;
		}
    public List<BankAccountDto> GetAll()
    {
        var bankAccounts = bankAccountRepository.GetAll();
        var bankAccountDto = bankAccounts.Select(bankAccountMapper.ToDto).ToList();
        return bankAccountDto;
    }
  }
}
