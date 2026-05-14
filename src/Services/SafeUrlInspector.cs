using System.Net;
using System.Net.Sockets;

namespace SecureCodingDemo.Services;

public sealed class SafeUrlInspector
{
    private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "example.com",
        "www.example.com",
        "httpbin.org"
    };

    public async Task<SafeUrlResult> InspectAsync(string url, CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return SafeUrlResult.Rejected("URL must be absolute.");
        }

        if (uri.Scheme != Uri.UriSchemeHttps)
        {
            return SafeUrlResult.Rejected("Only HTTPS is allowed.");
        }

        if (!AllowedHosts.Contains(uri.Host))
        {
            return SafeUrlResult.Rejected("Host is not in the outbound allowlist.");
        }

        var addresses = await Dns.GetHostAddressesAsync(uri.Host, cancellationToken);
        if (addresses.Any(IsPrivateOrLocal))
        {
            return SafeUrlResult.Rejected("Resolved address is private, loopback, link-local or otherwise local.");
        }

        return SafeUrlResult.Allowed(uri);
    }

    private static bool IsPrivateOrLocal(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
        {
            return true;
        }

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            return bytes[0] == 10
                || bytes[0] == 127
                || bytes[0] == 169 && bytes[1] == 254
                || bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31
                || bytes[0] == 192 && bytes[1] == 168;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.Equals(IPAddress.IPv6Loopback);
        }

        return true;
    }
}

public sealed record SafeUrlResult(bool IsAllowed, Uri? Uri, string? Reason)
{
    public static SafeUrlResult Allowed(Uri uri) => new(true, uri, null);

    public static SafeUrlResult Rejected(string reason) => new(false, null, reason);
}
