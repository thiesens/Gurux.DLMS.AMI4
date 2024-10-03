using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Loader;
using Gurux.DLMS.AMI.Agent.Worker;
using Gurux.DLMS.AMI.Agent.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Gurux.DLMS.AMI.Shared;

namespace Gurux.DLMS.AMI.Agent
{
    class Program
    {
        const string settingsFile = "settings.json";
        static ILogger? _logger;
        static IGXAgentWorker? worker = null;
                
        static IConfiguration Configuration;

        static string serverName;
        static string hostName;
        static string token;

        /// <summary>
        /// Register the agent.
        /// </summary>
        /// <returns></returns>
        public static async Task RegisterAgent(
            IServiceCollection services,
            AutoResetEvent newVersion,
            IGXAgentWorker worker,
            AgentOptions options)
        {
            string name = hostName ?? System.Net.Dns.GetHostName();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Running on host. Collecting params");
                Console.WriteLine($"Welcome to use Gurux.DLMS.AMI.");
                Console.WriteLine($"Gurux.DLMS.AMI address: [{options.Address}]");
                string? tmp = Console.ReadLine();
                if (!string.IsNullOrEmpty(tmp))
                {
                    options.Address = tmp;
                }
                Console.WriteLine($"Enter agent name: [{name}]");
                tmp = Console.ReadLine();
                if (!string.IsNullOrEmpty(tmp))
                {
                    name = tmp;
                }
                Console.WriteLine("Enter Personal Access Token:");
                options.Token = Console.ReadLine();
                if (options.Token == null || options.Token.Length != 64)
                {
                    throw new ArgumentException("Invalid token.");
                }
            }
            else
            {
                Console.WriteLine("Running in a container.");
                options.Address = serverName;
                name = hostName;
                options.Token = token;
            }

            worker.Init(services, options, newVersion);
            options.Id = await worker.AddAgentAsync(name);
            if (options.Id == Guid.Empty)
            {
                throw new ArgumentException("Invalid agent Id.");
            }
            File.WriteAllText(settingsFile, JsonSerializer.Serialize(options));
            Console.WriteLine("Connection succeeded.");
        }

        static async Task<int> Main(string[] args)
        {
            // Load appsettings.json and environment variables.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            // Set configuration values, fallback to environment variables if they exist.
            serverName = Configuration["AgentSettings:ServerName"] ?? Environment.GetEnvironmentVariable("GURUX_SERVER") ?? "https://localhost:8001";
            hostName = Configuration["AgentSettings:HostName"] ?? Environment.GetEnvironmentVariable("GURUX_HOST") ?? System.Net.Dns.GetHostName();
            token = Configuration["AgentSettings:Token"] ?? Environment.GetEnvironmentVariable("GURUX_TOKEN");

            AssemblyLoadContext alc;
            AutoResetEvent newVersion = new AutoResetEvent(true);
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(asm.Location);
            Console.WriteLine($"starting Gurux.DLMS.Agent version {info.FileVersion}");

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += async (s, e) =>
            {
                Console.WriteLine("Canceling...");
                cts.Cancel();
                Environment.Exit(0);
            };

            try
            {
                using var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder
                        .AddFilter("Microsoft", LogLevel.Warning)
                        .AddFilter("System", LogLevel.Warning)
                        .AddFilter(typeof(Program).FullName, LogLevel.Debug)
                        .AddConsole();
                });
                _logger = loggerFactory.CreateLogger<Program>();

                AgentOptions options = new AgentOptions
                {
                    Address = serverName // default value from configuration
                };

                Settings settings = new Settings();
                int ret = Settings.GetParameters(args, settings);
                if (ret != 0)
                {
                    return ret;
                }

                if (!string.IsNullOrEmpty(settings.Host))
                {
                    options.Address = settings.Host;
                }

