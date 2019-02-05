using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AgentR.Client
{
    public static class Logging
    {
        private static ILoggerFactory loggerFactory = null;

        public static void SetFactory(ILoggerFactory factory) => loggerFactory = factory;

        static ILoggerFactory Factory
        {
            get
            {
                if (null == loggerFactory)
                {
                    loggerFactory = new LoggerFactory();
                }
                return loggerFactory;
            }
        }

        static Lazy<ILogger> logger = new Lazy<ILogger>(() => Factory.CreateLogger("AgentR.Client"));

        internal static ILogger Logger => logger.Value;
    }
}
