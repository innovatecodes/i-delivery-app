using System;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        var FormattedPhoneRegex = new Regex(@"^\([0-9]{2}\) (?:9[0-9]{4}-[0-9]{4}|[2-5][0-9]{3}-[0-9]{4})$", RegexOptions.Compiled);
        var UnformattedPhoneRegex = new Regex(@"^[0-9]{2}(?:9[0-9]{8}|[2-5][0-9]{7})$", RegexOptions.Compiled);

        var test1 = "(11) 99999-9999";
        var test2 = "(11) 88888-8888";

        Console.WriteLine($"Formatted test1: {FormattedPhoneRegex.IsMatch(test1)}");
        Console.WriteLine($"Unformatted test1: {UnformattedPhoneRegex.IsMatch(test1)}");
        Console.WriteLine($"Formatted test2: {FormattedPhoneRegex.IsMatch(test2)}");
        Console.WriteLine($"Unformatted test2: {UnformattedPhoneRegex.IsMatch(test2)}");

        // Also check area codes
        var ValidAreaCodes = new System.Collections.Generic.HashSet<string> { "11", "12", "13", "14", "15", "16", "17", "18", "19", "21", "22", "24", "27", "28", "31", "32", "33", "34", "35", "37", "38", "41", "42", "43", "44", "45", "46", "47", "48", "49", "51", "53", "54", "55", "61", "62", "63", "64", "65", "66", "67", "68", "69", "71", "73", "74", "75", "77", "79", "81", "82", "83", "84", "85", "86", "87", "88", "89", "91", "92", "93", "94", "95", "96", "97", "98", "99" };

        var normalized1 = Regex.Replace(test1, @"[\s()-]", "");
        var areaCode1 = normalized1[..2];
        Console.WriteLine($"Normalized: {normalized1}, AreaCode: {areaCode1}, Valid: {ValidAreaCodes.Contains(areaCode1)}");
    }
}