                if (!File.Exists(settingsFile) || args.Contains("Configure"))
                {
                    alc = new AssemblyLoadContext("tmp");
                    asm = alc.LoadFromAssemblyPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Gurux.DLMS.AMI.Agent.Worker.dll"));
                    IGXAgentWorker worker = (IGXAgentWorker)asm.CreateInstance(typeof(GXAgentWorker).FullName);
                    IServiceCollection services = new ServiceCollection();
                    services.AddLogging(builder =>
                    {
                        builder
                            .AddFilter("Microsoft", LogLevel.Warning)
                            .AddFilter("System", LogLevel.Warning)
                            .AddFilter(typeof(Program).FullName, LogLevel.Debug)
                            .AddConsole();
                    });
                    await RegisterAgent(services, newVersion, worker, options);
                    await worker.StopAsync();
                    worker = null;
                }

                options = JsonSerializer.Deserialize<AgentOptions>(File.ReadAllText(settingsFile));
                if (options == null || options.Token == null || options.Token.Length != 64)
                {
                    throw new ArgumentException("Invalid token.");
                }

                if (options.Id == Guid.Empty)
                {
                    throw new ArgumentException("Invalid agent Id.");
                }

                if (info.ProductVersion != null && !info.ProductVersion.Contains("-local"))
                {
                    options.Version = info.ProductVersion;
                }
                else
                {
                    options.Version = null;
                }

                Task t = Task.Run(() => ClosePoller(cts));
                alc = new AssemblyLoadContext("Agent.Worker", true);
                string currentVersion = options.Version;
                bool update = newVersion.WaitOne(0);
                do
                {
                    if (update)
                    {
                        if (worker != null)
                        {
                            File.WriteAllText(settingsFile, JsonSerializer.Serialize(options));
                            await worker.StopAsync();
                            worker = null;
                            alc.Unload();
                            alc = new AssemblyLoadContext("Agent.Worker", true);
                        }

                        string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        if (path != null && !string.IsNullOrEmpty(options.Version))
                        {
                            path = Path.Combine(path, "bin" + options.Version);
                            if (!Directory.Exists(path))
                            {
                                path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                            }
                        }

                        try
                        {
                            asm = alc.LoadFromAssemblyPath(Path.Combine(path, "Gurux.DLMS.AMI.Agent.Worker.dll"));
                            worker = (IGXAgentWorker)asm.CreateInstance(typeof(GXAgentWorker).FullName);
                            IServiceCollection services = new ServiceCollection();
                            services.AddLogging(builder =>
                            {
                                builder
                                    .AddFilter("Microsoft", LogLevel.Warning)
                                    .AddFilter("System", LogLevel.Warning)
                                    .AddFilter(typeof(Program).FullName, LogLevel.Debug)
                                    .AddConsole();
                            });
                            worker.Init(services, options, newVersion);
                            await worker.StartAsync();
                            currentVersion = options.Version;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error: {ex.Message}");
                            options.Version = currentVersion;
                            File.WriteAllText(settingsFile, JsonSerializer.Serialize(options));
                            newVersion.Set();
                        }
                    }

                    int wait = AutoResetEvent.WaitAny(new WaitHandle[] { cts.Token.WaitHandle, newVersion });
                    if (wait == 0)
                    {
                        break;
                    }
                    else if (wait == 1)
                    {
                        update = true;
                    }
                }
                while (true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return 1;
            }
            finally
            {
                await Disconnect();
            }
            return 0;
        }

        private static void ClosePoller(CancellationTokenSource cts)
        {
            Console.WriteLine("----------------------------------------------------------");
            ConsoleKey k;
            while ((k = Console.ReadKey().Key) != ConsoleKey.Escape)
            {
                if (k == ConsoleKey.Delete)
                {
                    Console.Clear();
                }
                Console.WriteLine("Press Esc to close application or delete clear the console.");
            }
            cts.Cancel();
        }

        private static async Task Disconnect()
        {
            if (worker != null)
            {
                await worker.StopAsync();
                worker = null;
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (worker