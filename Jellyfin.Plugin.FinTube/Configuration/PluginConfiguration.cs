using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.FinTube.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        exec_YTDL = "/usr/local/bin/yt-dlp";
        exec_ID3 = "/usr/bin/id3v2";
        AudioOnly = true;
        YTUriOrId = "https://youtu.be/abcde";
        DownloadPath = "/";
        Artist = "unknown";
        Title = "unknown";
    }

    /// <summary>
    /// Executable for youtube-dl/youtube-dlp
    /// </summary>
    public string exec_YTDL { get; set; }

    /// <summary>
    /// Executable for ID3v2
    /// </summary>
    public string exec_ID3 { get; set; }

    /// <summary>
    /// Whether to download audio (mp3) or video (mp4)
    /// </summary>
    public bool AudioOnly { get; set; }

    /// <summary>
    /// Set id3 tags
    /// </summary>
    public bool SetId3 { get; set; }

    /// <summary>
    /// id3v2 Artist
    /// </summary>
    public string Artist { get; set; }

    /// <summary>
    /// id3v2 Title/Song name
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// id3v2 track number/index
    /// </summary>
    public int Track { get; set; }

    /// <summary>
    /// id3v2 release year
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// The video to be downloaded
    /// </summary>
    public string YTUriOrId { get; set; }

    /// <summary>
    /// Download folder for yt-dlp
    /// </summary>
    public string DownloadPath { get; set; }
}
