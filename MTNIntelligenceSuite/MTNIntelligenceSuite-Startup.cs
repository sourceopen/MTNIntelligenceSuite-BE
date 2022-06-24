using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net.Http.Headers;
using NavisSmartRestGateway.RestIntrastructure;
using System.Reflection;
using NavisSmartRestGateway;
using DBFramework;

namespace MTNIntelligenceSuite
{
    public class MTNIntelligenceSuite_Startup
    {
        private static ILogger<MTNIntelligenceSuite_Startup> logger;
        private static IHost host = null;

        public static void Main(string[] args)
        {
            try
            {
                host = CreateHostBuilder(args).Build();

                Assembly assembly = Assembly.GetExecutingAssembly();

                string versionNo = AssemblyName.GetAssemblyName(assembly.Location).Version.ToString();

                logger = host.Services.GetRequiredService<ILogger<MTNIntelligenceSuite_Startup>>();

                host.Run();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Caught unhandled exception, shutting down, Exception -> {0}", ex.ToString());
            }
            finally
            {
                LogManager.Shutdown();
                host.StopAsync();
                host.Dispose();
                Environment.Exit(0);
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                 .UseWindowsService()
                 .ConfigureLogging((hostingContext, logging) =>
                 {
                     IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
#if DEBUG
                                                                                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
#endif
                                                                                .Build();

                     logging.AddConfiguration(configuration.GetSection("Logging"));

                     LogManager.Configuration = new NLogLoggingConfiguration(configuration.GetSection("NLog"));
                     Trace.Listeners.Add(new NLogTraceListener());

#if DEBUG
                     Trace.Listeners.Add(new ConsoleTraceListener());
                     logging.AddConsole();
#endif

                     logging.AddNLog();
                 })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //startup the rest server first before anything else
                    IConfigurationRoot configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
#if DEBUG
                                                                              .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
#endif
                                                                              .Build();

                    IConfigurationSection restPort = configuration.GetSection("AppSettings:ListeningUri");

                    if (restPort.Value == null)
                    {
                        Trace.TraceError("Throwing missing key exception and shutting down service and since key \"AppSettings:ListeningUri\" is missing!");
                        throw new Exception("AppSettings:ListeningUri");
                    }
                    webBuilder.UseStartup<RestStartup>()
                              .UseUrls(restPort.Value);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddCors();



                    //set up http client for REST service
                    services.AddHttpClient<IMTNDataService, NavisSmartMTNDataService>(httpClient =>
                     {
                         string endpoint = hostContext.Configuration["AppSettings:RestEndPoint"];
                         if (endpoint == null)
                         {
                             Trace.TraceError("Throwing missing key exception and shutting down service and since key \"AppSettings:RestEndPoint\" is missing!");
                             throw new Exception("AppSettings:RestEndPoint");
                         }
                         httpClient.BaseAddress = new System.Uri(endpoint);
                         httpClient.DefaultRequestHeaders.Accept.Clear();
                         httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                     });


                    services.AddSingleton<RestGatewaySevice>();
                    services.AddSingleton<DBConnection>();

                    });
    }
}