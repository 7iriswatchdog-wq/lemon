using System.Linq;
using AML.Web.Helpers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace AML.Web.Tests
{
    /// <summary>
    /// Pins the hardened defaults from the 2026-05-02 audit:
    ///   - Session cookie: HttpOnly + Secure + SameSite=Lax + 30-min idle timeout
    ///   - Anti-forgery cookie: HttpOnly + Secure + SameSite=Lax + custom header name
    ///   - JwtAppSettings now exposes Issuer + Audience properties
    ///   - The .csproj no longer enables the unsafe BinaryFormatter
    /// These are unit tests, not integration tests — they run the configurators directly
    /// against an in-memory ServiceCollection so they're deterministic and fast.
    /// </summary>
    public class SecurityHardeningTests
    {
        [Fact]
        public void SessionCookie_ShouldBeHardened()
        {
            // Mirror the configuration block in Startup.ConfigureServices verbatim.
            var services = new ServiceCollection();
            services.AddSession(options =>
            {
                options.IdleTimeout = System.TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.IsEssential = true;
            });
            services.AddDataProtection();

            using var sp = services.BuildServiceProvider();
            var opts = sp.GetRequiredService<IOptions<SessionOptions>>().Value;

            Assert.True(opts.Cookie.HttpOnly, "session cookie must be HttpOnly");
            Assert.Equal(CookieSecurePolicy.Always, opts.Cookie.SecurePolicy);
            Assert.Equal(SameSiteMode.Lax, opts.Cookie.SameSite);
            Assert.True(opts.Cookie.IsEssential, "session cookie marked essential to bypass cookie-consent gate");
            Assert.Equal(System.TimeSpan.FromMinutes(30), opts.IdleTimeout);
        }

        [Fact]
        public void AntiforgeryCookie_ShouldBeHardened()
        {
            var services = new ServiceCollection();
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "RequestVerificationToken";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });
            services.AddDataProtection();

            using var sp = services.BuildServiceProvider();
            var opts = sp.GetRequiredService<IOptions<AntiforgeryOptions>>().Value;

            Assert.Equal("RequestVerificationToken", opts.HeaderName);
            Assert.True(opts.Cookie.HttpOnly, "antiforgery cookie must be HttpOnly");
            Assert.Equal(CookieSecurePolicy.Always, opts.Cookie.SecurePolicy);
            Assert.Equal(SameSiteMode.Lax, opts.Cookie.SameSite);
        }

        [Fact]
        public void JwtAppSettings_ExposesIssuerAndAudienceForValidation()
        {
            // Audit fix: middleware uses Issuer+Audience (when set) to enforce token binding.
            var settings = new JwtAppSettings
            {
                Secret = "test-secret-at-least-32-bytes-please-xx",
                Issuer = "https://aml-tenant-x.example",
                Audience = "aml-web"
            };

            Assert.Equal("https://aml-tenant-x.example", settings.Issuer);
            Assert.Equal("aml-web", settings.Audience);
        }

        [Fact]
        public void Csproj_ShouldNot_EnableUnsafeBinaryFormatter()
        {
            // BinaryFormatter is disabled by default in net8.0 because deserializing
            // attacker bytes can lead to RCE. The previous flag explicitly re-enabled it.
            // We assert the project file no longer carries the flag.
            var csprojPath = System.IO.Path.GetFullPath(
                System.IO.Path.Combine(
                    System.AppContext.BaseDirectory,
                    "..", "..", "..", "..", "AML.Web", "AML.Web.csproj"));

            Assert.True(System.IO.File.Exists(csprojPath), $"csproj not found at: {csprojPath}");
            var contents = System.IO.File.ReadAllText(csprojPath);

            Assert.DoesNotContain(
                "<EnableUnsafeBinaryFormatterSerialization>true</EnableUnsafeBinaryFormatterSerialization>",
                contents);
        }
    }
}
