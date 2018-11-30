using System.Collections.Generic;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace SecuritySchemes.AuthServer
{
    public static class Config
    {
        internal static IEnumerable<ApiScope> ApiScopes()
        {
            return new[]
            {
                new ApiScope("readAccess", "Access read operations"),
                new ApiScope("writeAccess", "Access write operations"),
            };
        }

        internal static IEnumerable<ApiResource> ApiResources()
        {
            return new[]
            {
                new ApiResource
                {
                    Name = "api",
                    DisplayName = "API",
                    Scopes = new[] { "readAccess", "writeAccess" }
                }
            };
        }

        internal static IEnumerable<Client> Clients()
        {
            return new[]
            {
                new Client
                {
                    ClientId = "test-client",
                    ClientName = "Test client (Code with PKCE)",
                    ClientSecrets = { new Secret("test-secret".Sha256()) },

                    RedirectUris = new[] {
                        "http://localhost:50134/resource-server/swagger/oauth2-redirect.html", // IIS Express
                        "http://localhost:5000/resource-server/swagger/oauth2-redirect.html", // Kestrel
                    },

                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedScopes = new[] { "readAccess", "writeAccess" },

                    RequireConsent = true,
                    RequirePkce = true,
                }
            };
        }

        internal static List<TestUser> Users()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "testuser1",
                    Username = "testuser1",
                    Password = "testpwd1"
                }
            };
        }
    }
}
