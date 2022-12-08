using System.Collections.Generic;
using Moneyman.Domain;
using Moneyman.Models.Dtos;

namespace Moneyman.Interfaces
{
    public interface IDtpReaderService
    {
        DtpDto GetCurrent();
    }
}