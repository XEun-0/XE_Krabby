namespace Krabby.Core.Models;

/// <summary>
/// 
/// </summary>
public class Anime
{
    public int Aid { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ShortTitle { get; set; } = string.Empty;

    public int EpisodeCount { get; set; }

    public string AnimeAirDate { get; set; } = string.Empty;
}

/// <summary>
/// 
/// </summary>
public class Episode
{
    public int Eid { get; set; }

    public string EpisodeNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    // public string Title { get; set; } = string.Empty;

    // public int Length { get; set; }

    // public DateTime? AirDate { get; set; }
}