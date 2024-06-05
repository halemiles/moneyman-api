using System;

namespace Moneyman.Domain
{
    public class BankAccount : Entity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}