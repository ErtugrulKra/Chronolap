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
        private readonly ILogger _logger;
        private TimeSpan _lastLapTimestamp;
        private bool _isPaused;
        private readonly int _maxLapCount = 1000;

        public bool IsPaused => _isPaused;
        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public IReadOnlyList<LapInfo> Laps => _laps.AsReadOnly();
        public bool IsRunning => _stopwatch.IsRunning;
        public TimeSpan TotalLapTime
        {
            get
            {
                TimeSpan total = TimeSpan.Zero;
                foreach (var lap in _laps)
                {
                    total += lap.Duration;
                }
                return total;
            }
        }

        public ChronolapTimer(ILogger logger) : this()
        {
            _logger = logger;
        }

        public ChronolapTimer()
        {
            _stopwatch = new Stopwatch();
            _laps = new List<LapInfo>();
            _lastLapTimestamp = TimeSpan.Zero;
        }

        public void Start() => _stopwatch.Start();

        public void Stop() => _stopwatch.Stop();

        public void Reset()
        {
            _stopwatch.Reset();
            _laps.Clear();
            _lastLapTimestamp = TimeSpan.Zero;
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
                _laps.RemoveAt(0);
            }

            _laps.Add(lap);
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
                _laps.RemoveAt(0);
            }

            _laps.Add(new LapInfo
            {
                Name = lapName ?? $"Measured Lap {Laps.Count + 1}",
                Duration = lapDuration,
                Timestamp = after
            });

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
                _laps.RemoveAt(0);
            }

            _laps.Add(new LapInfo
            {
                Name = lapName ?? $"Measured Lap {Laps.Count + 1}",
                Duration = lapDuration,
                Timestamp = after
            });

            _lastLapTimestamp = after;
        }

        public async Task MeasureExecutionTimeAsync(Func<Task> asyncAction)
        {
            await MeasureExecutionTimeAsync(asyncAction, asyncAction.Method.Name);
        }


        public double? CalculateLapStatistic(LapStatisticsType statType)
        {
            if (_laps.Count < 30)
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

                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
        }


    }
}
