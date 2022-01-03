using System;

namespace Moneyman.Domain
{
    public class Payday : Entity
    {
        public int Id { get; set; }
        public DateTime Date {get; set;}
    }
}