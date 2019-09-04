using System;

namespace Ovh.Api.Testing
{
    public class TimeProvider : ITimeProvider
    {
        DateTimeOffset ITimeProvider.Now => DateTimeOffset.Now;
        DateTimeOffset ITimeProvider.UtcNow => DateTimeOffset.UtcNow;
    }
}