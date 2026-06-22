using Moneyman.Domain.MapperProfiles;
using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;

namespace Moneyman.Services
{
	public class BankAccountService : IBankAccountService
	{
    private readonly IBankAccountRepository bankAccountRepository;
    private readonly BankAccountMapper bankAccountMapper;
    private readonly ILogger<BankAccountService> logger;

		public BankAccountService(
      IBankAccountRepository bankAccountRepository,
      ILogger<BankAccountService> logger,
      BankAccountMapper bankAccountMapper
    )
		{
			this.bankAccountRepository = bankAccountRepository;
      this.logger = logger;
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
