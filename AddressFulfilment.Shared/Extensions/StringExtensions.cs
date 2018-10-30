namespace AddressFulfilment.Shared.Extensions
{
    public static class StringExtensions
    {
        public static string TrimTo(this string s, int length)
        {
            return s.Length < length ? s : s.Substring(0, length);
        }
    }
}
