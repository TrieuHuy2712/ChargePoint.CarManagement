namespace ChargePoint.CarManagement.Domain.Enums
{
    public static class EnumExtensions
    {
        public static T ToEnum<T>(this string value, bool ignoreCase = true) where T : struct
        {
            return Enum.TryParse<T>(value, ignoreCase, out T result) ? result : default;
        }
    }
}
