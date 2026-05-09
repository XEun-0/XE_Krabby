using Krabby.Core.Models;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Krabby.Core.Services.AniDB;

public class AniDbService
{
    private readonly AniDbAuth _settings;

    private const string NOT_LOGGED_IN_ERROR = "Not Logged in.";
    private const string ALREADY_LOGGED_IN_ERROR = "Already logged in.";
    private const string SESSION_KEY_EXPIRED = "Session key has expired, login again.";
    private const string TIMEOUT_ERROR = "TIMEOUT";
    private const string ANIME_CURRENTLY_LOADED = "Anime is already loaded";
    private const string ANIME_NOT_LOADED = "Anime is not loaded";
    private int episodeCounter = 1;
    private int episodeMax = 12;
    private bool animeLoaded = false;
    private bool needToRelog = false;
    private int currWorkingAid = -1;
    private string? animeNameRomaji;
    private string? airDateYear;
    // Readonlys
    private readonly AniDbSession _session;
    private readonly AniDbRateLimiter _rateLimiter;
    
    List<object> episodeData = new List<object>();

    private bool _loginStatus = false;

    private readonly UdpClient udp;
    private IPEndPoint? _endpoint;
    private string? _sessionKey;
    private bool episodesDataGathering = false;
    // Accesors for nullables
    private IPEndPoint Endpoint =>
    _endpoint ?? throw new InvalidOperationException("Endpoint not initialized");
    private AniDbRateLimiter RateLimiter =>
    _rateLimiter ?? throw new InvalidOperationException("RateLimiter not initialized");

    private string ServiceLocalSessionKey =>
    _sessionKey ?? throw new InvalidOperationException("SessionKey not initialized");

    private string AnimeNameInRomaji =>
    animeNameRomaji ?? throw new InvalidOperationException("AnimeNameInRomaji not initialized");
    private string StartingDate =>
    airDateYear ?? throw new InvalidOperationException("StartingDate not initialized");
    public AniDbService(AniDbSession session, AniDbRateLimiter rateLimiter, IOptions<AniDbAuth> settings)
    {   
        // Add necessary components
        _session = session;
        _rateLimiter = rateLimiter;
        _settings = settings.Value;

        udp = new UdpClient(9001);
        
        Console.WriteLine("[AniDbService: AniDbService] Session and udpIn client created");
    }

    public async Task InitializeAsync()
    {
        Console.WriteLine("[AniDbService: InitializeAsync] Initializing...");

        await _session.LoadAsync();

        if (_session.HasKeyExpired())
        {
            Console.WriteLine("[AniDbService: InitializeAsync] Key has expired btw.");
        }
        else
        {
            Console.WriteLine("[AniDbService: InitializeAsync] Key is good.");
        }
        _endpoint = await ResolveEndpoint();
        Console.WriteLine("[AniDbService: InitializeAsync] Session load complete");
    }

    public async Task<object> GetLoginStatusAsync()
    {
        return new
        {
            login_status = _loginStatus
        };
    }

    public async Task<object> ExecuteLoginAsync()
    {
        if (_session.HasKeyExpired())
        {
            _loginStatus = false;
        }

        if (!_loginStatus)
        {
            
            if (_session.HasKeyExpired() || _session.SessionStatus == SessionCacheStatus.ExistingFirstTime || needToRelog)
            {
                _sessionKey = await Login(udp, Endpoint);

                if (needToRelog) { needToRelog = false; }
                if (string.IsNullOrWhiteSpace(ServiceLocalSessionKey))
                {
                    return new
                    {
                        error = "LOGIN FAILED",
                        login_status = _loginStatus
                    };
                }
                else
                {
                    // SetSession sets the current sessionKey as well as well as the login
                    // time. Should only update session cache if it's expired.
                    await _session.SetSession(ServiceLocalSessionKey);
                    _loginStatus = true;
                }

                return new
                {
                    session_key = ServiceLocalSessionKey,
                    login_status = _loginStatus
                };
            }
            else
            {
                _loginStatus = true;
                _sessionKey = await _session.GetSessionAsync();
                return new
                {
                    session_key = ServiceLocalSessionKey,
                    login_status = _loginStatus
                };
            }
        }

        return new
        {
            error = ALREADY_LOGGED_IN_ERROR
        };
    }

