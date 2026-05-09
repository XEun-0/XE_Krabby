using Krabby.Core.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;

namespace Krabby.Core.Services.AniDB;

public enum SessionCacheStatus
{
    Existing,          // Good
    CreatedNew,        // Good
    ExistingFirstTime, // Good
    CannotCreate,      // Bad
    DoesNotExist       // Bad
}

public class AniDbSession
{
    private readonly string CacheFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Krabby",
        "anidb_session.json"
    );

    public SessionCacheStatus SessionStatus = SessionCacheStatus.DoesNotExist;

    private const int SESSION_KEY_EXPIRATION = 20;
    private string? _sessionKey;
    private DateTime? _lastLoggedIn;
    private DateTime? _currSessionLoadedTime;

    public string? SessionKey => _sessionKey;

    public async Task LoadAsync()
    {
        Console.WriteLine("[AniDbSession] Calling LoadAsync");
        if (!File.Exists(CacheFile))
        {
            var dir = Path.GetDirectoryName(CacheFile)!;
            Directory.CreateDirectory(dir);

            Console.WriteLine("[AniDbSession: LoadAsync] No cache file found");
            Console.WriteLine("[AniDbSession: LoadAsync] Creating directory: " + dir);

            SessionCache loadCache = new SessionCache
            {
                SessionKey = "FAKE_SESSION_INIT",
                LastLogin = DateTime.UtcNow,
                LastLogout = DateTime.MinValue
            };

            string loadJson = JsonSerializer.Serialize(loadCache, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(CacheFile, loadJson);

            Console.WriteLine("[AniDbSession: LoadAsync] Created initial cache file with default values");
            Console.WriteLine("[AniDbSession: LoadAsync] Temporary Session Key = " + loadCache.SessionKey);

            _sessionKey = loadCache.SessionKey; // optional for runtime use


            SessionStatus = SessionCacheStatus.CreatedNew;
        } // if (!File.Exists(CacheFile))

        var json = await File.ReadAllTextAsync(CacheFile);
        var cache = JsonSerializer.Deserialize<SessionCache>(json);

        if (cache?.SessionKey != null)
        {
            // Check if session key stored in the session cache file is the default fake one.
            // This only happens if it was just created and it is now being evaluated.
            if (!cache.SessionKey.Equals("FAKE_SESSION_INIT"))
            {
                // If its anything other than the default, the session status should be existing.
                SessionStatus = SessionCacheStatus.Existing;
            }
            else // ServiceLocalSessionKey == FAKE_SESSION_INIT
            {
                // If its still FAKE_SESSION_INIT then that means its the first time this program
                // has started so make sure to set the state accordingly.
                SessionStatus = SessionCacheStatus.ExistingFirstTime;
            }

            // Set the session key locally here.
            _sessionKey = cache.SessionKey;
            Console.WriteLine("[AniDbSession: LoadAsync] Loaded cached session: " + _sessionKey);
        } // if (cache?.ServiceLocalSessionKey != null)

        // Set time and log it
        // NOTE: might have to set it a different variable to represent correctly
        _currSessionLoadedTime = DateTime.Now;
        LastLoggedInTime = cache?.LastLogin ?? DateTime.Now;

        Console.WriteLine("[AniDbSession: LoadAsync] Current Time: " + _currSessionLoadedTime.ToString());
        Console.WriteLine("[AniDbSession: LoadAsync] Current status is: " + StatusToString(SessionStatus));
    }

    public async Task<string?> GetSessionAsync()
    {
        if (!string.IsNullOrWhiteSpace(_sessionKey))
        {
            if (this.HasKeyExpired())
            {
                return null;
            } 
            else 
            { 
                return _sessionKey;
            }
        } 
        else
        {
            return null;
        }
        
    }

    public bool HasKeyExpired()
    {
        // Check expiration criteria
        //TimeOnly updatedCurrTimeUTC = TimeOnly.FromDateTime(DateTime.Now);
        //TimeOnly existingCurrTimeUTC = TimeOnly.FromDateTime(LastLoggedInTime);
        var updatedCurrTimeUTC = DateTime.UtcNow;
        var existingCurrTimeUTC = LastLoggedInTime.ToUniversalTime();

        Console.WriteLine("[AniDbSession: HasKeyExpired] Times: updatedCurrTimeUTC " +
                              updatedCurrTimeUTC.ToString() +
                              ", existingCurrTimeUTC: " +
                              existingCurrTimeUTC.ToString());

        if ((updatedCurrTimeUTC - existingCurrTimeUTC).TotalMinutes >= SESSION_KEY_EXPIRATION)
        {
            return true;
        }

        return false;
    }
    public async Task SaveAsync()
    {
        var cache = new SessionCache
        {
            SessionKey = _sessionKey,
            LastLogin = LastLoggedInTime
        };

        var json = JsonSerializer.Serialize(cache, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(CacheFile, json);

        Console.WriteLine("[AniDbSession: SaveAsync] Saved session");
    }

    public async Task SetSession(string session)
    {
        _sessionKey = session;
        LastLoggedInTime = DateTime.Now;

        await SaveAsync();
    }

    public DateTime LastLoggedInTime
    {
        get 
        {
            return _lastLoggedIn ??= DateTime.Now;
        }
        set { _lastLoggedIn = value; }
    }

    public bool HasSession()
    {
        return !string.IsNullOrWhiteSpace(_sessionKey);
    }

    private string StatusToString(SessionCacheStatus statusIn)
    {
        return statusIn switch
        {
            SessionCacheStatus.Existing => "Cache file exists",
            SessionCacheStatus.CreatedNew => "Created new cache file",
            SessionCacheStatus.ExistingFirstTime => "Cache file is still new",
            SessionCacheStatus.CannotCreate => "Caannot create new cache file",
            SessionCacheStatus.DoesNotExist => "Cache file does not exist",
            // Default
            _ => "ERROR: How did you get here"
        };
    }
}