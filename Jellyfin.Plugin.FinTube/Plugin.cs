using System;
using System.Collections.Generic;
using System.Globalization;
using Jellyfin.Plugin.FinTube.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.FinTube;

/// <summary>
/// The main plugin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;

        ConfigurationChanged += OnConfigurationChanged;
    }

    /// <inheritdoc />
    public override string Name => "FinTube";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("d20aa9ed-eafc-4578-b320-4e3b7093129c");

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }
    public PluginConfiguration PluginConfiguration => Configuration;

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = this.Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace)
            },
            
            new PluginPageInfo
            {
                Name = @"FinTubeDownload",
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Pages.downloadPage.html", GetType().Namespace),
                EnableInMainMenu = true
            }
        };
    }

    private void OnConfigurationChanged(object? sender, BasePluginConfiguration e)
    {
    }
}
