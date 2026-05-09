namespace Krabby.Core.Models;

public class Episode
{
    public int Eid { get; set; }

    public int Aid { get; set; }

    public string EpisodeNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int Length { get; set; }

    public DateTime? AirDate { get; set; }
}