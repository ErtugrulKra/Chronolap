using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
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
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("TestLap")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
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

        [Fact]
        public void CalculateLapStatistic_Min_ReturnsMinimumDuration()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            for (int i = 0; i < 30; i++)
            {
                Thread.Sleep(10 + i);
                timer.Lap($"Lap{i}");
            }

            var min = timer.CalculateLapStatistic(LapStatisticsType.Min, minimumLapCount: 30);
            Assert.NotNull(min);
            Assert.True(min >= 10);
        }

        [Fact]
        public void CalculateLapStatistic_Max_ReturnsMaximumDuration()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            for (int i = 0; i < 30; i++)
            {
                Thread.Sleep(10 + i);
                timer.Lap($"Lap{i}");
            }

            var max = timer.CalculateLapStatistic(LapStatisticsType.Max, minimumLapCount: 30);
            Assert.NotNull(max);
            Assert.True(max >= 39);
        }

        [Fact]
        public void CalculateLapStatistic_Variance_ReturnsVariance()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            for (int i = 0; i < 30; i++)
            {
                Thread.Sleep(10);
                timer.Lap($"Lap{i}");
            }

            var variance = timer.CalculateLapStatistic(LapStatisticsType.Variance, minimumLapCount: 30);
            Assert.NotNull(variance);
            Assert.True(variance >= 0);
        }

        [Fact]
        public void CalculatePercentile_ReturnsCorrectPercentile()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            for (int i = 0; i < 30; i++)
            {
                Thread.Sleep(10 + i);
                timer.Lap($"Lap{i}");
            }

            var p50 = timer.CalculatePercentile(50, minimumLapCount: 30);
            var p95 = timer.CalculatePercentile(95, minimumLapCount: 30);
            var p99 = timer.CalculatePercentile(99, minimumLapCount: 30);

            Assert.NotNull(p50);
            Assert.NotNull(p95);
            Assert.NotNull(p99);
            Assert.True(p95 >= p50);
            Assert.True(p99 >= p95);
        }

        [Fact]
        public void CalculatePercentile_ThrowsException_WhenPercentileOutOfRange()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            for (int i = 0; i < 30; i++)
            {
                Thread.Sleep(10);
                timer.Lap($"Lap{i}");
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => timer.CalculatePercentile(-1, minimumLapCount: 30));
            Assert.Throws<ArgumentOutOfRangeException>(() => timer.CalculatePercentile(101, minimumLapCount: 30));
        }

        [Fact]
        public void GetFastestLap_ReturnsLapWithMinimumDuration()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            Thread.Sleep(50);
            timer.Lap("Slow");
            Thread.Sleep(10);
            timer.Lap("Fast");
            Thread.Sleep(30);
            timer.Lap("Medium");

            var fastest = timer.GetFastestLap();
            Assert.NotNull(fastest);
            Assert.Equal("Fast", fastest.Name);
        }

        [Fact]
        public void GetSlowestLap_ReturnsLapWithMaximumDuration()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            Thread.Sleep(10);
            timer.Lap("Fast");
            Thread.Sleep(50);
            timer.Lap("Slow");
            Thread.Sleep(30);
            timer.Lap("Medium");

            var slowest = timer.GetSlowestLap();
            Assert.NotNull(slowest);
            Assert.Equal("Slow", slowest.Name);
        }

        [Fact]
        public void GetFastestLap_ReturnsNull_WhenNoLaps()
        {
            var timer = new ChronolapTimer();
            var fastest = timer.GetFastestLap();
            Assert.Null(fastest);
        }

        [Fact]
        public void GetSlowestLap_ReturnsNull_WhenNoLaps()
        {
            var timer = new ChronolapTimer();
            var slowest = timer.GetSlowestLap();
            Assert.Null(slowest);
        }

        [Fact]
        public void MeasureExecutionTime_WithReturnValue_ReturnsResult()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            int result = timer.MeasureExecutionTime(() => 42, "GetNumber");

            Assert.Equal(42, result);
            Assert.Single(timer.Laps);
            Assert.Equal("GetNumber", timer.Laps[0].Name);
        }

        [Fact]
        public void MeasureExecutionTime_WithReturnValue_WithoutLapName_UsesMethodName()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            string result = timer.MeasureExecutionTime(() => "test");

            Assert.Equal("test", result);
            Assert.Single(timer.Laps);
        }

        [Fact]
        public async Task MeasureExecutionTimeAsync_WithReturnValue_ReturnsResult()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            int result = await timer.MeasureExecutionTimeAsync(async () =>
            {
                await Task.Delay(10);
                return 100;
            }, "GetAsyncNumber");

            Assert.Equal(100, result);
            Assert.Single(timer.Laps);
            Assert.Equal("GetAsyncNumber", timer.Laps[0].Name);
        }

        [Fact]
        public void MeasureExecutionTimeWithExceptionHandling_RecordsLap_WhenExceptionThrown()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            Assert.Throws<InvalidOperationException>(() =>
                timer.MeasureExecutionTimeWithExceptionHandling(() =>
                {
                    throw new InvalidOperationException("Test exception");
                }, "ExceptionTest"));

            Assert.Single(timer.Laps);
            Assert.Equal("ExceptionTest", timer.Laps[0].Name);
        }

        [Fact]
        public void MeasureExecutionTimeWithExceptionHandling_RecordsLap_WhenNoException()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            timer.MeasureExecutionTimeWithExceptionHandling(() =>
            {
                Thread.Sleep(10);
            }, "NoExceptionTest");

            Assert.Single(timer.Laps);
            Assert.Equal("NoExceptionTest", timer.Laps[0].Name);
        }

        [Fact]
        public async Task MeasureExecutionTimeWithExceptionHandlingAsync_RecordsLap_WhenExceptionThrown()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await timer.MeasureExecutionTimeWithExceptionHandlingAsync(async () =>
                {
                    await Task.Delay(10);
                    throw new InvalidOperationException("Test exception");
                }, "AsyncExceptionTest"));

            Assert.Single(timer.Laps);
            Assert.Equal("AsyncExceptionTest", timer.Laps[0].Name);
        }

        [Fact]
        public void MeasureExecutionTimeWithExceptionHandling_WithReturnValue_ReturnsResult()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            int result = timer.MeasureExecutionTimeWithExceptionHandling(() => 42, "GetNumber");

            Assert.Equal(42, result);
            Assert.Single(timer.Laps);
        }

        [Fact]
        public void MeasureExecutionTimeWithExceptionHandling_WithReturnValue_RecordsLap_WhenExceptionThrown()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            Assert.Throws<InvalidOperationException>(() =>
                timer.MeasureExecutionTimeWithExceptionHandling<int>(() =>
                {
                    throw new InvalidOperationException("Test exception");
                }, "ExceptionTest"));

            Assert.Single(timer.Laps);
        }

        [Fact]
        public async Task MeasureExecutionTimeWithExceptionHandlingAsync_WithReturnValue_ReturnsResult()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            int result = await timer.MeasureExecutionTimeWithExceptionHandlingAsync(async () =>
            {
                await Task.Delay(10);
                return 100;
            }, "GetAsyncNumber");

            Assert.Equal(100, result);
            Assert.Single(timer.Laps);
        }

        [Fact]
        public async Task MeasureExecutionTimeWithExceptionHandlingAsync_WithReturnValue_RecordsLap_WhenExceptionThrown()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await timer.MeasureExecutionTimeWithExceptionHandlingAsync<int>(async () =>
                {
                    await Task.Delay(10);
                    throw new InvalidOperationException("Test exception");
                }, "AsyncExceptionTest"));

            Assert.Single(timer.Laps);
        }

        [Fact]
        public void Constructor_WithMaxLapCount_SetsMaxLapCount()
        {
            var timer = new ChronolapTimer(maxLapCount: 500);
            Assert.Equal(500, timer.MaxLapCount);
        }

        [Fact]
        public void Constructor_WithMaxLapCountZero_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChronolapTimer(maxLapCount: 0));
        }

        [Fact]
        public void Constructor_WithMaxLapCountNegative_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChronolapTimer(maxLapCount: -1));
        }

        [Fact]
        public void Constructor_WithMinimumLapCountForStatistics_SetsProperty()
        {
            var timer = new ChronolapTimer(minimumLapCountForStatistics: 50);
            Assert.Equal(50, timer.MinimumLapCountForStatistics);
        }

        [Fact]
        public void Constructor_WithMinimumLapCountForStatisticsZero_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChronolapTimer(minimumLapCountForStatistics: 0));
        }

        [Fact]
        public void Constructor_WithLoggerAndParameters_SetsProperties()
        {
            var mockLogger = new Mock<ILogger>();
            var timer = new ChronolapTimer(mockLogger.Object, maxLapCount: 200, minimumLapCountForStatistics: 25);
            
            Assert.Equal(200, timer.MaxLapCount);
            Assert.Equal(25, timer.MinimumLapCountForStatistics);
        }

        [Fact]
        public void MinimumLapCountForStatistics_CanBeChanged()
        {
            var timer = new ChronolapTimer();
            timer.MinimumLapCountForStatistics = 50;
            Assert.Equal(50, timer.MinimumLapCountForStatistics);
        }

        [Fact]
        public void CalculateLapStatistic_UsesProperty_WhenMinimumLapCountNotProvided()
        {
            var timer = new ChronolapTimer(minimumLapCountForStatistics: 10);
            timer.Start();

            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(10);
                timer.Lap($"Lap{i}");
            }

            var min = timer.CalculateLapStatistic(LapStatisticsType.Min);
            Assert.NotNull(min);
        }

        [Fact]
        public void CalculatePercentile_UsesProperty_WhenMinimumLapCountNotProvided()
        {
            var timer = new ChronolapTimer(minimumLapCountForStatistics: 10);
            timer.Start();

            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(10);
                timer.Lap($"Lap{i}");
            }

            var p50 = timer.CalculatePercentile(50);
            Assert.NotNull(p50);
        }

        [Fact]
        public void TotalLapTime_IsCached_AndUpdatedCorrectly()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            Thread.Sleep(20);
            timer.Lap("Lap1");
            var total1 = timer.TotalLapTime;

            Thread.Sleep(30);
            timer.Lap("Lap2");
            var total2 = timer.TotalLapTime;

            Assert.True(total2 > total1);
            Assert.True(total2.TotalMilliseconds >= 50);
        }

        [Fact]
        public void TotalLapTime_IsUpdated_WhenMaxLapCountExceeded()
        {
            var timer = new ChronolapTimer(maxLapCount: 2);
            timer.Start();

            Thread.Sleep(10);
            timer.Lap("Lap1");
            var total1 = timer.TotalLapTime;

            Thread.Sleep(20);
            timer.Lap("Lap2");
            var total2 = timer.TotalLapTime;

            Thread.Sleep(30);
            timer.Lap("Lap3");
            var total3 = timer.TotalLapTime;

            Assert.True(total2 > total1);
            Assert.Equal(2, timer.Laps.Count);
            Assert.True(total3.TotalMilliseconds >= 50);
        }

        [Fact]
        public void TotalLapTime_IsReset_WhenResetCalled()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            Thread.Sleep(20);
            timer.Lap("Lap1");
            Thread.Sleep(30);
            timer.Lap("Lap2");

            Assert.True(timer.TotalLapTime.TotalMilliseconds >= 50);

            timer.Reset();
            Assert.Equal(TimeSpan.Zero, timer.TotalLapTime);
        }

        [Fact]
        public void ThreadSafety_MultipleThreads_CanAddLapsConcurrently()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            const int threadCount = 10;
            const int lapsPerThread = 50;
            var threads = new System.Threading.Thread[threadCount];
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                threads[i] = new System.Threading.Thread(() =>
                {
                    try
                    {
                        for (int j = 0; j < lapsPerThread; j++)
                        {
                            timer.Lap($"Thread{threadId}_Lap{j}");
                            System.Threading.Thread.Sleep(1);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            Assert.Empty(exceptions);
            Assert.Equal(threadCount * lapsPerThread, timer.Laps.Count);
        }

        [Fact]
        public void ThreadSafety_MultipleThreads_CanCalculateStatisticsConcurrently()
        {
            var timer = new ChronolapTimer(minimumLapCountForStatistics: 10);
            timer.Start();

            const int threadCount = 5;
            const int lapsPerThread = 20;
            var threads = new System.Threading.Thread[threadCount];
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                threads[i] = new System.Threading.Thread(() =>
                {
                    try
                    {
                        for (int j = 0; j < lapsPerThread; j++)
                        {
                            timer.Lap($"Thread{threadId}_Lap{j}");
                            System.Threading.Thread.Sleep(1);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            var statsThread = new System.Threading.Thread(() =>
            {
                try
                {
                    while (timer.Laps.Count < threadCount * lapsPerThread)
                    {
                        timer.CalculateLapStatistic(LapStatisticsType.ArithmeticMean);
                        timer.CalculatePercentile(50);
                        timer.GetFastestLap();
                        timer.GetSlowestLap();
                        System.Threading.Thread.Sleep(10);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });

            statsThread.Start();

            foreach (var thread in threads)
            {
                thread.Join();
            }

            statsThread.Join();

            Assert.Empty(exceptions);
            var mean = timer.CalculateLapStatistic(LapStatisticsType.ArithmeticMean);
            Assert.NotNull(mean);
        }

        [Fact]
        public void ThreadSafety_MultipleThreads_CanPauseAndResumeConcurrently()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            const int threadCount = 5;
            var threads = new System.Threading.Thread[threadCount];
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                threads[i] = new System.Threading.Thread(() =>
                {
                    try
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            timer.Pause();
                            System.Threading.Thread.Sleep(1);
                            timer.Resume();
                            System.Threading.Thread.Sleep(1);
                            timer.Lap($"Thread{threadId}_Lap{j}");
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            Assert.Empty(exceptions);
            Assert.Equal(threadCount * 10, timer.Laps.Count);
        }

        [Fact]
        public void ThreadSafety_MultipleThreads_CanMeasureExecutionTimeConcurrently()
        {
            var timer = new ChronolapTimer();
            timer.Start();

            const int threadCount = 5;
            const int operationsPerThread = 20;
            var threads = new System.Threading.Thread[threadCount];
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                threads[i] = new System.Threading.Thread(() =>
                {
                    try
                    {
                        for (int j = 0; j < operationsPerThread; j++)
                        {
                            timer.MeasureExecutionTime(() =>
                            {
                                System.Threading.Thread.Sleep(1);
                            }, $"Thread{threadId}_Op{j}");
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            Assert.Empty(exceptions);
            Assert.Equal(threadCount * operationsPerThread, timer.Laps.Count);
        }

    }

}
