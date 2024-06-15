using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using Moneyman.Domain;
using Moneyman.Services.Validators;
using AutoMapper;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moneyman.Domain.Models;

namespace Moneyman.Services
{
	public class BankAccountService : IBankAccountService
	{
		private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<TransactionService> logger;
    private readonly IBankAccountRepository bankAccountRepository;
    private readonly IMapper mapper;

		public BankAccountService(
      IBankAccountRepository bankAccountRepository,
      ILogger<TransactionService> logger,
      IMapper mapper
    )
		{
			this.bankAccountRepository = bankAccountRepository;
      this.logger = logger;
      this.mapper = mapper;
		}
    public List<BankAccountDto> GetAll()
    {
        var bankAccounts =  bankAccountRepository.GetAll();
        var bankAccountDto = mapper.Map<List<BankAccountDto>>(bankAccounts);
        return bankAccountDto?.ToList() ?? new List<BankAccountDto>();
    }
  }
}
