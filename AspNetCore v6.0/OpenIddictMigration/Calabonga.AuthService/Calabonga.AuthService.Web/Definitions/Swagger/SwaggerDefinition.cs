﻿using Calabonga.AuthService.Web.Definitions.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Calabonga.AuthService.Web.Definitions.Swagger
{
    public class SwaggerDefinition : AppDefinition
    {
        private const string _appTitle = "Microservice API";
        private const string _appVersion = $"{ThisAssembly.Git.SemVer.Major}.{ThisAssembly.Git.SemVer.Minor}.{ThisAssembly.Git.SemVer.Patch}";
        private const string _swaggerConfig = "/swagger/v1/swagger.json";

        public override void ConfigureApplication(WebApplication app, IWebHostEnvironment environment)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(settings =>
                {
                    settings.SwaggerEndpoint(_swaggerConfig, $"{_appTitle} v.{_appVersion}");
                    settings.HeadContent = $"{ThisAssembly.Git.Branch.ToUpper()} {ThisAssembly.Git.Commit.ToUpper()}";
                    settings.DocumentTitle = $"{_appTitle}";
                    settings.OAuth2RedirectUrl("https://localhost:20001/swagger/oauth2-redirect.html");
                    settings.DefaultModelExpandDepth(0);
                    settings.DefaultModelRendering(ModelRendering.Model);
                    settings.DefaultModelsExpandDepth(0);
                    settings.DocExpansion(DocExpansion.None);
                    settings.OAuthClientId("thunder_client");
                    settings.OAuthScopeSeparator(" ");
                    settings.OAuthClientSecret("thunder_client_secret");
                    settings.DisplayRequestDuration();
                    settings.OAuthAppName(_appTitle);
                });
            }
        }

        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = _appTitle,
                    Version = _appVersion,
                    Description = "Microservice module API. This template based on .NET 6.0"
                });

                options.ResolveConflictingActions(x => x.First());

                options.TagActionsBy(api =>
                {
                    string tag;
                    if (api.ActionDescriptor is ActionDescriptor descriptor)
                    {
                        var attribute = descriptor.EndpointMetadata.OfType<FeatureGroupNameAttribute>().FirstOrDefault();
                        tag = attribute?.GroupName ?? descriptor.RouteValues["controller"] ?? "Untitled";
                    }
                    else
                    {
                        tag = api.RelativePath!;
                    }

                    var tags = new List<string>();
                    if (!string.IsNullOrEmpty(tag))
                    {
                        tags.Add(tag);
                    }
                    return tags;
                });

                var url = configuration.GetSection("AuthServer").GetValue<string>("Url");

                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri($"{url}/connect/token", UriKind.Absolute),
                            AuthorizationUrl = new Uri($"{url}/connect/authorize", UriKind.Absolute),
                            Scopes = new Dictionary<string, string>
                            {
                                { "api", "Default scope" }
                            },
                            
                        }
                    }
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            },
                            In = ParameterLocation.Cookie

                        },
                        new List<string>()
                    }
                });
            });
        }
    }
}