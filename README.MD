# TubeFin

Easily add content from YouTube to your Jellyfin installation

![](assets/20230726_113636_logo.png)

## Dependencies

Tubefin requires [YouTube-DL/YouTube-DLP](https://github.com/yt-dlp/yt-dlp) and [id3v2](https://sourceforge.net/projects/id3v2/) for full functionality, however YT-DLP is sufficient for operation.

Check the link above to install YT-DLP and install id3v2:

- Debian/Ubuntu `# sudo apt install id3v2`
- Arch `# sudo pacman -S id3v2`

For other OS please check your package manager.

## Install

### Add my Repository

1. In your Admin Dashboard navigate to "Pluginsplugin"
2. Switch to the "Repositories" tab
3. Click "+" and add the Repository `https://raw.githubusercontent.com/AECX/FinTube/master/manifest.json`
   Name it "HenkeGG" or "AECX" - Or whatever helps you remember

### Install and configure the plugin

1. Switch to the "Catalog" tab
2. Search for the "FinTube" plugin and click install
3. Restart the Server and head back to the "Plugins" Sections
4. Click on FinTube and Select "Settings", enter a valid executable for yt-dlp/youtube-dl
5. Optionally: Enter a valid executable for id3v2 to be able to Tag Music with Artist, Title, Album and Track information

Now you are ready to go, head to the "FinTube" plugin page (at the bottom of your dashboard navigation), enter information as desired to start importing from YouTube.
