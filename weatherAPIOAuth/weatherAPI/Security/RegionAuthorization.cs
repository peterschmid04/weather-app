using System.Security.Claims;

namespace weatherAPI.Security;

public static class RegionAuthorization
{
    private static readonly HashSet<string> Europe = new(StringComparer.OrdinalIgnoreCase)
    {
        "AL","AD","AM","AT","AZ","BY","BE","BA","BG","CH","CY","CZ","DE","DK","EE","ES",
        "FI","FO","FR","GB","GE","GI","GR","HR","HU","IE","IM","IS","IT","LI","LT","LU",
        "LV","MC","MD","ME","MK","MT","NL","NO","PL","PT","RO","RS","RU","SE","SI","SK",
        "SM","TR","UA","VA"
    };

    public static bool IsCountryAllowed(ClaimsPrincipal user, string countryCode)
    {
        var perms = user.FindAll("permissions").Select(p => p.Value).ToHashSet();
        if (!perms.Any() && countryCode.Equals("DE", StringComparison.OrdinalIgnoreCase)) return true;
        if (perms.Contains("region:all")) return true;
        if (perms.Contains("region:eu") && Europe.Contains(countryCode)) return true;
        return false;
    }
}