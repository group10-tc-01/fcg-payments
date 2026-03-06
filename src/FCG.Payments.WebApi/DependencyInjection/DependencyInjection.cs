using Asp.Versioning;
using FCG.Payments.Infrastructure.SqlServer.Persistance;
using FCG.Payments.WebApi.Observability;
using FCG.Payments.WebApi.Filters;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Logs;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Serialization;

namespace FCG.Payments.WebApi.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    public static class DependencyInjection
    {
        public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
                   .AddJsonOptions(options =>
                   {
                       options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                   });

            services.AddEndpointsApiExplorer();
            services.AddHttpContextAccessor();
            services.AddSwaggerGen();
            services.AddSwaggerConfiguration();

            services.AddVersioning();
            services.AddFilters();
            services.AddHealthChecks().AddDbContextCheck<FcgPaymentDbContext>();
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddObservability(configuration);
            services.AddSerilogLogging(configuration);
            return services;
        }

        private static void AddObservability(this IServiceCollection services, IConfiguration configuration)
        {
            var observabilityOptions = configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>() ?? new ObservabilityOptions();

            if (string.IsNullOrWhiteSpace(observabilityOptions.ServiceVersion))
            {
                observabilityOptions.ServiceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0-local";
            }

            var environmentName = configuration["ASPNETCORE_ENVIRONMENT"] ?? Environments.Production;

            services.Configure<ObservabilityOptions>(configuration.GetSection(ObservabilityOptions.SectionName));

            services.AddOpenTelemetry()
                .WithTracing(tracing =>
                {
                    var resourceBuilder = ObservabilityTelemetry.CreateResourceBuilder(observabilityOptions, environmentName);

                    ObservabilityTelemetry.ConfigureTracing(tracing, resourceBuilder, observabilityOptions);
                })
                .WithMetrics(metrics =>
                {
                    var resourceBuilder = ObservabilityTelemetry.CreateResourceBuilder(observabilityOptions, environmentName);

                    ObservabilityTelemetry.ConfigureMetrics(metrics, resourceBuilder, observabilityOptions);
                });
        }

        private static void AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FCG.Payments - V1", Version = "v1.0" });

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
            var observabilityOptions = configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>() ?? new ObservabilityOptions();
            var environmentName = configuration["ASPNETCORE_ENVIRONMENT"] ?? Environments.Production;
            var resourceBuilder = ObservabilityTelemetry.CreateResourceBuilder(observabilityOptions, environmentName);
            var seqUrl = configuration["Serilog:WriteTo:1:Args:serverUrl"] ?? configuration["Serilog:SeqUrl"] ?? "http://localhost:5341";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.With(new TraceLogEnricher())
                .Enrich.WithProperty("Application", observabilityOptions.ServiceName)
                .Enrich.WithProperty("service.name", observabilityOptions.ServiceName)
                .Enrich.WithProperty("service.version", observabilityOptions.ServiceVersion)
                .Enrich.WithProperty("deployment.environment", environmentName)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] [{trace_id}/{span_id}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Seq(seqUrl)
                .CreateLogger();

            Log.Information("Starting {ServiceName} application", observabilityOptions.ServiceName);
            Log.Information("Seq URL configured: {SeqUrl}", seqUrl);
            Log.Information("Environment: {Environment}", environmentName);
            Log.Information("OTLP endpoint configured: {OtlpEndpoint}", observabilityOptions.OtlpEndpoint);

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
                loggingBuilder.AddOpenTelemetry(options =>
                {
                    options.SetResourceBuilder(resourceBuilder);
                    options.IncludeFormattedMessage = true;
                    options.IncludeScopes = true;
                    options.ParseStateValues = true;

                    if (observabilityOptions.EnableOtlpExporter)
                    {
                        options.AddOtlpExporter(exporterOptions =>
                        {
                            exporterOptions.Endpoint = new Uri(observabilityOptions.OtlpEndpoint);
                        });
                    }
                });
            });
        }
    }
}
