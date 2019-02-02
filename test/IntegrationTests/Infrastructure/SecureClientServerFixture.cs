using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;

namespace IntegrationTests
{
    public class SecureClientServerFixture : ClientServerFixture
    {
        // Secret Key
        protected static readonly SecurityKey Key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes($"I-Am-The-GateKeeper-{DateTime.UtcNow}" + new string('=', 1024)));
        protected static readonly SigningCredentials SigningCreds = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);

        protected ILogger<SecureClientServerFixture> logger = NullLogger<SecureClientServerFixture>.Instance;
       
         protected override void ConfigureApp(IApplicationBuilder app, string url)
        {
            base.ConfigureApp(app, url);

            // Enable Auth
            app.UseAuthentication();

            // Set the Logger
            logger = app.ApplicationServices.GetService<ILogger<SecureClientServerFixture>>();
        }

        protected override void ConfigureServices(IServiceCollection services, string url)
        {
            base.ConfigureServices(services, url);

            // https://docs.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-2.2
            services.AddAuthentication(options =>
            {
                // Identity made Cookie authentication the default.
                // However, we want JWT Bearer Auth to be the default.
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Configure JWT Bearer Auth to expect our security key
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        LifetimeValidator = (before, expires, token, param) =>
                        {
                            return expires > DateTime.UtcNow;
                        },
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateActor = false,
                        ValidateLifetime = true,
                        IssuerSigningKey = Key
                    };

                // We have to hook the OnMessageReceived event in order to
                // allow the JWT authentication handler to read the access
                // token from the query string when a WebSocket or 
                // Server-Sent Events request comes in.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.StartsWithSegments(HubPath)))
                        {
                            // Read the token out of the query string
                            context.Token = accessToken;

                            logger.LogInformation($"Recieved JWT {accessToken}");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
        }

        protected override void ConfigureClientOptions(HttpConnectionOptions options)
        {
            base.ConfigureClientOptions(options);

            options.AccessTokenProvider = () =>
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                var token = new JwtSecurityToken(
                        "Test",
                        "Test",
                        new Claim[] {
                            new Claim(ClaimTypes.Name, "testuser")
                        },
                        expires: DateTime.UtcNow.AddDays(30),
                        signingCredentials: SigningCreds);

                var jwtToken = tokenHandler.WriteToken(token);

                logger.LogInformation($"JWT: {jwtToken}");

                return Task.FromResult(jwtToken);
            };
        }

        public class NameUserIdProvider : IUserIdProvider
        {
            public string GetUserId(HubConnectionContext connection)
            {
                return connection.User?.Identity?.Name;
            }
        }
    }
}