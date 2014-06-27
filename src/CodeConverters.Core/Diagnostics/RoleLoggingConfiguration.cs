using System;
using System.Linq;
using log4net;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using log4net.Appender;

namespace CodeConverters.Core.Diagnostics
{
    public static class RoleLoggingConfiguration
    {
        public static readonly TimeSpan ShippingInterval = TimeSpan.FromMinutes(2);

        private static readonly ILog Logger = LogManager.GetLogger(typeof(RoleLoggingConfiguration));

        public static string LoggingPath { get { return RoleEnvironment.GetLocalResource("LoggingStorage").RootPath; } }
        public static string WcfTracePath { get { return RoleEnvironment.GetLocalResource("TracingStorage").RootPath; } }
        public static string DiagnosticsConnectionString { get { return CloudConfigurationManager.GetSetting("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"); } }
        private static DiagnosticMonitorConfiguration _initialConfiguration;

        public static void Initialize(string processName)
        {
            // by default use date rolling
            InitializeWithDateRolling(processName, Log4NetDirectory);
        }

        public static void InitializeWithDateRolling(string processName, params DirectoryConfiguration[] directories)
        {
            RoleConfiguration.ThrowIfUnavailable();

            var fileAppender = Log4NetAppenderFactory.CreateRollingFileAppender(processName, LoggingPath);

            InitializeWithBaseAppender(processName, fileAppender, directories);
        }

        /// <summary>
        /// Initialises logging using a base size based log that is configured to fill up 31 x 10Mb log files before deleting old logs.
        /// Each time the log file roll overs it appends an incrementing counter onto the end of the filename.
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="directories"></param>
        public static void InitializeWithSizeRolling(string processName, params DirectoryConfiguration[] directories)
        {
            RoleConfiguration.ThrowIfUnavailable();

            var fileAppender = Log4NetAppenderFactory.CreateSizeBasedRollingFileAppender(processName, LoggingPath);

            InitializeWithBaseAppender(processName, fileAppender, directories);
        }

        private static void InitializeWithBaseAppender(string processName, RollingFileAppender fileAppender, params DirectoryConfiguration[] directories)
        {
             var logAppenders = new[]
                {
                    fileAppender,
                    Log4NetAppenderFactory.CreateNewRelicAgentAppender()
                };

            Log4NetConfiguration.InitialiseLog4Net(logAppenders);
            ConfigureAzureDiagnostics(directories);

            ConfigureLastResortExceptionHandling();

            Logger.Info("Initialized Logging for Role: " + processName);
        }
       
        public static DirectoryConfiguration Log4NetDirectory
        {
            get
            {
                return new DirectoryConfiguration
                {
                    Container = "wad-log4net",
                    DirectoryQuotaInMB = 1024,
                    Path = LoggingPath
                };
            }
        }

        public static DirectoryConfiguration WcfTraceDirectory
        {
            get
            {
                return new DirectoryConfiguration
                {
                    Container = "wad-wcf-trace",
                    DirectoryQuotaInMB = 1024,
                    Path = WcfTracePath
                };
            }
        }

        private static void ConfigureAzureDiagnostics(params DirectoryConfiguration[] directories)
        {
            _initialConfiguration = DiagnosticMonitor.GetDefaultInitialConfiguration();

            _initialConfiguration.Directories.ScheduledTransferPeriod = ShippingInterval;
            foreach (var directory in directories)
                _initialConfiguration.Directories.DataSources.Add(directory);

            _initialConfiguration.Logs.ScheduledTransferPeriod = ShippingInterval;
            _initialConfiguration.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;

            _initialConfiguration.WindowsEventLog.ScheduledTransferPeriod = ShippingInterval;
            _initialConfiguration.WindowsEventLog.ScheduledTransferLogLevelFilter = LogLevel.Verbose;

            _initialConfiguration.DiagnosticInfrastructureLogs.ScheduledTransferPeriod = ShippingInterval;
            _initialConfiguration.DiagnosticInfrastructureLogs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;

            _initialConfiguration.Directories.DataSources.Single(d => d.Container == "wad-crash-dumps").DirectoryQuotaInMB = 512;

            var iisLogSource = _initialConfiguration.Directories.DataSources.SingleOrDefault(d => d.Container == "wad-iis-logfiles");
            if (iisLogSource != null) iisLogSource.DirectoryQuotaInMB = 512;

            var iisFailedLogSource = _initialConfiguration.Directories.DataSources.SingleOrDefault(d => d.Container == "wad-iis-failedreqlogfiles");
            if (iisFailedLogSource != null) iisFailedLogSource.DirectoryQuotaInMB = 512;

            _initialConfiguration.OverallQuotaInMB = _initialConfiguration.Directories.DataSources.Sum(d => d.DirectoryQuotaInMB);

            DiagnosticMonitor.StartWithConnectionString(DiagnosticsConnectionString, _initialConfiguration);
        }

        private static void ConfigureLastResortExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var exception = args.ExceptionObject as Exception;
                if (exception != null)
                {
                    Logger.Fatal("Unhandled exception is crashing process", exception);
                    return;
                }

                Logger.Fatal("Unhandled exception of unrecognisable type is crashing process");
            };
        }
    }
}