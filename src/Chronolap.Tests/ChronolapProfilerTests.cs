using Chronolap.Profiler;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace Chronolap.Tests
{
    public class ChronolapProfilerTests
    {
        [Fact]
        public void Profile_Action_RecordsResult()
        {
            // Arrange
            var profiler = new ChronolapProfiler();
            var attribute = new ChronolapProfileAttribute("TestMethod");

            // Act
            profiler.Profile(() => Thread.Sleep(50), attribute, "TestMethod");

            // Assert
            Assert.Single(profiler.Results);
            var result = profiler.Results[0];
            Assert.Equal("TestMethod", result.MethodName);
            Assert.True(result.DurationMs >= 50);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Profile_FuncWithReturnValue_RecordsResultAndReturnsValue()
        {
            // Arrange
            var profiler = new ChronolapProfiler();
            var attribute = new ChronolapProfileAttribute("TestFunc");

            // Act
            var returnValue = profiler.Profile(() =>
            {
                Thread.Sleep(30);
                return 42;
            }, attribute, "TestFunc");

            // Assert
            Assert.Equal(42, returnValue);
            Assert.Single(profiler.Results);
            Assert.Equal("TestFunc", profiler.Results[0].MethodName);
        }

        [Fact]
        public void Profile_WithException_RecordsFailure()
        {
            // Arrange
            var profiler = new ChronolapProfiler();
            var attribute = new ChronolapProfileAttribute("FailingMethod")
            {
                RecordOnException = true
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                profiler.Profile(() =>
                {
                    throw new InvalidOperationException("Test exception");
                }, attribute, "FailingMethod");
            });

            // Assert
            Assert.Single(profiler.Results);
            var result = profiler.Results[0];
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Exception);
            Assert.Equal("Test exception", result.Exception.Message);
        }

        [Fact]
        public void Profile_WithMinimumDuration_FiltersResults()
        {
            // Arrange
            var profiler = new ChronolapProfiler();
            var attribute = new ChronolapProfileAttribute("QuickMethod")
            {
                MinimumDurationMs = 100
            };

            // Act
            profiler.Profile(() => Thread.Sleep(10), attribute, "QuickMethod");

            // Assert
            Assert.Empty(profiler.Results); // Should not record because duration < 100ms
        }

        [Fact]
        public void Profile_WithTags_RecordsTags()
        {
            // Arrange
            var profiler = new ChronolapProfiler();
            var attribute = new ChronolapProfileAttribute("TaggedMethod")
            {
                Tags = "database, critical, slow"
            };

            // Act
            profiler.Profile(() => Thread.Sleep(10), attribute, "TaggedMethod");

            // Assert
            var result = profiler.Results[0];
            Assert.Equal(3, result.Tags.Count);
            Assert.Contains("database", result.Tags);
            Assert.Contains("critical", result.Tags);
            Assert.Contains("slow", result.Tags);
        }

        [Fact]
        public void Profile_WithCategory_RecordsCategory()
        {
            // Arrange
            var profiler = new ChronolapProfiler();
            var attribute = new ChronolapProfileAttribute("CategorizedMethod")
            {
                Category = "API"
            };

            // Act
            profiler.Profile(() => Thread.Sleep(10), attribute, "CategorizedMethod");

            // Assert
            var result = profiler.Results[0];
            Assert.Equal("API", result.Category);
        }

        [Fact]
        public void GetResultsByCategory_FiltersCorrectly()
        {
            // Arrange
            var profiler = new ChronolapProfiler();
            
            profiler.Profile(() => { }, new ChronolapProfileAttribute { Category = "API" }, "Method1");
            profiler.Profile(() => { }, new ChronolapProfileAttribute { Category = "Database" }, "Method2");
            profiler.Profile(() => { }, new ChronolapProfileAttribute { Category = "API" }, "Method3");

            // Act
            var apiResults = profiler.GetResultsByCategory("API");

            // Assert
            Assert.Equal(2, apiResults.Count);
            Assert.All(apiResults, r => Assert.Equal("API", r.Category));
        }

        [Fact]
        public void GetResultsByTag_FiltersCorrectly()
        {
            // Arrange
            var profiler = new ChronolapProfiler();
            
            profiler.Profile(() => { }, new ChronolapProfileAttribute { Tags = "critical, slow" }, "Method1");
            profiler.Profile(() => { }, new ChronolapProfileAttribute { Tags = "fast" }, "Method2");
            profiler.Profile(() => { }, new ChronolapProfileAttribute { Tags = "critical" }, "Method3");

            // Act
            var criticalResults = profiler.GetResultsByTag("critical");

            // Assert
            Assert.Equal(2, criticalResults.Count);
            Assert.All(criticalResults, r => Assert.Contains("critical", r.Tags));
        }

        [Fact]
        public void GetStatistics_CalculatesCorrectly()
        {
            // Arrange
            var profiler = new ChronolapProfiler();
            var attribute = new ChronolapProfileAttribute("TestMethod");

            // Act
            for (int i = 0; i < 5; i++)
            {
                profiler.Profile(() => Thread.Sleep(10 + i * 10), attribute, "TestMethod");
            }

            var stats = profiler.GetStatistics("TestMethod");

            // Assert
            Assert.NotNull(stats);
            Assert.Equal("TestMethod", stats.MethodName);
            Assert.Equal(5, stats.TotalCalls);
            Assert.Equal(5, stats.SuccessCount);
            Assert.Equal(0, stats.FailureCount);
            Assert.Equal(100, stats.SuccessRate);
            Assert.True(stats.MinDuration.TotalMilliseconds >= 10);
            Assert.True(stats.MaxDuration.TotalMilliseconds >= 40);
        }

        [Fact]
        public void GetAllStatistics_ReturnsAllMethods()
        {
            // Arrange
            var profiler = new ChronolapProfiler();

            profiler.Profile(() => { }, new ChronolapProfileAttribute("Method1"), "Method1");
            profiler.Profile(() => { }, new ChronolapProfileAttribute("Method2"), "Method2");
            profiler.Profile(() => { }, new ChronolapProfileAttribute("Method3"), "Method3");

            // Act
            var allStats = profiler.GetAllStatistics();

            // Assert
            Assert.Equal(3, allStats.Count);
        }

        [Fact]
        public void Clear_RemovesAllResults()
        {
            // Arrange
            var profiler = new ChronolapProfiler();
            profiler.Profile(() => { }, new ChronolapProfileAttribute("Test"), "Test");
            profiler.Profile(() => { }, new ChronolapProfileAttribute("Test2"), "Test2");

            // Act
            profiler.Clear();

            // Assert
            Assert.Empty(profiler.Results);
        }

        [Fact]
        public void IsEnabled_DisablesProfiler()
        {
            // Arrange
            var profiler = new ChronolapProfiler();
            profiler.IsEnabled = false;

            // Act
            profiler.Profile(() => Thread.Sleep(10), new ChronolapProfileAttribute("Test"), "Test");

            // Assert
            Assert.Empty(profiler.Results);
        }

        [Fact]
        public void Profile_WithLogger_LogsInformation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var profiler = new ChronolapProfiler(mockLogger.Object);
            var attribute = new ChronolapProfileAttribute("TestMethod");

            // Act
            profiler.Profile(() => Thread.Sleep(10), attribute, "TestMethod");

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v != null && v.ToString()!.Contains("TestMethod")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ExportSummary_ReturnsFormattedString()
        {
            // Arrange
            var profiler = new ChronolapProfiler();
            
            for (int i = 0; i < 3; i++)
            {
                profiler.Profile(() => Thread.Sleep(10), new ChronolapProfileAttribute("Method1"), "Method1");
            }

            // Act
            var summary = profiler.ExportSummary();

            // Assert
            Assert.Contains("ChronolapProfiler Summary", summary);
            Assert.Contains("Method1", summary);
            Assert.Contains("Calls: 3", summary);
        }

        [Fact]
        public void ProfileResult_ToString_ReturnsFormattedString()
        {
            // Arrange
            var result = new ProfileResult
            {
                MethodName = "TestMethod",
                Category = "API",
                Duration = TimeSpan.FromMilliseconds(123.45),
                IsSuccess = true
            };
            result.Tags.Add("critical");

            // Act
            var str = result.ToString();

            // Assert
            Assert.Contains("TestMethod", str);
            Assert.Contains("API", str);
            Assert.Contains("ms", str);
            Assert.Contains("Success", str);
            Assert.Contains("critical", str);
        }

        [Fact]
        public void ProfileStatistics_CalculatesSuccessRate()
        {
            // Arrange
            var stats = new ProfileStatistics
            {
                TotalCalls = 10,
                SuccessCount = 7,
                FailureCount = 3
            };

            // Act
            var successRate = stats.SuccessRate;

            // Assert
            Assert.Equal(70, successRate);
        }
    }
}
