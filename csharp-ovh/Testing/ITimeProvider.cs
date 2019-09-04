using System;

namespace Ovh.Api.Testing
{
    public interface ITimeProvider
    {
        DateTimeOffset Now { get; }
        DateTimeOffset UtcNow { get; }
    }
}