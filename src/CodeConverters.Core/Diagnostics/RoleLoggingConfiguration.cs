using System;
using log4net;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using log4net.Core;

namespace CodeConverters.Core.Diagnostics
{
    public static class RoleLoggingConfiguration
    {
        public static readonly TimeSpan ShippingInterval = TimeSpan.FromMinutes(2);

        private static readonly ILog Logger = LogManager.GetLogger(typeof(RoleLoggingConfiguration));

        public static string LoggingPath { get { return RoleEnvironment.GetLocalResource("LoggingStorage").RootPath; } }
        public static string WcfTracePath { get { return RoleEnvironment.GetLocalResource("TracingStorage").RootPath; } }
        public static string DiagnosticsConnectionString { get { return CloudConfigurationManager.GetSetting("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"); } }
    
        /// <summary>
        /// Initialise logging using a size based rolling appender by default.
        /// </summary>
        /// <param name="processName">custom process name to use - this is incorporated into the log filename</param>
        public static void Initialize(string processName)
        {
            InitializeWith(processName, Level.All);
        }

        /// <summary>
        /// Initialises logging using size based rolling.  The logger is configured to fill up 31 x 10Mb log files before deleting old logs.
        /// Each time the log file roll overs it appends an incrementing counter onto the end of the filename.
        /// </summary>
        /// <param name="processName">custom process name to use - this is incorporated into the log filename</param>
        /// <param name="desiredDefaultLoggingLevel">desired logging level of default appender.  Defaults to ALL if not supplied.</param>
        public static void InitializeWith(string processName, Level desiredDefaultLoggingLevel = null)
        {
            RoleConfiguration.ThrowIfUnavailable();

            var fileAppender = Log4NetAppenderFactory.CreateRollingFileAppender(processName, LoggingPath, desiredDefaultLoggingLevel);

            var logAppenders = new[]
                {
                    fileAppender,
                    Log4NetAppenderFactory.CreateNewRelicAgentAppender()
                };

            Log4NetConfiguration.InitialiseLog4Net(logAppenders);
         
            ConfigureLastResortExceptionHandling();

            Logger.Info("Initialized Logging for Role: " + processName);
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