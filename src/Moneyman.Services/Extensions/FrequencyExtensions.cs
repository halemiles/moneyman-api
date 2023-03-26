using Moneyman.Domain;

namespace Moneyman.Services.Extentions
{
    public static class FrequencyExtensions
    {
        public static int ToFrequencyCount(this Frequency frequency)
        {
            int count = 0;

            switch(frequency)
            {
                case Frequency.Daily:
                    count = 365;
                    break;
                case Frequency.Weekly:
                    count = 52;
                    break;
                case Frequency.Monthly:
                    count = 12;
                    break;
                case Frequency.Yearly:
                    count = 1;
                    break;
            }

            return count;
        }
    }
}