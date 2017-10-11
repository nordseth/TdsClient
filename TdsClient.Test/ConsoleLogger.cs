using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TdsClient.Test
{
    public static class ConsoleLogger
    {
        private static ILoggerFactory Factory = null;

        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (Factory == null)
                {
                    Factory = new LoggerFactory()
                        .AddConsole(LogLevel.Debug);
                }
                return Factory;
            }
            set
            {
                Factory = value;
            }
        }

        public static void Dispose()
        {
            if (Factory != null)
            {
                Factory.Dispose();
                Factory = null;
            }
        }
    }
}
