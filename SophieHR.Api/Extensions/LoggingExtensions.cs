using System.Reflection;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Elastic.Ingest.Elasticsearch;
using Serilog;
using Serilog.Exceptions;

namespace SophieHR.Api.Extensions
{
    public static class LoggingExtensions
    {
        public static void ConfigureLogging(this WebApplicationBuilder builder)
        {
            var environment = builder.Environment.EnvironmentName;
            var configuration = builder.Configuration;

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithEnvironmentName()
                .Enrich.WithMachineName()
                .WriteTo.Debug()
                // .WriteTo.Console()
                .WriteTo.Elasticsearch(new[] { new Uri(configuration["ElasticConfiguration:Uri"]) }, opts =>
                {
                    opts.DataStream = new DataStreamName($"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace('.', '-')}", $"{environment?.ToLower().Replace('.', '-')}");
                    opts.BootstrapMethod = BootstrapMethod.Failure;
                }, transport => { })
                .Enrich.WithProperty("Environment", environment)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }
    }
}
