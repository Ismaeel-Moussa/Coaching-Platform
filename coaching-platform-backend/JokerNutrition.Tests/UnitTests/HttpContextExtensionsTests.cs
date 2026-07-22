using System.Net;
using JokerNutrition.Business.Common;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace JokerNutrition.Tests.UnitTests;

public class HttpContextExtensionsTests
{
    [Fact]
    public void GetClientIpAddress_ReturnsNull_WhenContextIsNull()
    {
        HttpContext? context = null;
        var result = context.GetClientIpAddress();
        Assert.Null(result);
    }

    [Fact]
    public void GetClientIpAddress_PrioritizesCloudflareHeader()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["CF-Connecting-IP"] = "198.51.100.1";
        context.Request.Headers["X-Real-IP"] = "198.51.100.2";
        context.Request.Headers["X-Forwarded-For"] = "198.51.100.3";

        var result = context.GetClientIpAddress();
        Assert.Equal("198.51.100.1", result);
    }

    [Fact]
    public void GetClientIpAddress_UsesRenderClientIp_WhenCloudflareMissing()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["Render-Client-IP"] = "198.51.100.15";
        context.Request.Headers["X-Real-IP"] = "198.51.100.2";
        context.Request.Headers["X-Forwarded-For"] = "198.51.100.3";

        var result = context.GetClientIpAddress();
        Assert.Equal("198.51.100.15", result);
    }

    [Fact]
    public void GetClientIpAddress_UsesXRealIp_WhenCloudflareMissing()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Real-IP"] = "198.51.100.2";
        context.Request.Headers["X-Forwarded-For"] = "198.51.100.3";

        var result = context.GetClientIpAddress();
        Assert.Equal("198.51.100.2", result);
    }

    [Fact]
    public void GetClientIpAddress_ExtractsPublicIpFromXForwardedFor_IgnoringInternalProxies()
    {
        var context = new DefaultHttpContext();
        // Attacker fake IP: 1.1.1.1, Real public client: 203.0.113.195, Internal proxy: 192.168.1.1
        context.Request.Headers["X-Forwarded-For"] = "1.1.1.1, 203.0.113.195, 192.168.1.1";

        var result = context.GetClientIpAddress();
        Assert.Equal("203.0.113.195", result);
    }

    [Fact]
    public void GetClientIpAddress_StripsPortFromIPv4()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = "198.51.100.55:54321";

        var result = context.GetClientIpAddress();
        Assert.Equal("198.51.100.55", result);
    }

    [Fact]
    public void GetClientIpAddress_StripsPortFromIPv6()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = "[2001:db8::1]:8080";

        var result = context.GetClientIpAddress();
        Assert.Equal("2001:db8::1", result);
    }

    [Fact]
    public void GetClientIpAddress_NormalizesIPv4MappedIPv6()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = "::ffff:192.0.2.1";

        var result = context.GetClientIpAddress();
        Assert.Equal("192.0.2.1", result);
    }

    [Fact]
    public void GetClientIpAddress_IgnoresInvalidIpStringsAndFallsBackToNext()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Forwarded-For"] = "invalid-ip-string, 198.51.100.99";

        var result = context.GetClientIpAddress();
        Assert.Equal("198.51.100.99", result);
    }

    [Fact]
    public void GetClientIpAddress_FallsBackToRemoteIpAddress_WhenHeadersAreMissing()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.50");

        var result = context.GetClientIpAddress();
        Assert.Equal("203.0.113.50", result);
    }
}
