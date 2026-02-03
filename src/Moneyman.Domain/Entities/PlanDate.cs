using System;
using Moneyman.Domain.Interfaces;

namespace Moneyman.Domain
{
    public class PlanDate : IEntity
    {
        public int Id { get; set; }
        public Transaction Transaction { get; set; }
        public DateTime Date { get; set; }
        public bool Active { get; set; }
        public DateTime OriginalDate { get; set; }
    }
}