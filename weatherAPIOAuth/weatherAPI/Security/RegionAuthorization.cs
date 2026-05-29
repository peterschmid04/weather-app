using System.Security.Claims;

namespace weatherAPI.Security;

public static class RegionAuthorization
{
    private const string RegionAllPermission = "region:all";
    private const string RegionEuropePermission = "region:eu";

    private static readonly HashSet<string> Europe = new(StringComparer.OrdinalIgnoreCase)
    {
        "AL","AD","AM","AT","AZ","BY","BE","BA","BG","CH","CY","CZ","DE","DK","EE","ES",
        "FI","FO","FR","GB","GE","GI","GR","HR","HU","IE","IM","IS","IT","LI","LT","LU",
        "LV","MC","MD","ME","MK","MT","NL","NO","PL","PT","RO","RS","RU","SE","SI","SK",
        "SM","TR","UA","VA"
    };

    public static bool IsCountryAllowed(ClaimsPrincipal user, string countryCode)
    {
        var perms = GetPermissions(user).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!perms.Any() && countryCode.Equals("DE", StringComparison.OrdinalIgnoreCase)) return true;
        if (perms.Contains(RegionAllPermission)) return true;
        if (perms.Contains(RegionEuropePermission) && Europe.Contains(countryCode)) return true;
        return false;
    }

    public static IReadOnlyCollection<string> GetPermissions(ClaimsPrincipal user) =>
        user.FindAll("permissions")
            .Select(permission => permission.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value)
            .ToList();

    public static string GetRegionScope(ClaimsPrincipal user)
    {
        var permissions = GetPermissions(user).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (permissions.Contains(RegionAllPermission))
        {
            return "Alle Regionen";
        }

        if (permissions.Contains(RegionEuropePermission))
        {
            return "Europa";
        }

        return "Deutschland";
    }

    public static IReadOnlyCollection<string> GetAllowedRegionLabels(ClaimsPrincipal user) =>
        GetRegionScope(user) switch
        {
            "Alle Regionen" => ["Weltweit"],
            "Europa" => ["Europa", "Deutschland"],
            _ => ["Deutschland"]
        };
}
