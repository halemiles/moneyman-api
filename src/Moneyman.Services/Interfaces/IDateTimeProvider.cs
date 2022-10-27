using System;

namespace Moneyman.Interfaces
{
    //Allows for mocking
    public interface IDateTimeProvider
    {
        DateTime GetToday();
        DateTime GetNow();
    }
}