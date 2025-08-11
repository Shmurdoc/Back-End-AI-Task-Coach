using Microsoft.Extensions.Logging;
using Moq;

namespace AITaskCoach.Tests.Infrastructure;

public abstract class RepositoryTestBase<T> : TestBase where T : class
{
    protected readonly Mock<ILogger<T>> MockLogger;

    protected RepositoryTestBase()
    {
        MockLogger = new Mock<ILogger<T>>();
    }

    protected void VerifyLoggerCalled(LogLevel logLevel, Times times)
    {
        MockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}
