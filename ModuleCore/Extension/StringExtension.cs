using System.Text.RegularExpressions;

namespace ModuleCore.Ex
{
    public static class StringExtension
    {
        public static bool IsIP(this string ip)
        {
            return Regex.IsMatch(ip, @"^([1-9]\d?|1\d{2}|2[01]\d|22[0-3])(\.([1-9]?\d|1\d{2}|2[0-4]\d|25[0-5])){3}$");
        }
    }
}