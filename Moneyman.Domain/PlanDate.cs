using System;

namespace Moneyman.Domain
{
    public class PlanDate : IEntity
    {
        public int Id { get; set; }
        public Transaction Transaction { get; set; }
        public DateTime Date { get; set; }
        public bool Active { get; set; }
        public DateTime OriginalDate { get; set; }
        public int YearGroup { get; set; } //? TODO - Do we need this? Could this be in a DTO
        public int MonthGroup { get; set; } //? TODO - Do we need this? Could this be in a DTO
        public bool IsAnticipated { get; set; } //TODO - Remove this as we don't currently do anticipated
        public int OrderId {get; set;} //? TODO - Do we need this? Could this be in a DTO
    }
}