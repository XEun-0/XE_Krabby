namespace Krabby.Core.Services.AniDB;

public enum AnimeJobState
{
    Idle,
    LoadingAnime,
    LoadingEpisodes,
    Complete,
    Failed
}