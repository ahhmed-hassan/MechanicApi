namespace MechanicAPI.Tests.Common;


public sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
  public override DateTimeOffset GetUtcNow() => utcNow;
  public override long GetTimestamp() => utcNow.ToUnixTimeMilliseconds();
}