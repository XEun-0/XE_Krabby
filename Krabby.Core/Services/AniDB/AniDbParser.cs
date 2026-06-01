using Krabby.Core.Models;

namespace Krabby.Core.Services.AniDB;

/// <summary>
/// 
/// </summary>
public class AniDbParser
{
    protected int _localAid = 0;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Anime ParseAnime(string response)
    {
        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            // skip header
            if (!char.IsDigit(line[0]))
                continue;

            var parts = line.Split('|');

            // Headers will have no '|' characters so skip
            if (parts.Length < 3)
                continue;
            
            // nameRomaji = AnimeNameInRomaji,
            //     startingDate = StartingDate,
            //     numEpisodes = episodeMax,
            //     dateYears = parts[10],
            //     aid = parts[0],
            //     animeRaw = response

            // Saving Aid locally in here just in case
            _localAid = int.Parse(parts[0]);
            // animeNameRomaji = parts[12];
            // airDateYear = ParseYear(parts[10]);
            // episodeMax = int.Parse(parts[1]); // 

            // Construct new Anime object.
            return new Anime
            {
                Aid = _localAid,
                EpisodeCount = int.Parse(parts[1]),
                AnimeAirDate = ParseYear(parts[10]),
                Title = parts[12]
            };
        }

        throw new Exception("Unable to parse anime");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public List<Episode> ParseEpisodes(string response)
    {
        var result = new List<Episode>();

        var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            // skip header
            if (!char.IsDigit(line[0]))
                continue;

            var parts = line.Split('|');

            if (parts.Length < 3)
                continue;

            var episode = new Episode
            {
                Eid = int.Parse(parts[0]),
                EpisodeNumber = EpisodePrefixTypeMap(parts[10]) + parts[5],
                Type = EpisodeTypeMap(parts[10])
            };

            Console.WriteLine(
                $"Episode Parsed -> " +
                $"Eid={episode.Eid}, " +
                $"EpisodeNumber={episode.EpisodeNumber}, " +
                $"Type={episode.Type}");

            result.Add(episode);
        }

        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    private static string ParseYear(string date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return "";

        return date.Split('-')[0];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeIn"></param>
    /// <returns></returns>
    private static string EpisodePrefixTypeMap(string typeIn)
    {
        return typeIn switch
        {
            "1" => "",
            "2" => "S",
            "3" => "C",
            "4" => "T",
            "5" => "P",
            "6" => "O",
            _ => "?"
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="typeIn"></param>
    /// <returns></returns>
    private static string EpisodeTypeMap(string typeIn)
    {
        return typeIn switch
        {
            "1" => "Regular",
            "2" => "Special",
            "3" => "Credit",
            "4" => "Trailer",
            "5" => "Parody",
            "6" => "Other",
            _ => "Unknown"
        };
    }
}