using System;
using System.IO;

using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;

using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace CubemapMaker
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Harmony harmony;

        internal static ConfigFile config;
        internal static ConfigEntry<string> configCaptureKeybind;
        internal static ConfigEntry<string> configOutputPath;
        internal static ConfigEntry<int> configOutputWidth;
        
        private void Awake()
        {
            Logger.LogInfo($"Loading Plugin {PluginInfo.PLUGIN_GUID}...");

            Plugin.Log = base.Logger;
            Plugin.Log.LogInfo("Created Global Logger");

            harmony = new Harmony("CubemapMaker");

            Plugin.Log.LogInfo("Applied All Patches");

            SetupConfig();

            Plugin.Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
        }

        private void SetupConfig() {
            string configFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\CubemapMaker";
            string outputFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\CubemapMaker";

            Directory.CreateDirectory(configFolder);

            if (!File.Exists(Path.Combine(configFolder, "config.cfg")) && Directory.Exists(Path.Combine(Application.dataPath, "..\\..\\ULTRAKILL"))) {
                // Checks if ULTRAKILL is installed in the same place as the first game run with this mod
                // If that's the case, set the default location to the Cybergrind folder

                outputFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "..\\..\\ULTRAKILL\\Cybergrind\\Textures\\Skyboxes\\Captured"));
                Plugin.Log.LogInfo("Found ULTRAKILL Install! Set the capture output to the Cybergrind directory");
            }

            config = new ConfigFile(Path.Combine(configFolder, "config.cfg"), true);

            configOutputPath = config.Bind("General", "OutputPath", outputFolder, "The path that cubemaps will be outputed to");
            configOutputWidth = config.Bind("General", "OutputWidth", 4096, "The width of the outputed cubemap");
            configCaptureKeybind = config.Bind("General.Keybinds", "CaptureButton", "f10", "The key used to capture a cubemap");

            outputFolder = configOutputPath.Value;

            Directory.CreateDirectory(outputFolder);

            Plugin.Log.LogInfo("Loaded Config");
        }

        private void Update() {
            if (!Input.GetKeyDown(configCaptureKeybind.Value.ToLower())) return;
            string fileName = $"{Application.productName} {DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}.png";

            string filePath = Path.GetFullPath(Path.Combine(configOutputPath.Value, fileName));

            RenderTexture cubeTex = new RenderTexture(configOutputWidth.Value, configOutputWidth.Value, 32);
            RenderTexture equirectTex = new RenderTexture(cubeTex.width, (int)(cubeTex.width * 0.5f), 32);
            Texture2D tex = new Texture2D(cubeTex.width, cubeTex.height);

            cubeTex.dimension = TextureDimension.Cube;

            Camera.main.RenderToCubemap(cubeTex);
            cubeTex.ConvertToEquirect(equirectTex);

            RenderTexture.active = equirectTex;
            tex.ReadPixels(new Rect(0, 0, equirectTex.width, equirectTex.height), 0, 0);
            RenderTexture.active = null;

            Directory.CreateDirectory(configOutputPath.Value);
            File.WriteAllBytes(filePath, tex.EncodeToPNG());

            Plugin.Log.LogInfo($"Captured cubemap to \"{filePath}\"");
        }
    }
}
