using System;

namespace Microsoft.Extensions.Internal
{
    public class TestClock : ISystemClock
    {
        public DateTimeOffset UtcNow { get; private set; } = new DateTimeOffset(2020, 1, 1, 1, 0, 0, offset: TimeSpan.Zero);

        public TestClock Add(TimeSpan timeSpan)
        {
            UtcNow = UtcNow.Add(timeSpan);
            return this;
        }
    }
}
