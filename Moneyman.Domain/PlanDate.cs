using System;

namespace Moneyman.Domain
{
    public class PlanDate
    {
        public int Id { get; set; }
        public Transaction Transaction { get; set; }
        public DateTime Date { get; set; }
    }
}