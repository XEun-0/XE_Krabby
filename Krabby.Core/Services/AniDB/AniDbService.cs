using Krabby.Core.Common;
using Krabby.Core.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;

namespace Krabby.Core.Services.AniDB;

public sealed class AniDbService
{
    private readonly AniDbSession      _session;
    private readonly AniDbAuth         _settings;
    private readonly AniDbRateLimiter  _rateLimiter;
    private readonly AniDbRuntimeState _state;
    private readonly AniDbParser       _parser;
    private readonly AniDbTransport    _transport;
    private readonly SemaphoreSlim     _jobLock = new(1, 1);

    private IPEndPoint? _endpoint;

    private IPEndPoint Endpoint =>
        _endpoint ?? throw new InvalidOperationException("Endpoint not initialized");

    private string SessionKey =>
        _state.SessionKey ?? throw new InvalidOperationException("Session not initialized");

    /// <summary>
    /// Constructor for AniDbService
    /// </summary>
    /// <param name="session"></param>
    /// <param name="rateLimiter"></param>
    /// <param name="settings"></param>
    public AniDbService(AniDbSession        session,
                        AniDbRateLimiter    rateLimiter,
                        IOptions<AniDbAuth> settings)
    {
        // Instantiate helpers
        _session     = session;
        _rateLimiter = rateLimiter;
        _settings    = settings.Value;

        _state     = new AniDbRuntimeState();

        _parser    = new AniDbParser();

        _transport = new AniDbTransport(rateLimiter);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync()
    {
        await _session.LoadAsync();

        _endpoint = await _transport.ResolveEndpoint();

        _state.LoginState =
            _session.HasKeyExpired()
                ? LoginState.Expired
                : LoginState.LoggedOut;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Task<object> GetLoginStatusAsync()
    {
        return Task.FromResult<object>(new
        {
            login = _state.LoginState
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Task<object> GetStatusAsync()
    {
        return Task.FromResult<object>(new
        {
            loginState = _state.LoginState,
            jobState = _state.JobState,
            aid = _state.CurrentAid
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<object> ExecuteLoginAsync()
    {
        if (_state.LoginState == LoginState.LoggedIn)
        {
            return new { error = "Already logged in" };
        }

        _state.LoginState = LoginState.LoggingIn;

        string? sessionKey = await LoginAsync();

        if (string.IsNullOrWhiteSpace(sessionKey))
        {
            _state.LoginState = LoginState.LoggedOut;

            return new { error = "LOGIN FAILED" };
        }

        await _session.SetSession(sessionKey);

        _state.SessionKey = sessionKey;
        _state.LoginState = LoginState.LoggedIn;

        return new
        {
            session_key = sessionKey
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task<string?> LoginAsync()
    {
        string cmd =
            $"AUTH user={_settings.User}" +
            $"&pass={_settings.Password}" +
            $"&protover=3" +
            $"&client={_settings.Client}" +
            $"&clientver={_settings.ClientVersion}";

        string response =
            await _transport.SendAsync(Endpoint, cmd);

        var parts =
            response.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
            return null;

        if (!parts[0].StartsWith("200"))
            return null;

        return parts[1];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aid"></param>
    /// <returns></returns>
    public async Task<object> GetAnimeDataAsync(int aid)
    {
        await _jobLock.WaitAsync();

        try
        {
            if (_state.LoginState != LoginState.LoggedIn)
                return new { error = "Not logged in" };

            if (_state.JobState != AnimeJobState.Idle)
                return new { error = "Another job running" };

            _state.JobState = AnimeJobState.LoadingAnime;

            string raw =
                await _transport.SendAsync(
                    Endpoint,
                    $"ANIME s={SessionKey}&aid={aid}");

            var anime = _parser.ParseAnime(raw);

            _state.CurrentAnime = anime;
            _state.CurrentAid   = anime.Aid;
            _state.EpisodeMax   = anime.EpisodeCount;

            _state.Episodes.Clear();

            _state.JobState = AnimeJobState.Complete;

        //     animeNameRomaji = parts[12];
        //     airDateYear = ParseYear(parts[10]);
        //     episodeMax = int.Parse(parts[1]); // 

        //     result.Add(new
        //     {
        //         nameRomaji = AnimeNameInRomaji,
        //         startingDate = StartingDate,
        //         numEpisodes = episodeMax,
        //         dateYears = parts[10],
        //         aid = parts[0],
        //         animeRaw = response
        //     });
        // }

            // animeNameRomaji = parts[12];
            // airDateYear = ParseYear(parts[10]);
            // episodeMax = int.Parse(parts[1]); // 

            return new {
                nameRomaji = anime.Title,
                startingDate = anime.AnimeAirDate,
                numEpisodes = anime.EpisodeCount,
                // dateYears = anime.,
                aid = anime.Aid,
                animeRaw = raw
            };
        }
        catch (Exception ex)
        {
            _state.JobState = AnimeJobState.Failed;

            return new { error = ex.Message };
        }
        finally
        {
            _jobLock.Release();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<object> GetEpisodeDataAsync()
    {
        await _jobLock.WaitAsync();

        try
        {
            if (_state.CurrentAnime == null)
                return new { error = "No anime loaded" };

            _state.JobState = AnimeJobState.LoadingEpisodes;

            _state.Episodes.Clear();

            Console.WriteLine($" _state.EpisodeMax = { _state.EpisodeMax}");

            for (int ep = 1; ep <= _state.EpisodeMax; ep++)
            {
                string raw = await _transport.SendAsync(Endpoint, $"EPISODE s={SessionKey}&aid={_state.CurrentAid}&epno={ep}");

                var episodes = _parser.ParseEpisodes(raw);

                _state.Episodes.AddRange(episodes);
            }

            _state.JobState = AnimeJobState.Complete;

            return new
            {
                anime    = _state.CurrentAnime,
                episodes = _state.Episodes
            };
        }
        catch (Exception ex)
        {
            _state.JobState = AnimeJobState.Failed;

            return new { error = ex.Message };
        }
        finally
        {
            _jobLock.Release();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Task<object> ClearAnime()
    {
        _state.CurrentAnime = null;
        _state.CurrentAid = -1;
        _state.Episodes.Clear();
        _state.EpisodeMax = 0;

        _state.JobState = AnimeJobState.Idle;

        return Task.FromResult<object>(new
        {
            status = "Anime Cleared"
        });
    }
}