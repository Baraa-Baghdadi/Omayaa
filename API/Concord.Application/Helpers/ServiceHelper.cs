namespace Concord.Application.Helpers
{
    public static class ServiceHelper
    {
        public static long? getTimeSpam(DateTime? dt)
        {
            if (dt.HasValue)
            {
                return ((DateTimeOffset)dt).ToUnixTimeSeconds();
            }
            return null;
        }

    }
}
