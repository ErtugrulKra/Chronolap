using System;

namespace Chronolap
{
    public class LapInfo
    {
        public string Name { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public TimeSpan Timestamp { get; set; }

        public override string ToString() =>
            $"{Name}: {Duration.TotalMilliseconds:N0} ms (At {Timestamp.TotalMilliseconds:N0} ms)";
    }
}
