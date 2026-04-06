using System;

namespace Moneyman.Domain
{
    public class PlanDate : Entity
    {
        public Transaction Transaction { get; set; }
        public DateTime Date { get; set; }
        public bool Active { get; set; }
        public DateTime OriginalDate { get; set; }
    }
}