using Asp.Versioning;
using FCG.Payments.Infrastructure.SqlServer.Persistance;
using FCG.Payments.WebApi.Filters;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;

namespace FCG.Payments.WebApi.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddVersioning();
            services.AddFilters();
            services.AddHealthChecks().AddDbContextCheck<FcgPaymentDbContext>();
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddSwaggerConfiguration();
            services.AddSerilogLogging(configuration);
            services.AddApplicationInsights(configuration);

            return services;
        }

        private static void AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FCG.Users - V1", Version = "v1.0" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme.
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });

                c.SchemaGeneratorOptions = new SchemaGeneratorOptions
                {
                    UseAllOfForInheritance = true
                };
            });
        }

        private static void AddVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
        }

        private static void AddFilters(this IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add<TrimStringsActionFilter>();
            });
        }

        private static void AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
        {
            var seqUrl = configuration["Serilog:WriteTo:1:Args:serverUrl"] ?? configuration["Serilog:SeqUrl"] ?? "http://localhost:5341";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Application", "FCG.Users")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Seq(seqUrl)
                .CreateLogger();

            Log.Information("Starting FCG.Users application");
            Log.Information("Seq URL configured: {SeqUrl}", seqUrl);
            Log.Information("Environment: {Environment}", configuration["ASPNETCORE_ENVIRONMENT"]);

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            });
        }

        private static void AddApplicationInsights(this IServiceCollection services, IConfiguration configuration)
        {
            var applicationUrl = configuration["ApplicationInsights:ConnectionString"];

            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = applicationUrl;
            });
        }
    }
}
