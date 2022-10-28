using System.Collections.Generic;
using Moneyman.Domain;

namespace Moneyman.Interfaces
{
    public interface IDtpReaderService
    {
        List<PlanDate> GetCurrent();
    }
}