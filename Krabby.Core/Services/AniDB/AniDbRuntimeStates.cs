using Krabby.Core.Models;

namespace Krabby.Core.Services.AniDB;

/// <summary>
/// 
/// </summary>
public class AniDbRuntimeState
{
    public LoginState       LoginState { get; set; }    = LoginState.LoggedOut;
    public AnimeJobState    JobState { get; set; }      = AnimeJobState.Idle;
    public List<Episode>    Episodes { get; }           = new();
    public Anime?           CurrentAnime { get; set; }
    public string?          SessionKey { get; set; }
    public bool             NeedRelog { get; set; }
    public int              CurrentAid { get; set; }    = -1;
    public int              EpisodeCounter { get; set; }
    public int              EpisodeMax { get; set; }
}