    public async Task<object> GetAnimeDataAsync(int aid)
    {
        if (_loginStatus)
        {
            var currentSessionKey = await _session.GetSessionAsync();
            
            if (currentSessionKey != null)
            {
                if (!animeLoaded)
                {
                    // Gather anime metadata first
                    var animeRaw = await SendRaw(udp, Endpoint, 
                                                 $"ANIME s={currentSessionKey}&aid={aid}");

                    // New anime loaded
                    if (!animeRaw.Equals(TIMEOUT_ERROR))
                    {
                        animeLoaded = true;
                    }
                    else if (animeRaw.Contains("501"))
                    {
                        animeRaw = "Need to login again.";
                        needToRelog = true;
                        _loginStatus = false;
                    }
                    else
                    {
                        animeLoaded = false;
                    }
                    
                    currWorkingAid = aid;
                    episodeCounter = 1;
                    episodeData.Clear();

                    return new
                    {
                        animeData = ParseAnimeData(animeRaw)
                    };
                }
                else
                {
                    // Anime is already loaded and working, show error and aid
                    return new
                    {   
                        aid,
                        error = ANIME_CURRENTLY_LOADED
                    };
                }
            } 
            else // Session Key has expired, login again.
            {
                _loginStatus = false;
                return new
                {
                    error = SESSION_KEY_EXPIRED
                };
            }
        }

        return new
        {
            error = NOT_LOGGED_IN_ERROR
        };
    }
    
    public async Task<object> GetAnimeLoadedAsync()
    {
        // If an anime is loaded in the api, make sure
        // to respond correctly where we get the anidbid
        // as well as a status
        if (animeLoaded)
        {
            return new
            {
                anime_loaded = animeLoaded,
                anime_aid = currWorkingAid
            };
        }
        return new
        {
            animeLoaded,
            error = ANIME_NOT_LOADED
        };
    }

    public async Task<object> GetEpisodeDataAsync()
    {
        if (animeLoaded)
        {
            if (!episodesDataGathering)
            {
                episodesDataGathering = true; //episodeCounter

                Console.WriteLine($"episodeMax = {episodeMax}");
                for (int i = 1; i <= episodeMax; i++)
                {
                    episodeCounter = i;
                    Console.WriteLine("REACHED HERE AT GetEpisodeDataAsync " + ServiceLocalSessionKey + " " + currWorkingAid) ;
                    string episodeRaw =
                    await SendRaw(udp, Endpoint, $"EPISODE s={ServiceLocalSessionKey}&aid={currWorkingAid}&epno={episodeCounter}");

                    var parsed = ParseEpisodes(episodeRaw);

                    episodeData.AddRange(parsed);
                }

                return new
                {
                    animeName = AnimeNameInRomaji,
                    airDateYear = StartingDate,
                    session_key = ServiceLocalSessionKey,
                    aid = currWorkingAid,
                    episodeData
                };
            }
            else
            {
                return new
                {
                    error = "Episodes data currently gathering"
                };
            }
        }

        return new
        {
            error = ANIME_NOT_LOADED
        };
    }

    private List<object> ParseEpisodes(string response)
    {
        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var result = new List<object>();

        foreach (var line in lines)
        {
            // skip header
            if (!char.IsDigit(line[0]))
                continue;

            var parts = line.Split('|');

            if (parts.Length < 3)
                continue;
            
            result.Add(new
            {
                eid = int.Parse(parts[0]),
                episodeNumber = EpisodePrefixTypeMap(parts[10]) + parts[5],
                type = EpisodeTypeMap(parts[10])
            });
        }

        return result;
    }

