namespace Krabby.Persistence.Models;

public class AnimeData
{
    public int Aid { get; set; }

    public string Title { get; set; } = string.Empty;

    public int EpisodeCount { get; set; }

    public DateTime LastChecked { get; set; }
}