using System;
using System.Threading;
using System.Threading.Tasks;

namespace Krabby.Core.Services.AniDB;

public class AniDbRateLimiter
{
    private readonly SemaphoreSlim _lock = new(1, 1);

    private DateTime _lastCall = DateTime.MinValue;

    // 🔥 safer delay
    private readonly TimeSpan _minDelay = TimeSpan.FromSeconds(4);

    public async Task WaitAsync()
    {
        await _lock.WaitAsync();

        try
        {
            var now = DateTime.UtcNow;
            var elapsed = now - _lastCall;

            if (elapsed < _minDelay)
            {
                var delay = _minDelay - elapsed;

                Console.WriteLine($"[RateLimiter] Waiting {delay.TotalMilliseconds} ms");

                await Task.Delay(delay);
            }

            _lastCall = DateTime.UtcNow;

            Console.WriteLine($"[RateLimiter] Proceed at {_lastCall:HH:mm:ss.fff}");
        }
        finally
        {
            _lock.Release();
        }
    }
}