    // 10 for date aired, 12 for romaji name
    private List<object> ParseAnimeData(string response)
    {
        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var result = new List<object>();

        foreach (var line in lines)
        {
            // skip header
            if (!char.IsDigit(line[0]))
                continue;

            var parts = line.Split('|');

            if (parts.Length < 3)
                continue;

            animeNameRomaji = parts[12];
            airDateYear = ParseYear(parts[10]);

            result.Add(new
            {
                nameRomaji = AnimeNameInRomaji,
                startingDate = StartingDate,
                date = parts[10],
                aid = parts[0],
                animeRaw = response
            });
        }

        return result;
    }

    private string EpisodePrefixTypeMap(string typeIn)
    {
        string result;

        switch (typeIn)
        {
            case "1":
                result = "";   // regular episode (no prefix)
                break;
            case "2":
                result = "S";  // special
                break;
            case "3":
                result = "C";  // credit
                break;
            case "4":
                result = "T";  // trailer
                break;
            case "5":
                result = "P";  // parody
                break;
            case "6":
                result = "O";  // other
                break;
            default:
                result = "?";  // unknown
                break;
        }

        return result;
    }

    private string EpisodeTypeMap(string typeIn)
    {
        string result;

        switch (typeIn)
        {
            case "1":
                result = "Regular";   // regular episode (no prefix)
                break;
            case "2":
                result = "Special";  // special
                break;
            case "3":
                result = "Credit";  // credit
                break;
            case "4":
                result = "Trailer";  // trailer
                break;
            case "5":
                result = "Parody";  // parody
                break;
            case "6":
                result = "Other";  // other
                break;
            default:
                result = "Unknown";  // unknown
                break;
        }

        return result;
    }

    string ParseYear(string date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return "";

        return date.Split('-')[0];
    }

    public async Task<object> GetEpisodesDataStatusAsync()
    {
        if (episodesDataGathering)
        {
            return new
            {
                status = "Currently working"
            };
        }
        
        return new
        {
            error = ""
        };
    }

    public async Task<object> GetEpisodesDataResultAsync()
    {
        return new
        {
            error = ""
        };
    }

    private async Task<IPEndPoint> ResolveEndpoint()
    {
        var addresses = await Dns.GetHostAddressesAsync("api.anidb.net");
        var ip = addresses[0];

        return new IPEndPoint(ip, 9000);
    }
    
    private async Task<string?> Login(UdpClient udp, IPEndPoint endpoint)
    {
        var cmd =
            $"AUTH user={_settings.User}&pass={_settings.Password}&protover=3&client={_settings.Client}&clientver={_settings.ClientVersion}";

        var response = await SendRaw(udp, endpoint, cmd);

        Console.WriteLine("LOGIN RESPONSE: " + response);

        var parts = response.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
            return null;

        if (!parts[0].StartsWith("200"))
            return null;

        return parts[1];
    }
    
    private async Task<string> SendRaw(UdpClient udpIn, IPEndPoint endpointIn, string message)
    {
        Console.WriteLine("\n---- SEND ----");
        Console.WriteLine(message);

        var bytes = Encoding.UTF8.GetBytes(message);
        
        await RateLimiter.WaitAsync();
        
        await udpIn.SendAsync(bytes, bytes.Length, endpointIn);

        var receiveTask = udpIn.ReceiveAsync();
        var timeoutTask = Task.Delay(10000);

        var completed = await Task.WhenAny(receiveTask, timeoutTask);

        if (completed == timeoutTask)
            return TIMEOUT_ERROR;

        var response = receiveTask.Result;

        var text = Encoding.UTF8.GetString(response.Buffer);

        Console.WriteLine("---- RESPONSE ----");
        Console.WriteLine(text);

        return text;
    }
}