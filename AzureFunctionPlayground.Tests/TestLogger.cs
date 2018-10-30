using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AzureFunctionPlayground.Tests
{
    public class TestLogger : ILogger
    {
        private readonly List<string> _logEntries = new List<string>();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _logEntries.Add(state.ToString());
        }

        public IEnumerable<string> LogEntries => _logEntries;

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}