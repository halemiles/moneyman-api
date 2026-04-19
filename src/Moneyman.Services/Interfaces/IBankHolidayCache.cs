using System.Collections.Generic;

namespace Moneyman.Interfaces
{
    public interface IBankHolidayCache
    {
        IReadOnlyList<string> Holidays { get; }
        void Populate(IEnumerable<string> holidays);
    }
}
