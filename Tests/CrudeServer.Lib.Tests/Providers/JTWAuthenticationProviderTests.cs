using System;
using System.Collections.Specialized;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers;
using CrudeServer.Providers.Contracts;

using Microsoft.IdentityModel.Tokens;

using Moq;

namespace CrudeServer.Lib.Tests.Providers
{
    public class JTWAuthenticationProviderTests
    {
        [Test]
        public async Task UserCanBeRetrievedFromJtwToken()
        {
            // Arrange
            JTWConfig jTWConfig = new JTWConfig
            {
                Audience = "audience",
                Issuer = "issuer",
                ExpiresAfter = 1000,
                SigningKey = "{8A7C5026-4348-4E5D-958E-B2052FFA3DB4}"
            };

            ServerConfig serverConfig = new ServerConfig
            {
                JTWConfiguration = jTWConfig
            };

            DateTime expiration = DateTime.UtcNow.AddMinutes(jTWConfig.ExpiresAfter);

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            ClaimsIdentity identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test"),
                new Claim(ClaimTypes.Role, "admin")
            });

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = expiration,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jTWConfig.SigningKey)),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = jTWConfig.Issuer,
                Audience = jTWConfig.Audience,
            };

            string token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            JTWAuthenticationProvider authenticationProvider = new JTWAuthenticationProvider(
                serverConfig,
                Mock.Of<ILogger>()
            );

            NameValueCollection nameValueCollection = new NameValueCollection();
            nameValueCollection.Add("Authorization", $"Bearer {token}");

            Mock<ICommandContext> requestContext = new Mock<ICommandContext>();
            requestContext
                .Setup(x => x.RequestHeaders)
                .Returns(nameValueCollection);

            // Act
            IPrincipal principal = await authenticationProvider.GetUser(requestContext.Object);

            // Assert
            Assert.That(principal, Is.Not.Null);
            Assert.That(principal.Identity.Name, Is.EqualTo("test"));
            Assert.That(principal.IsInRole("admin"), Is.True);
        }
    }
}
