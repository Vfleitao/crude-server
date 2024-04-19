using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

using Microsoft.IdentityModel.Tokens;

namespace CrudeServer.Providers
{
    public class JTWAuthenticationProvider : IAuthenticationProvider
    {
        private readonly IServerConfig serverConfig;
        private readonly ILoggerProvider loggerProvider;

        public JTWAuthenticationProvider(IServerConfig serverConfig, ILoggerProvider loggerProvider)
        {
            this.serverConfig = serverConfig;
            this.loggerProvider = loggerProvider;
        }

        public async Task<IPrincipal> GetUser(ICommandContext requestContext)
        {
            try
            {
                if (serverConfig.JTWConfiguration == null)
                {
                    this.loggerProvider.Log("JTW Configuration is missing");

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
                    this.loggerProvider.Log("Authorization Token is not valid");

                    return (IPrincipal)null;
                }

                token = token.Substring(7);

                TokenValidationParameters validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = this.serverConfig.JTWConfiguration.Issuer,
                    ValidateAudience = true,
                    ValidAudience = serverConfig.JTWConfiguration.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(serverConfig.JTWConfiguration.SigningKey)),
                };

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                return tokenHandler.ValidateToken(token, validationParameters, out SecurityToken __);
            }
            catch (Exception ex)
            {
                this.loggerProvider.Error(ex);
            }

            return (IPrincipal)null;
        }
    }
}
