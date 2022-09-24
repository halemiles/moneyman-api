using System.Collections.Generic;
using Moneyman.Domain;

namespace Moneyman.Interfaces
{
    public interface IWeekdayService
    {
        List<WeekDay> GenerateWeekdays(); 
    }
}