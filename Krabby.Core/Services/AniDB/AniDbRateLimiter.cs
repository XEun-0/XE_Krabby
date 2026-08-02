using System;
using System.Threading;
using System.Threading.Tasks;

namespace Krabby.Core.Services.AniDB;

/// <summary>
/// 
/// </summary>
public class AniDbRateLimiter
{
    /// <summary>
    /// 
    /// </summary>
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// 
    /// </summary>
    private DateTime _lastCallUTC = DateTime.MinValue;

    /// <summary>
    /// 
    /// </summary>
    private readonly TimeSpan _minDelay = TimeSpan.FromSeconds(4);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task WaitAsync()
    {
        await _lock.WaitAsync();

        try
        {   
            // Grab current time in UTC
            var nowUTC = DateTime.UtcNow;
            // Calculate delta from last timestamp in UTC
            var elapsedUTC = nowUTC - _lastCallUTC;

            if (elapsedUTC < _minDelay)
            {
                var delay = _minDelay - elapsedUTC;

                Console.WriteLine($"[RateLimiter] Waiting {delay.TotalMilliseconds} ms");

                await Task.Delay(delay);
            }

            _lastCallUTC = DateTime.UtcNow;

            Console.WriteLine($"[RateLimiter] Proceed at {_lastCallUTC:HH:mm:ss.fff}");
        }
        finally
        {
            _lock.Release();
        }
    }
}