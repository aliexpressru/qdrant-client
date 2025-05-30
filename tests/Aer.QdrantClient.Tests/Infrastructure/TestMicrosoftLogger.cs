﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Tests.Infrastructure;

internal class TestMicrosoftLogger : ILogger
{
    private class MicrosoftLoggerScopeMock<TState> : IDisposable
    {
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        public TState ScopeState { get; }

        public MicrosoftLoggerScopeMock(TState scopeState)
        {
            ScopeState = scopeState;
        }

        public void Dispose()
        { }
    }

    public List<(LogLevel level, string message)> WrittenEvents { set; get; } = [];

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        var rendered = formatter.Invoke(state, exception);

        WrittenEvents.Add((logLevel, rendered));
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return new MicrosoftLoggerScopeMock<TState>(state);
    }
}
