﻿using System.Reflection;
using TextureMod;

#region Assembly attributes
/*
 * These attributes define various metainformation of the generated DLL.
 * In general, you don't need to touch these. Instead, edit the values in PluginInfos.
 */
[assembly: AssemblyVersion(PluginInfos.PLUGIN_VERSION)]
[assembly: AssemblyTitle(PluginInfos.PLUGIN_NAME + " (" + PluginInfos.PLUGIN_ID + ")")]
[assembly: AssemblyProduct(PluginInfos.PLUGIN_NAME)]
#endregion

namespace TextureMod
{
    /// <summary>
    /// The main metadata of the plugin.
    /// This information is used for BepInEx plugin metadata.
    /// </summary>
    /// <remarks>
    /// See also description of BepInEx metadata:
    /// https://bepinex.github.io/bepinex_docs/master/articles/dev_guide/plugin_tutorial/2_plugin_start.html#basic-information-about-the-plug-in
    /// </remarks>
    internal static class PluginInfos
    {
        /// <summary>
        /// Human-readable name of the plugin. In general, it should be short and concise.
        /// This is the name that is shown to the users who run BepInEx and to modders that inspect BepInEx logs.
        /// </summary>
        public const string PLUGIN_NAME = "TextureMod";

        /// <summary>
        /// Unique ID of the plugin.
        /// This must be a unique string that contains only characters a-z, 0-9 underscores (_) and dots (.)
        /// Prefer using the reverse domain name notation: https://eqdn.tech/reverse-domain-notation/
        ///
        /// When creating Harmony patches, prefer using this ID for Harmony instances as well.
        /// </summary>
        public const string PLUGIN_ID = "no.mrgentle.plugins.llb.texturemod";

        /// <summary>
        /// Version of the plugin. Must be in form <major>.<minor>.<build>.<revision>.
        /// Major and minor versions are mandatory, but build and revision can be left unspecified.
        /// </summary>
        public const string PLUGIN_VERSION = "2.4.0";
    }
}
