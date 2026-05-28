using System.Text;

namespace weatherAPI.Logging;

public sealed class DetailedFileLoggerProvider : ILoggerProvider
{
    private readonly object syncRoot = new();
    private readonly StreamWriter writer;

    public DetailedFileLoggerProvider(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        writer = new StreamWriter(new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
        {
            AutoFlush = true
        };
    }

    public ILogger CreateLogger(string categoryName) => new DetailedFileLogger(categoryName, writer, syncRoot);

    public void Dispose() => writer.Dispose();

    private sealed class DetailedFileLogger(string categoryName, StreamWriter writer, object syncRoot) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var builder = new StringBuilder()
                .Append(DateTimeOffset.UtcNow.ToString("O"))
                .Append(" [")
                .Append(logLevel)
                .Append("] ")
                .Append(categoryName);

            if (eventId.Id != 0 || !string.IsNullOrWhiteSpace(eventId.Name))
            {
                builder
                    .Append(" (")
                    .Append(eventId.Id)
                    .Append(":")
                    .Append(eventId.Name)
                    .Append(")");
            }

            builder
                .Append(" - ")
                .Append(formatter(state, exception));

            if (exception is not null)
            {
                builder
                    .AppendLine()
                    .Append(exception);
            }

            lock (syncRoot)
            {
                writer.WriteLine(builder.ToString());
            }
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();

        public void Dispose()
        {
        }
    }
}
