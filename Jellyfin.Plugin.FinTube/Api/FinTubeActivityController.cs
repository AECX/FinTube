using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Jellyfin.Data.Entities;
using Jellyfin.Plugin.FinTube.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.FinTube.Api;

[ApiController]
[Authorize(Roles = "Administrator")]
[Route("fintube")]
[Produces(MediaTypeNames.Application.Json)]
public class FinTubeActivityController : ControllerBase
{
        private readonly ILogger<FinTubeActivityController> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IFileSystem _fileSystem;
        private readonly IServerConfigurationManager _config;
        private readonly IUserManager _userManager;
        private readonly ILibraryManager _libraryManager;

        public FinTubeActivityController(
            ILoggerFactory loggerFactory,
            IFileSystem fileSystem,
            IServerConfigurationManager config,
            IUserManager userManager,
            ILibraryManager libraryManager)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<FinTubeActivityController>();
            _fileSystem = fileSystem;
            _config = config;
            _userManager = userManager;
            _libraryManager = libraryManager;

            _logger.LogInformation("FinTubeActivityController Loaded");
        }

        public class FinTubeData
        {
            public string ytid {get; set;} = "";
            public string targetlibrary{get; set;} = "";
            public string targetfolder{get; set;} = "";
            public string targetfilename { get; set; } = "";
            public bool audioonly{get; set;} = false;
            public bool preferfreeformat{get; set;} = false;
            public string videoresolution{get; set;} = "";
            public string artist{get; set;} = "";
            public string album{get; set;} = "";
            public string title{get; set;} = "";
            public int track{get; set;} = 0;

        }

        [HttpPost("submit_dl")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<Dictionary<string, object>> FinTubeDownload([FromBody] FinTubeData data)
        {
            try
            {
                _logger.LogInformation("FinTubeDownload : {ytid} to {targetfoldeer}, prefer free format: {preferfreeformat} audio only: {audioonly}", data.ytid, data.targetfolder, data.preferfreeformat, data.audioonly);

                Dictionary<string, object> response = new Dictionary<string, object>();
                PluginConfiguration? config = Plugin.Instance?.Configuration;

                if (config != null)
                {
                    String status = "";

                    // check binaries
                    if(!System.IO.File.Exists(config.exec_YTDL))
                        throw new Exception("YT-DL Executable configured incorrectly");
                    
                    bool hasid3v2 = System.IO.File.Exists(config.exec_ID3);
                    

                    // Ensure proper / separator
                    data.targetfolder = String.Join("/", data.targetfolder.Split("/", StringSplitOptions.RemoveEmptyEntries));
                    String targetPath = data.targetlibrary.EndsWith("/") ? data.targetlibrary + data.targetfolder : data.targetlibrary + "/" + data.targetfolder;
                    // Create Folder if it doesn't exist
                    if(!System.IO.Directory.CreateDirectory(targetPath).Exists)
                        throw new Exception("Directory could not be created");


                    // Check for tags
                    bool hasTags = 1 < (data.title.Length + data.album.Length + data.artist.Length + data.track.ToString().Length);

                    // Save file with ytdlp as mp4 or mp3 depending on audioonly and free format preference
                    String targetFilename;
                    String targetExtension = (data.preferfreeformat ? (data.audioonly ? @".ogg" : @".webm") : (data.audioonly ? @".mp3" : @".mp4"));

                    if (!String.IsNullOrWhiteSpace(data.targetfilename))
                        targetFilename = System.IO.Path.Combine(targetPath, $"{data.targetfilename}");
                    else if (data.audioonly && hasTags && data.title.Length > 1) // Use title Tag for filename
                        targetFilename = System.IO.Path.Combine(targetPath, $"{data.title}");
                    else // Use YTID as filename
                        targetFilename = System.IO.Path.Combine(targetPath, $"{data.ytid}");

                    // Check if filename exists
                    if(System.IO.File.Exists(targetFilename))
                        throw new Exception($"File {targetFilename} already exists");

                    status += $"Filename: {targetFilename}<br>";

                    String args;
                    if(data.audioonly)
                    {
                        args = "-x";
                        if(data.preferfreeformat)
                            args += " --audio-format vorbis";
                        else
                            args += " --audio-format mp3";
                        args += $" -o \"{targetFilename}.%(ext)s\" -- {data.ytid}";
                    }
                    else
                    {
                        if(data.preferfreeformat)
                            args = "--prefer-free-format";
                        else
                            args = "-f mp4";
                        if(!string.IsNullOrEmpty(data.videoresolution))
                            args += $" -S res:{data.videoresolution}";
                        args += $" -o \"{targetFilename}-%(title)s.%(ext)s\" -- {data.ytid}";
                    }

                    status += $"Exec: {config.exec_YTDL} {args}<br>";

                    var procyt = createProcess(config.exec_YTDL, args);
                    procyt.Start();
                    procyt.WaitForExit();

                    // If audioonly and has tags
                    if (data.audioonly && hasTags)
                    {
                        Process? tagsProcess = null;
                        // Tag the mp3 file if id3v2
                        if (!data.preferfreeformat && hasid3v2)
                        {
                            args = $"-a \"{data.artist}\" -A \"{data.album}\" -t \"{data.title}\" -T \"{data.track}\" \"{targetFilename}{targetExtension}\"";

                            status += $"Exec: {config.exec_ID3} {args}<br>"; 

                            tagsProcess = createProcess(config.exec_ID3, args);
                        }
                        {
                            args = $"-w -t \"TITLE={data.title}\" -t \"ALBUM={data.album}\" -t \"TRACKNUMBER={data.track}\"";
                            foreach (var artist in data.artist.Split(';'))
                            {
                                args += $" -t \"ARTIST={artist}\"";
                            }
                            args += $" \"{targetFilename}{targetExtension}\"";

                            status += $"Exec: {config.exec_vorbiscomment} {args}<br>"; 

                            tagsProcess = createProcess(config.exec_vorbiscomment, args);
                        }
                        tagsProcess.Start();
                        tagsProcess.WaitForExit();
                    }

                    status += "<font color='green'>File Saved!</font>";

                    response.Add("message", status);
                }
                return Ok(response);
            }
            catch(Exception e)
            {
                _logger.LogError(e, e.Message);
                return StatusCode(500, new Dictionary<string, object>() {{"message", e.Message}});
            }
        }

        [HttpGet("libraries")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<Dictionary<string, object>> FinTubeLibraries()
        {
            try
            {
                _logger.LogInformation("FinTubeDLibraries count: {count}", _libraryManager.GetVirtualFolders().Count);

                Dictionary<string, object> response = new Dictionary<string, object>();
                response.Add("data", _libraryManager.GetVirtualFolders().Select(i => i.Locations).ToArray());
                return Ok(response);
            }
            catch(Exception e)
            {
                _logger.LogError(e, e.Message);
                return StatusCode(500, new Dictionary<string, object>() {{"message", e.Message}});
            }
        }

        private static Process createProcess(String exe, String args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = exe, Arguments = args };
            return new Process() { StartInfo = startInfo };
        }
}