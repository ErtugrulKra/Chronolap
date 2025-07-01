using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Chronolap.Tests
{
    public class ChronolapTimerTests
    {
        [Fact]
        public void StartStop_ElapsedTimeIsGreaterThanZero()
        {
            var timer = new ChronolapTimer();
            timer.Start();
            Thread.Sleep(100);
            timer.Stop();

            Assert.True(timer.Elapsed.TotalMilliseconds >= 100);
        }

        [Fact]
        public void Lap_AddsLapCorrectly()
        {
            var timer = new ChronolapTimer();
            timer.Start();
            Thread.Sleep(50);
            timer.Lap("First");
            timer.Stop();

            Assert.Single(timer.Laps);
            Assert.Equal("First", timer.Laps[0].Name);
        }

        [Fact]
        public void Reset_ClearsAllState()
        {
            var timer = new ChronolapTimer();
            timer.Start();
            timer.Lap("Lap1");
            timer.Stop();
            timer.Reset();

            Assert.Empty(timer.Laps);
            Assert.Equal(TimeSpan.Zero, timer.Elapsed);
        }

        [Fact]
        public void MeasureExecutionTime_AddsLap()
        {
            var timer = new ChronolapTimer();
            timer.Start();
            timer.MeasureExecutionTime(() => Thread.Sleep(30));

            Assert.Single(timer.Laps);
            Assert.True(timer.Laps[0].Duration.TotalMilliseconds >= 30);
        }

        [Fact]
        public async Task MeasureExecutionTimeAsync_AddsLap()
        {
            var timer = new ChronolapTimer();
            timer.Start();
            await timer.MeasureExecutionTimeAsync(async () =>
            {
                await Task.Delay(50);
            });

            Assert.Single(timer.Laps);
            Assert.True(timer.Laps[0].Duration.TotalMilliseconds >= 50);
        }

        [Fact]
        public void TotalLapTime_SumsAllLaps()
        {
            var timer = new ChronolapTimer();
            timer.Start();
            Thread.Sleep(20);
            timer.Lap("1");
            Thread.Sleep(30);
            timer.Lap("2");

            var total = timer.TotalLapTime.TotalMilliseconds;
            Assert.True(total >= 50);
        }

        [Fact]
        public void Logger_WritesMessage_WhenLoggerProvided()
        {
            var mockLogger = new Mock<ILogger>();
            var timer = new ChronolapTimer(mockLogger.Object);

            timer.Start();
            Thread.Sleep(10);
            timer.Lap("TestLap");

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TestLap")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Pause_StopsStopwatch()
        {
            var timer = new ChronolapTimer();
            timer.Start();
            Thread.Sleep(30);
            timer.Pause();

            var pausedTime = timer.Elapsed;
            Thread.Sleep(30);
            Assert.Equal(pausedTime, timer.Elapsed);
        }

        [Fact]
        public void Resume_ContinuesStopwatch()
        {
            var timer = new ChronolapTimer();
            timer.Start();
            Thread.Sleep(30);
            timer.Pause();
            var pausedTime = timer.Elapsed;

            Thread.Sleep(30);
            timer.Resume();
            Thread.Sleep(30);

            Assert.True(timer.Elapsed > pausedTime);
        }

        
    }

}
