using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace SecuritySchemes
{
    public class StartupWithApiKeySecurity
    {
        public StartupWithApiKeySecurity(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", c => { });

            services.AddAuthorization(c =>
            {
                c.AddPolicy("readAccess", p =>
                {
                    p.AuthenticationSchemes.Add("ApiKey");
                    p.RequireClaim(ClaimTypes.System);
                });
                c.AddPolicy("writeAccess", p =>
                {
                    p.AuthenticationSchemes.Add("ApiKey");
                    p.RequireClaim(ClaimTypes.System);
                });
            });

            services.AddControllersWithViews();

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("apiKey", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Query,
                    Name = "apiKey"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Id = "apiKey", Type = ReferenceType.SecurityScheme }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Map("/resource-server", resourceServer =>
            {
                resourceServer.UseSwagger();

                resourceServer.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", "V1 Docs");
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
