using Riok.Mapperly.Abstractions;

namespace Moneyman.Domain
{
    [Mapper]
    public partial class BankAccountMapper
    {
        public partial BankAccountDto ToDto(BankAccount entity);
        public partial BankAccount ToEntity(BankAccountDto dto);
    }
}
