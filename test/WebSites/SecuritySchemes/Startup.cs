using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SecuritySchemes.ResourceServer.Swagger;

namespace SecuritySchemes
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Register IdentityServer services to power OAuth2.0 flows
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryApiScopes(AuthServer.Config.ApiScopes())
                .AddInMemoryApiResources(AuthServer.Config.ApiResources())
                .AddInMemoryClients(AuthServer.Config.Clients())
                .AddTestUsers(AuthServer.Config.Users());

            services.AddAuthentication("Cookies")
                .AddCookie()
                .AddJwtBearer(c =>
                {
                    c.Authority = "http://localhost:50134/auth-server";
                    c.RequireHttpsMetadata = false;
                    c.Audience = "api";
                });

            services.AddAuthorization(c =>
            {
                c.AddPolicy("readAccess", p =>
                {
                    p.AuthenticationSchemes.Add("Bearer");
                    p.RequireClaim("scope", "readAccess");
                });
                c.AddPolicy("writeAccess", p =>
                {
                    p.AuthenticationSchemes.Add("Bearer");
                    p.RequireClaim("scope", "writeAccess");
                });
            });

            services.AddControllersWithViews();

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("/auth-server/connect/authorize", UriKind.Relative),
                            TokenUrl = new Uri("/auth-server/connect/token", UriKind.Relative),
                            Scopes = new Dictionary<string, string>
                            {
                                { "readAccess", "Access read operations" },
                                { "writeAccess", "Access write operations" }
                            }
                        }
                    }
                });

                c.OperationFilter<SecurityRequirementsOperationFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Map("/auth-server", authServer =>
            {
                authServer.UseRouting();

                authServer.UseAuthentication();
                authServer.UseIdentityServer();

                authServer.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            });

            app.Map("/resource-server", resourceServer =>
            {
                resourceServer.UseSwagger();

                resourceServer.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", "V1 Docs");

                    // Additional OAuth settings (See https://github.com/swagger-api/swagger-ui/blob/v3.42.0/docs/usage/oauth2.md)
                    c.OAuthClientId("test-client");
                    c.OAuthClientSecret("test-secret");
                    c.OAuthAppName("test-app");
                    c.OAuthScopeSeparator(" ");
                    c.OAuthScopes("readAccess");
                    c.OAuthUsePkce();
                });

                resourceServer.UseRouting();

                resourceServer.UseAuthentication();
                resourceServer.UseAuthorization();

                resourceServer.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            });
        }
    }
}
