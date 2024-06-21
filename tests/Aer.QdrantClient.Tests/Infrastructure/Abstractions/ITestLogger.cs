using Microsoft.Extensions.Logging;

namespace Aer.QdrantClient.Tests.Infrastructure.Abstractions;

internal interface ITestLogger
{
	public List<(LogLevel level, string message)> WrittenEvents { set; get; }
}
