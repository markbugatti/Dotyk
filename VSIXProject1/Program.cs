using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Dotyk.Store.Deployment.Installer;
using Dotyk.Store.Deployment.Packaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Dotyk.Store.Deployment;

namespace VSIXProject1/*Dotyk.Store.Deployment*/
{
    public class DeployOptions
    {
        [VerbOption("PackPush", HelpText = "Packages and pushes the project to the server")]
        public PackagePushOption PackagePushOption { get; set; } = new PackagePushOption();
        [VerbOption("Pack", HelpText = "Packages the project to package file")]
        public PackageOption PackageOption { get; set; } = new PackageOption();
        [VerbOption("Install", HelpText = "Installs the package")]
        public InstallOption InstallOption { get; set; } = new InstallOption();
        [VerbOption("Push", HelpText = "Pushes the package to remote server")]
        public PushOption PushOption { get; set; } = new PushOption();
        [VerbOption("Share", HelpText = "Grants role to specific user")]
        public ShareOption ShareOption { get; set; } = new ShareOption();
    }

    public class CommonOptions
    {
        [Option("Verbosity", HelpText = "Log verbosity (Trace, Debug, Information, Warning, Error, Critical)", Required = false, DefaultValue = LogLevel.Information)]
        public LogLevel LogVerbosity { get; set; }
    }

    public class InstallOption : CommonOptions
    {
        [Option("Package", HelpText = "Path to package to install", Required = true)]
        public string PackagePath { get; set; }
    }

    public class PackageOption : CommonOptions
    {
        [Option("Project", HelpText = "Path to project to build", Required = true)]
        public string Project { get; set; }

        [Option("Solution", HelpText = "Path to the solution")]
        public string Solution { get; set; }

        [Option("Output", HelpText = "Output package file")]
        public string Output { get; set; }

        [Option("Configuration", HelpText = "Build configuration", DefaultValue = "Release")]
        public string Configuration { get; set; } = "Release";
    }

    public class PushOption : CommonOptions
    {
        [Option("Package", HelpText = "Path to package to push", Required = true)]
        public string PackagePath { get; set; }

        [Option("Login", HelpText = "Server login")]
        public string Login { get; set; }

        [Option("Password", HelpText = "Server password")]
        public string Password { get; set; }

        [Option("Register", HelpText = "Register on failed login")]
        public bool Register { get; set; }

        [Option("Server", HelpText = "Server to push package to", DefaultValue = "https://dotyk.store/")]
        public string ServerUrl { get; set; }
    }

    public class PackagePushOption : PackageOption
    {
        [Option("Server", HelpText = "Server to push package to", DefaultValue = "https://dotyk.store/")]
        public string ServerUrl { get; set; }

        [Option("Login", HelpText = "Server login")]
        public string Login { get; set; }

        [Option("Password", HelpText = "Server password")]
        public string Password { get; set; }

        [Option("Register", HelpText = "Register on failed login")]
        public bool Register { get; set; }
    }

    public class ShareOption : CommonOptions
    {
        [Option("AppId", HelpText = "Application Id", Required = true)]
        public string AppId { get; set; }

        [Option("Role", HelpText = "Role to grant", Required = true)]
        public string Role { get; set; }

        [Option("User", HelpText = "Dotyk.Me user id (usually email)", Required = true)]
        public string UserId { get; set; }
    }

    internal static class Program
    {
        private static string ResolveSolutionPath(string projectFile)
        {
            var currentDir = Path.GetDirectoryName(Path.GetFullPath(projectFile));

            while (currentDir != null)
            {
                var sln = Directory.EnumerateFiles(currentDir, "*.sln").FirstOrDefault();
                if (sln != null)
                    return sln;

                currentDir = Path.GetDirectoryName(currentDir);
            }

            return null;
        }

