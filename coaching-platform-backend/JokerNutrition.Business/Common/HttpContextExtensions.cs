using System.Net;
using Microsoft.AspNetCore.Http;

namespace JokerNutrition.Business.Common;

public static class HttpContextExtensions
{
    /// <summary>
    /// Extract the actual client IP address considering proxy headers (CF-Connecting-IP, X-Real-IP, X-Forwarded-For).
    /// Sanitizes ports, validates IP formatting, normalizes IPv4-mapped IPv6 addresses, and safely parses 
    /// X-Forwarded-For from right-to-left to prevent IP spoofing.
    /// </summary>
    public static string? GetClientIpAddress(this HttpContext? context)
    {
        if (context == null) return null;

        // 1. Cloudflare header (highest priority, stripped/overwritten by Cloudflare edge)
        if (context.Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp) && !string.IsNullOrWhiteSpace(cfIp))
        {
            var cleanIp = CleanAndValidateIp(cfIp.ToString());
            if (cleanIp != null) return cleanIp;
        }

        // 2. Render header (set & overwritten by Render edge proxy)
        if (context.Request.Headers.TryGetValue("Render-Client-IP", out var renderIp) && !string.IsNullOrWhiteSpace(renderIp))
        {
            var cleanIp = CleanAndValidateIp(renderIp.ToString());
            if (cleanIp != null) return cleanIp;
        }

        // 3. X-Real-IP header (set by reverse proxies such as Nginx)
        if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp) && !string.IsNullOrWhiteSpace(realIp))
        {
            var cleanIp = CleanAndValidateIp(realIp.ToString());
            if (cleanIp != null) return cleanIp;
        }

        // 3. Standard X-Forwarded-For header ("client, proxy1, proxy2")
        // Traverse from right-to-left to prevent header-prepending spoofing attacks,
        // prioritizing the outermost public IP or falling back to the last valid IP.
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor) && !string.IsNullOrWhiteSpace(forwardedFor))
        {
            var ips = forwardedFor.ToString().Split(',');
            string? fallbackIp = null;

            for (int i = ips.Length - 1; i >= 0; i--)
            {
                var cleanIp = CleanAndValidateIp(ips[i]);
                if (cleanIp != null)
                {
                    fallbackIp ??= cleanIp;
                    // Skip internal proxy/loopback addresses to extract the real client IP
                    if (!IsPrivateOrLoopback(cleanIp))
                    {
                        return cleanIp;
                    }
                }
            }

            if (fallbackIp != null) return fallbackIp;
        }

        // 4. Fallback to direct RemoteIpAddress (populated by ASP.NET Core UseForwardedHeaders middleware)
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp == null) return null;

        if (remoteIp.IsIPv4MappedToIPv6)
        {
            remoteIp = remoteIp.MapToIPv4();
        }

        return remoteIp.ToString();
    }

    private static bool IsPrivateOrLoopback(string ipString)
    {
        if (!IPAddress.TryParse(ipString, out var ip)) return false;
        if (IPAddress.IsLoopback(ip)) return true;

        byte[] bytes = ip.GetAddressBytes();
        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            // 10.0.0.0/8
            if (bytes[0] == 10) return true;
            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;
            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168) return true;
            // 169.254.0.0/16 (Link-local)
            if (bytes[0] == 169 && bytes[1] == 254) return true;
        }
        else if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal) return true;
            // Unique local address fc00::/7
            if ((bytes[0] & 0xfe) == 0xfc) return true;
        }

        return false;
    }

    private static string? CleanAndValidateIp(string rawIp)
    {
        if (string.IsNullOrWhiteSpace(rawIp)) return null;

        var trimmed = rawIp.Trim();

        // Handle IPv6 with port: "[2001:db8::1]:8080"
        if (trimmed.StartsWith("[") && trimmed.Contains("]"))
        {
            var endBracketIndex = trimmed.IndexOf(']');
            trimmed = trimmed.Substring(1, endBracketIndex - 1);
        }
        // Handle IPv4 with port: "192.168.1.1:8080" (must contain exactly one colon)
        else if (trimmed.Contains(":") && trimmed.IndexOf(":") == trimmed.LastIndexOf(":"))
        {
            trimmed = trimmed.Split(':')[0];
        }

        if (IPAddress.TryParse(trimmed, out var parsedIp))
        {
            if (parsedIp.IsIPv4MappedToIPv6)
            {
                parsedIp = parsedIp.MapToIPv4();
            }
            return parsedIp.ToString();
        }

        return null;
    }
}

