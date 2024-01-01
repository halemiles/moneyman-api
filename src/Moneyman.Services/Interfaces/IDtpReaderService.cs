using System.Collections.Generic;
using Moneyman.Domain;
using Moneyman.Domain.Models;
using Moneyman.Models.Dtos;

namespace Moneyman.Interfaces
{
    public interface IDtpReaderService
    {
        ApiResponse<DtpDto> GetCurrent();
        DtpDto GetOffset(int? monthOffset);
    }
}