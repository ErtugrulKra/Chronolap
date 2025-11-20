using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Chronolap
{
    public class ChronolapTimer
    {
        private readonly Stopwatch _stopwatch;
        private readonly List<LapInfo> _laps;
        private readonly ILogger? _logger;
        private TimeSpan _lastLapTimestamp;
        private bool _isPaused;
        private readonly int _maxLapCount;
        private TimeSpan _cachedTotalLapTime;
        private bool _isTotalLapTimeDirty;

        public bool IsPaused => _isPaused;
        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public IReadOnlyList<LapInfo> Laps => _laps.AsReadOnly();
        public bool IsRunning => _stopwatch.IsRunning;
        public int MaxLapCount => _maxLapCount;
        public int MinimumLapCountForStatistics { get; set; } = 30;

        public TimeSpan TotalLapTime
        {
            get
            {
                if (_isTotalLapTimeDirty)
                {
                    _cachedTotalLapTime = TimeSpan.Zero;
                    foreach (var lap in _laps)
                    {
                        _cachedTotalLapTime += lap.Duration;
                    }
                    _isTotalLapTimeDirty = false;
                }
                return _cachedTotalLapTime;
            }
        }

        public ChronolapTimer(ILogger logger, int maxLapCount = 1000, int minimumLapCountForStatistics = 30) 
            : this(maxLapCount, minimumLapCountForStatistics)
        {
            _logger = logger;
        }

        public ChronolapTimer(int maxLapCount = 1000, int minimumLapCountForStatistics = 30)
        {
            if (maxLapCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxLapCount), maxLapCount, "MaxLapCount must be greater than 0");
            
            if (minimumLapCountForStatistics <= 0)
                throw new ArgumentOutOfRangeException(nameof(minimumLapCountForStatistics), minimumLapCountForStatistics, "MinimumLapCountForStatistics must be greater than 0");

            _stopwatch = new Stopwatch();
            _laps = new List<LapInfo>();
            _lastLapTimestamp = TimeSpan.Zero;
            _maxLapCount = maxLapCount;
            MinimumLapCountForStatistics = minimumLapCountForStatistics;
            _cachedTotalLapTime = TimeSpan.Zero;
            _isTotalLapTimeDirty = false;
        }

        public void Start() => _stopwatch.Start();

        public void Stop() => _stopwatch.Stop();

        public void Reset()
        {
            _stopwatch.Reset();
            _laps.Clear();
            _lastLapTimestamp = TimeSpan.Zero;
            _cachedTotalLapTime = TimeSpan.Zero;
            _isTotalLapTimeDirty = false;
        }

        public void Pause()
        {
            if (_stopwatch.IsRunning)
            {
                _stopwatch.Stop();
                _isPaused = true;
            }
        }

        public void Resume()
        {
            if (_isPaused)
            {
                _stopwatch.Start();
                _isPaused = false;
            }
        }

        public void Lap(string name)
        {
            var now = _stopwatch.Elapsed;
            var lapDuration = now - _lastLapTimestamp;

            var lap = new LapInfo
            {
                Name = name ?? $"Lap {Laps.Count + 1}",
                Duration = lapDuration,
                Timestamp = now
            };

            if (_laps.Count >= _maxLapCount)
            {
                var removedLap = _laps[0];
                _laps.RemoveAt(0);
                _cachedTotalLapTime -= removedLap.Duration;
            }

            _laps.Add(lap);
            _cachedTotalLapTime += lap.Duration;
            _lastLapTimestamp = now;

            _logger?.LogInformation("Lap recorded: {LapName}, Duration: {Duration} ms", lap.Name, lap.Duration.TotalMilliseconds);

        }

        public void MeasureExecutionTime(Action action, string lapName)
        {
            var before = _stopwatch.Elapsed;

            action();

            var after = _stopwatch.Elapsed;
            var lapDuration = after - before;

            if (_laps.Count >= _maxLapCount)
            {
                var removedLap = _laps[0];
                _laps.RemoveAt(0);
                _cachedTotalLapTime -= removedLap.Duration;
            }

            var lap = new LapInfo
            {
                Name = lapName ?? $"Measured Lap {Laps.Count + 1}",
                Duration = lapDuration,
                Timestamp = after
            };

            _laps.Add(lap);
            _cachedTotalLapTime += lap.Duration;
            _lastLapTimestamp = after;
        }

        public void MeasureExecutionTime(Action action)
        {
            MeasureExecutionTime(action, action.Method.Name);
        }

        public async Task MeasureExecutionTimeAsync(Func<Task> asyncAction, string lapName)
        {
            var before = _stopwatch.Elapsed;

            await asyncAction();

            var after = _stopwatch.Elapsed;
            var lapDuration = after - before;

            if (_laps.Count >= _maxLapCount)
            {
                var removedLap = _laps[0];
                _laps.RemoveAt(0);
                _cachedTotalLapTime -= removedLap.Duration;
            }

            var lap = new LapInfo
            {
                Name = lapName ?? $"Measured Lap {Laps.Count + 1}",
                Duration = lapDuration,
                Timestamp = after
            };

            _laps.Add(lap);
            _cachedTotalLapTime += lap.Duration;
            _lastLapTimestamp = after;
        }

        public async Task MeasureExecutionTimeAsync(Func<Task> asyncAction)
        {
            await MeasureExecutionTimeAsync(asyncAction, asyncAction.Method.Name);
        }

        public T MeasureExecutionTime<T>(Func<T> func, string lapName)
        {
            var before = _stopwatch.Elapsed;

            T result = func();

            var after = _stopwatch.Elapsed;
            var lapDuration = after - before;

            if (_laps.Count >= _maxLapCount)
            {
                var removedLap = _laps[0];
                _laps.RemoveAt(0);
                _cachedTotalLapTime -= removedLap.Duration;
            }

            var lap = new LapInfo
            {
                Name = lapName ?? $"Measured Lap {Laps.Count + 1}",
                Duration = lapDuration,
                Timestamp = after
            };

            _laps.Add(lap);
            _cachedTotalLapTime += lap.Duration;
            _lastLapTimestamp = after;
            return result;
        }

        public T MeasureExecutionTime<T>(Func<T> func)
        {
            return MeasureExecutionTime(func, func.Method.Name);
        }

        public async Task<T> MeasureExecutionTimeAsync<T>(Func<Task<T>> asyncFunc, string lapName)
        {
            var before = _stopwatch.Elapsed;

            T result = await asyncFunc();

            var after = _stopwatch.Elapsed;
            var lapDuration = after - before;

            if (_laps.Count >= _maxLapCount)
            {
                var removedLap = _laps[0];
                _laps.RemoveAt(0);
                _cachedTotalLapTime -= removedLap.Duration;
            }

            var lap = new LapInfo
            {
                Name = lapName ?? $"Measured Lap {Laps.Count + 1}",
                Duration = lapDuration,
                Timestamp = after
            };

            _laps.Add(lap);
            _cachedTotalLapTime += lap.Duration;
            _lastLapTimestamp = after;
            return result;
        }

        public async Task<T> MeasureExecutionTimeAsync<T>(Func<Task<T>> asyncFunc)
        {
            return await MeasureExecutionTimeAsync(asyncFunc, asyncFunc.Method.Name);
        }

        public void MeasureExecutionTimeWithExceptionHandling(Action action, string lapName)
        {
            var before = _stopwatch.Elapsed;

            try
            {
                action();
            }
            finally
            {
                var after = _stopwatch.Elapsed;
                var lapDuration = after - before;

                if (_laps.Count >= _maxLapCount)
                {
                    var removedLap = _laps[0];
                    _laps.RemoveAt(0);
                    _cachedTotalLapTime -= removedLap.Duration;
                }

                var lap = new LapInfo
                {
                    Name = lapName ?? $"Measured Lap {Laps.Count + 1}",
                    Duration = lapDuration,
                    Timestamp = after
                };

                _laps.Add(lap);
                _cachedTotalLapTime += lap.Duration;
                _lastLapTimestamp = after;
            }
        }

        public void MeasureExecutionTimeWithExceptionHandling(Action action)
        {
            MeasureExecutionTimeWithExceptionHandling(action, action.Method.Name);
        }

        public async Task MeasureExecutionTimeWithExceptionHandlingAsync(Func<Task> asyncAction, string lapName)
        {
            var before = _stopwatch.Elapsed;

            try
            {
                await asyncAction();
            }
            finally
            {
                var after = _stopwatch.Elapsed;
                var lapDuration = after - before;

                if (_laps.Count >= _maxLapCount)
                {
                    var removedLap = _laps[0];
                    _laps.RemoveAt(0);
                    _cachedTotalLapTime -= removedLap.Duration;
                }

                var lap = new LapInfo
                {
                    Name = lapName ?? $"Measured Lap {Laps.Count + 1}",
                    Duration = lapDuration,
                    Timestamp = after
                };

                _laps.Add(lap);
                _cachedTotalLapTime += lap.Duration;
                _lastLapTimestamp = after;
            }
        }

        public async Task MeasureExecutionTimeWithExceptionHandlingAsync(Func<Task> asyncAction)
        {
            await MeasureExecutionTimeWithExceptionHandlingAsync(asyncAction, asyncAction.Method.Name);
        }

        public T MeasureExecutionTimeWithExceptionHandling<T>(Func<T> func, string lapName)
        {
            var before = _stopwatch.Elapsed;
            T result;

            try
            {
                result = func();
            }
            finally
            {
                var after = _stopwatch.Elapsed;
                var lapDuration = after - before;

                if (_laps.Count >= _maxLapCount)
                {
                    var removedLap = _laps[0];
                    _laps.RemoveAt(0);
                    _cachedTotalLapTime -= removedLap.Duration;
                }

                var lap = new LapInfo
                {
                    Name = lapName ?? $"Measured Lap {Laps.Count + 1}",
                    Duration = lapDuration,
                    Timestamp = after
                };

                _laps.Add(lap);
                _cachedTotalLapTime += lap.Duration;
                _lastLapTimestamp = after;
            }

            return result;
        }

        public T MeasureExecutionTimeWithExceptionHandling<T>(Func<T> func)
        {
            return MeasureExecutionTimeWithExceptionHandling(func, func.Method.Name);
        }

        public async Task<T> MeasureExecutionTimeWithExceptionHandlingAsync<T>(Func<Task<T>> asyncFunc, string lapName)
        {
            var before = _stopwatch.Elapsed;
            T result;

            try
            {
                result = await asyncFunc();
            }
            finally
            {
                var after = _stopwatch.Elapsed;
                var lapDuration = after - before;

                if (_laps.Count >= _maxLapCount)
                {
                    var removedLap = _laps[0];
                    _laps.RemoveAt(0);
                    _cachedTotalLapTime -= removedLap.Duration;
                }

                var lap = new LapInfo
                {
                    Name = lapName ?? $"Measured Lap {Laps.Count + 1}",
                    Duration = lapDuration,
                    Timestamp = after
                };

                _laps.Add(lap);
                _cachedTotalLapTime += lap.Duration;
                _lastLapTimestamp = after;
            }

            return result;
        }

        public async Task<T> MeasureExecutionTimeWithExceptionHandlingAsync<T>(Func<Task<T>> asyncFunc)
        {
            return await MeasureExecutionTimeWithExceptionHandlingAsync(asyncFunc, asyncFunc.Method.Name);
        }


        public double? CalculateLapStatistic(LapStatisticsType statType, int? minimumLapCount = null)
        {
            int requiredCount = minimumLapCount ?? MinimumLapCountForStatistics;
            if (_laps.Count < requiredCount)
                return null;

            var durations = _laps.Select(l => l.Duration.TotalMilliseconds).OrderBy(d => d).ToList();

            switch (statType)
            {
                case LapStatisticsType.ArithmeticMean:
                    return durations.Average();

                case LapStatisticsType.Median:
                    int count = durations.Count;
                    if (count % 2 == 0)
                        return (durations[count / 2 - 1] + durations[count / 2]) / 2;
                    else
                        return durations[count / 2];

                case LapStatisticsType.StandardDeviation:
                    double avg = durations.Average();
                    double sumSquares = durations.Sum(d => Math.Pow(d - avg, 2));
                    return Math.Sqrt(sumSquares / (durations.Count - 1));

                case LapStatisticsType.Min:
                    return durations.Min();

                case LapStatisticsType.Max:
                    return durations.Max();

                case LapStatisticsType.Variance:
                    avg = durations.Average();
                    sumSquares = durations.Sum(d => Math.Pow(d - avg, 2));
                    return sumSquares / (durations.Count - 1);

                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
        }

        public double? CalculatePercentile(double percentile, int? minimumLapCount = null)
        {
            int requiredCount = minimumLapCount ?? MinimumLapCountForStatistics;
            if (_laps.Count < requiredCount)
                return null;

            if (percentile < 0 || percentile > 100)
                throw new ArgumentOutOfRangeException(nameof(percentile), percentile, "Percentile must be between 0 and 100");

            var durations = _laps.Select(l => l.Duration.TotalMilliseconds).OrderBy(d => d).ToList();
            
            if (durations.Count == 0)
                return null;

            if (percentile == 0)
                return durations[0];
            
            if (percentile == 100)
                return durations[durations.Count - 1];

            double index = (percentile / 100.0) * (durations.Count - 1);
            int lowerIndex = (int)Math.Floor(index);
            int upperIndex = (int)Math.Ceiling(index);

            if (lowerIndex == upperIndex)
                return durations[lowerIndex];

            double weight = index - lowerIndex;
            return durations[lowerIndex] * (1 - weight) + durations[upperIndex] * weight;
        }

        public LapInfo? GetFastestLap()
        {
            if (_laps.Count == 0)
                return null;

            return _laps.OrderBy(l => l.Duration).First();
        }

        public LapInfo? GetSlowestLap()
        {
            if (_laps.Count == 0)
                return null;

            return _laps.OrderByDescending(l => l.Duration).First();
        }


    }
}