        private static async Task ExecuteVerb(PackageOption options)
        {
            var logger = GetLogger(options);

            var packager = PreparePackager(options.Configuration, logger);

            using (var packageStream = await packager.CreatePackage(
                   options.Project,
                   options.Solution ?? ResolveSolutionPath(options.Project),
                   default(CancellationToken)))
            {
                packageStream.Seek(0, SeekOrigin.Begin);

                if (options.Output != null)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(options.Output));
                    using (var outFile = File.Create(options.Output))
                    {
                        await packageStream.CopyToAsync(outFile);
                    }
                }
            }
        }

        private static async Task ExecuteVerb(PackagePushOption options)
        {
            var logger = GetLogger(options);

            var packager = PreparePackager(options.Configuration, logger);

            using (var packageStream = await packager.CreatePackage(
                    options.Project,
                    options.Solution ?? ResolveSolutionPath(options.Project),
                    default(CancellationToken)))
            {
                await PushPackage(new PushOption
                {
                    Login = options.Login,
                    Password = options.Password,
                    Register = options.Register,
                    ServerUrl = options.ServerUrl
                }, logger, packageStream);
            }
        }

        private static async Task ExecuteVerb(PushOption options)
        {
            var logger = GetLogger(options);

            using (var packageStream = File.OpenRead(options.PackagePath))
            {
                //await PushPackage(options, logger, packageStream);
            }
        }

        private static ILogger GetLogger(CommonOptions options)
        {
            return new ConsoleLogger("Publisher", (m, l) => l >= options.LogVerbosity, true);
        }

        //private static async Task PushPackage(PushOption options, ILogger logger, Stream packageStream)
        //{
        //    packageStream.Seek(0, SeekOrigin.Begin);

        //    var client = new StoreClient(
        //                    new StoreClientOptions
        //                    {
        //                        ServerUrl = new Uri(options.ServerUrl)
        //                    });

        //    using (logger.BeginScope("Publishing"))
        //    {
        //        try
        //        {
        //            try
        //            {
        //                await client.Login(new LoginViewModel
        //                {
        //                    Email = options.Login,
        //                    Password = options.Password
        //                });
        //            }
        //            catch
        //            {
        //                if (!options.Register) throw;

        //                logger.LogInformation("Registering new user");

        //                await client.Register(new RegisterViewModel
        //                {
        //                    Email = options.Login,
        //                    Password = options.Password
        //                });

        //                await client.Login(new LoginViewModel
        //                {
        //                    Email = options.Login,
        //                    Password = options.Password
        //                });
        //            }

        //            await client.SubmitPackage(packageStream);

        //            logger.LogInformation("Package published");
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.LogError(new EventId(), ex, "Failed to publish package");
        //            throw;
        //        }
        //    }
        //}

        private static async Task ExecuteVerb(InstallOption io)
        {
            var logger = new ConsoleLogger("Installer", null, true);

            using (logger.BeginScope("ExecuteInstall"))
            {
                try
                {
                    var installerFactory = new PackageInstallerFactory();
                    var appManager = new ApplicationManager();

                    var package = new InstallationPackage(io.PackagePath);

                    var installer = installerFactory.GetInstaller(package.Manifest.Deploy.Platform);

                    var result = await installer.InstallPackage(package, logger);

                    appManager.WriteAppPackageInstalled(package.Manifest.AppId, result);
                }
                catch (Exception ex)
                {
                    logger.LogError(new EventId(), ex, "Failed to isntall package");
                    throw;
                }
            }
        }

        private static Packager PreparePackager(string configuration, ILogger logger)
        {
            var be = new BuildEnvironment()
            {
                NugetPaths = { @"C:\Chocolatey\bin\nuget.exe", "nuget.exe" },
                Configuration = configuration
            };

            return new Packager
            {
                Packagers =
                {
                    new FolderPackager(logger),
                    new WinRTPackager(be, logger),
                    new Win32Packager(be, logger)
                }
            };
        }

        private static Regex PSArgRegex = new Regex(@"^-\w+$", RegexOptions.IgnoreCase);

        private static void FixArgsStyle(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (PSArgRegex.IsMatch(args[i]))
                    args[i] = "-" + args[i];
            }
        }

        private static void Main(string[] args)
        {
            FixArgsStyle(args);
            //TODO: configuration key
            var options = new DeployOptions();
            if (args.Length == 0)
            {
                WriteHelp();
                Environment.Exit(Parser.DefaultExitCodeFail);
            }

            try
            {
                if (!Parser.Default.ParseArguments(args, options, (v, o) =>
                {
                    if (o is PackagePushOption ppo) ExecuteVerb(ppo).Wait();
                    else if (o is PackageOption po) ExecuteVerb(po).Wait();
                    else if (o is InstallOption io) ExecuteVerb(io).Wait();
                    else if (o is PushOption _po) ExecuteVerb(_po).Wait();
                    else
                    {
                        WriteHelp();
                        Environment.Exit(Parser.DefaultExitCodeFail);
                    }
                }))
                {
                    WriteHelp();
                    Environment.Exit(Parser.DefaultExitCodeFail);
                }
            }
            catch (AggregateException ex) when (ex.InnerException != null)
            {
                Console.Error.WriteLine(ex.InnerException.ToString());
                Environment.Exit(-1);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
                Environment.Exit(-1);
            }
        }

        private static void WriteHelp()
        {
            var options = new DeployOptions();
            Console.WriteLine(new HelpText("Usage: <Verb> [-Option value]", " ", options));
            Console.WriteLine(new HelpText("PackPush", " ", options.PackagePushOption));
            Console.WriteLine(new HelpText("Pack", " ", options.PackageOption));
            Console.WriteLine(new HelpText("Install", " ", options.InstallOption));
            Console.WriteLine(new HelpText("Push", " ", options.PushOption));
            Console.WriteLine(new HelpText("Share", " ", options.ShareOption));
            Console.WriteLine("Example:");
            Console.WriteLine(@"Dotyk.Store.Deployment.exe PackPush -Project ""c:\Something\Project.csproj"" -Server ""http://dotyk-store.azurewebsites.net"" -Login ""something@example.com"" -Password ""securePassword!"" -Register");
            Console.WriteLine(@"Dotyk.Store.Deployment.exe Pack -Project ""c:\Something\Project.csproj""");
        }
    }
}
