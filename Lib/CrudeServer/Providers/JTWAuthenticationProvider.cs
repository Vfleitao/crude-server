using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CrudeServer.Providers
{
    public class JTWAuthenticationProvider : IAuthenticationProvider
    {
        private readonly IOptions<ServerConfiguration> serverConfig;
        private readonly ILogger loggerProvider;
        private readonly IEncryptionProvider encryptionProvider;

        public JTWAuthenticationProvider(
            IOptions<ServerConfiguration> serverConfig,
            ILogger loggerProvider,
            IEncryptionProvider encryptionProvider
        )
        {
            this.serverConfig = serverConfig;
            this.loggerProvider = loggerProvider;
            this.encryptionProvider = encryptionProvider;
        }

        public Task<IPrincipal> GetUserFromHeaders(ICommandContext requestContext)
        {
            return Task.Run<IPrincipal>(() =>
            {
                try
                {
                    if (serverConfig.Value.JTWConfiguration == null)
                    {
                        this.loggerProvider.Log("[JTWAuthenticationProvider] JTW Configuration is missing");

                        return (IPrincipal)null;
                    }

                    if (!requestContext.RequestHeaders.AllKeys.Any(x => x.ToLower() == "authorization"))
                    {
                        return (IPrincipal)null;
                    }

                    string expectedKey = requestContext.RequestHeaders.AllKeys.First(x => x.ToLower() == "authorization");

                    string token = requestContext.RequestHeaders[expectedKey];
                    if (string.IsNullOrEmpty(token) || !token.StartsWith("Bearer"))
                    {
                        this.loggerProvider.Log("[JTWAuthenticationProvider] Authorization Token is not valid");

                        return (IPrincipal)null;
                    }

                    token = token.Substring(7);
                    return GetPrincipalFromTokenString(token);
                }
                catch (Exception ex)
                {
                    this.loggerProvider.Error(ex);
                }

                return (IPrincipal)null;
            });
        }

        public Task<IPrincipal> GetUserFromCookies(ICommandContext requestContext)
        {
            return Task.Run<IPrincipal>(() =>
            {
                if (serverConfig.Value.JTWConfiguration == null)
                {
                    this.loggerProvider.Log("[JTWAuthenticationProvider - GetUserFromCookies] JTW Configuration is missing");

                    return (IPrincipal)null;
                }

                if (string.IsNullOrEmpty(this.serverConfig.Value.JTWConfiguration.CookieName))
                {
                    this.loggerProvider.Log("[JTWAuthenticationProvider - GetUserFromCookies] Cookie name is missing");

                    return (IPrincipal)null;
                }

                string loweredCookieName = this.serverConfig.Value.JTWConfiguration.CookieName.ToLower();
                if (requestContext.RequestCookies == null ||
                    !requestContext.RequestCookies.Any(x => x.Name.ToLower() == loweredCookieName))
                {
                    return (IPrincipal)null;
                }

                HttpCookie cookie = requestContext.RequestCookies.First(x => x.Name.ToLower() == loweredCookieName);

                string encryptedToken = cookie.Value;
                string token = this.encryptionProvider.Decrypt(encryptedToken, this.serverConfig.Value.PrivateEncryptionKey);

                return GetPrincipalFromTokenString(token);
            });
        }

        public Task<string> GenerateToken(IPrincipal principal)
        {
            return Task.Run<string>(() =>
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                ClaimsIdentity identity = principal.Identity as ClaimsIdentity;

                SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = identity,
                    Expires = DateTime.UtcNow.AddMinutes(this.serverConfig.Value.JTWConfiguration.ExpiresAfter),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.serverConfig.Value.JTWConfiguration.SigningKey)),
                        SecurityAlgorithms.HmacSha256Signature
                    ),
                    Issuer = this.serverConfig.Value.JTWConfiguration.Issuer,
                    Audience = this.serverConfig.Value.JTWConfiguration.Audience,
                    IssuedAt = DateTime.UtcNow,
                    NotBefore = DateTime.UtcNow,
                };

                return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
            });
        }

        public async Task<HttpCookie> GenerateTokenCookie(IPrincipal principal)
        {
            string token = await GenerateToken(principal);
            string encryptedToken = this.encryptionProvider.Encrypt(token, this.serverConfig.Value.PublicEncryptionKey);

            return new HttpCookie
            {
                Name = this.serverConfig.Value.JTWConfiguration.CookieName,
                Value = encryptedToken,
                ExpireTimeMinutes = this.serverConfig.Value.JTWConfiguration.ExpiresAfter,
                Secure = true,
                HttpOnly = true
            };
        }

        private IPrincipal GetPrincipalFromTokenString(string token)
        {
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = this.serverConfig.Value.JTWConfiguration.Issuer,
                ValidateAudience = true,
                ValidAudience = serverConfig.Value.JTWConfiguration.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(serverConfig.Value.JTWConfiguration.SigningKey)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ValidateToken(token, validationParameters, out SecurityToken __);
        }
    }
}
