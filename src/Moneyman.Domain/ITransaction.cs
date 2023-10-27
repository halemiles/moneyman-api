using System;
using Moneyman.Domain;

namespace Moneyman.Domain
{
    public class BaseTransaction : Entity
    {
        public string Name {get; set;}
        public decimal Amount {get; set;}
        public bool Active { get; set; }

        public DateTime StartDate { get; set; }
    }
}