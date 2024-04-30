using System;
using System.Collections.Generic;
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
        private const string PUBLIC_KEY = "BgIAAACkAABSU0ExABAAAAEAAQAVnUUmp57hi6ADkKYcHD/n4NTzSSB/wFRNlJXuihB4+Pr+MKiCre1XDdQZCSY3yizSppAlTRW+XxfQym8xju32DviQpnz47KSj61HMLMYduVXSo/Yg1hyMn7jWY1m7qUfkY7q64ZzNzQFPDgZcErBTU3JYOqKynX9wgnEFoAbIANKFdqHZczA+Zaj7rwhQlSiOF66B4T7qrxeOU1OR6QhBb7x3/U0eeSr/Dp35dB3f3btxkgpqruB1Uhtb3VWfxJmzb0zWytG/2DdmHZqOq803DbuI8/L0F4f8uAKB+ue1Tk4mF6Zu1qBM1eBpHkKnLk289WjKaXj39QZgtBJEDQtCQ1HYN3bsAPZ1sMfNy87RwbRcvFPDMH2nHM0BaAr3AsDSo+8gCwDcswlQQjc5eWKy/tzAtF/H/77FhnjipOKEt5Lu1pGezkorzlFo2kMrNTTD9OtRostpJ808vI652NWRASDPztCppWdt1gHu2NObzzrh5NL4xK9M9+5tQpvm7eQW62D/TFWJjTXF+bETkYbgd2BO65CBkn7+lHkfbUtDiY3kL+MYit5eHGh7c3TcRlVU64nVIvkxYTChUMppVfV/FB92erQQqAeViY4z8mEgcksxWl81aA0jTZ48lMquuJQca2yruh8dlle6bQfoUrzDPZ4IHWGT4ndz7miC4Q0PwA==";
        private const string PRIVATE_KEY = "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";

        [Test]
        public async Task UserCanBeRetrievedFromJtwTokenHeaders()
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

            string randomClaimName = Guid.NewGuid().ToString();
            string randomClaimValue = Guid.NewGuid().ToString();

            ClaimsIdentity identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(randomClaimName, randomClaimValue)
            });

            JTWAuthenticationProvider authenticationProvider = new JTWAuthenticationProvider(
                serverConfig,
                Mock.Of<ILogger>(),
                null
            );

            string generatedToken = await authenticationProvider.GenerateToken(new ClaimsPrincipal(identity));

            NameValueCollection nameValueCollection = new NameValueCollection();
            nameValueCollection.Add("Authorization", $"Bearer {generatedToken}");

            Mock<ICommandContext> requestContext = new Mock<ICommandContext>();
            requestContext
                .Setup(x => x.RequestHeaders)
                .Returns(nameValueCollection);

            // Act
            IPrincipal principal = await authenticationProvider.GetUserFromHeaders(requestContext.Object);

            // Assert
            Assert.That(principal, Is.Not.Null);
            Assert.That(principal.Identity.Name, Is.EqualTo("test"));
            Assert.That(principal.IsInRole("admin"), Is.True);

            Assert.That(((ClaimsPrincipal)principal).HasClaim(randomClaimName, randomClaimValue), Is.True);
        }

        [Test]
        public async Task UserCanBeRetrievedFromJtwTokenInCookies()
        {
            // Arrange
            JTWConfig jTWConfig = new JTWConfig
            {
                Audience = "audience",
                Issuer = "issuer",
                ExpiresAfter = 1000,
                SigningKey = "{8A7C5026-4348-4E5D-958E-B2052FFA3DB4}",
                CookieName = "__csauth"
            };

            ServerConfig serverConfig = new ServerConfig
            {
                JTWConfiguration = jTWConfig,
                PrivateEncryptionKey = PRIVATE_KEY,
                PublicEncryptionKey = PUBLIC_KEY,
            };

            string randomClaimName = Guid.NewGuid().ToString();
            string randomClaimValue = Guid.NewGuid().ToString();

            ClaimsIdentity identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "test"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(randomClaimName, randomClaimValue)
            });

            JTWAuthenticationProvider authenticationProvider = new JTWAuthenticationProvider(
                serverConfig,
                Mock.Of<ILogger>(),
                new EncryptionProvider()
            );

            HttpCookie authCookie = await authenticationProvider.GenerateTokenCookie(new ClaimsPrincipal(identity));

            Mock<ICommandContext> requestContext = new Mock<ICommandContext>();
            requestContext
                .Setup(x => x.RequestCookies)
                .Returns(new List<HttpCookie> { authCookie });


            // Act
            IPrincipal principal = await authenticationProvider.GetUserFromCookies(requestContext.Object);

            // Assert
            Assert.That(principal, Is.Not.Null);
            Assert.That(principal.Identity.Name, Is.EqualTo("test"));
            Assert.That(principal.IsInRole("admin"), Is.True);
            Assert.That(((ClaimsPrincipal)principal).HasClaim(randomClaimName, randomClaimValue), Is.True);
        }
    }
}
