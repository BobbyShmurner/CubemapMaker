using System;
using System.IO;
using System.Collections.Generic;

using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;

using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace CubemapMaker
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Harmony harmony;

        internal static ConfigFile config;
        internal static ConfigEntry<string> configCaptureKeybind;
        internal static ConfigEntry<string> configTransCaptureKeybind;
        internal static ConfigEntry<string> configOutputPath;
        internal static ConfigEntry<string> configCaptureOrientation;
        internal static ConfigEntry<bool> configCaptureUI;
        internal static ConfigEntry<int> configOutputWidth;

        internal static Camera renderCam = null;
        
        private void Awake()
        {
            Logger.LogInfo($"Loading Plugin {PluginInfo.PLUGIN_GUID}...");

            Log = Logger;
            Log.LogInfo("Created Global Logger");

            harmony = new Harmony("CubemapMaker");
            harmony.PatchAll();
            Log.LogInfo("Applied All Patches");

            SetupConfig();
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private void OnDestroy() =>
            harmony?.UnpatchSelf(); 

        private void SetupConfig() {
            string configFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\CubemapMaker";
            string outputFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Cubemaps";

            Directory.CreateDirectory(configFolder);

            if (!File.Exists(Path.Combine(configFolder, "config.cfg")) && Directory.Exists(Path.Combine(Application.dataPath, "..\\..\\ULTRAKILL"))) {
                // Checks if ULTRAKILL is installed in the same place as the first game run with this mod
                // If that's the case, set the default location to the Cybergrind folder

                outputFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "..\\..\\ULTRAKILL\\Cybergrind\\Textures\\Skyboxes\\Captured"));
                Log.LogInfo("Found ULTRAKILL Install! Set the capture output to the Cybergrind directory");
            }

            config = new ConfigFile(Path.Combine(configFolder, "config.cfg"), true);

            configOutputPath = config.Bind("General", "OutputPath", outputFolder, "The path that cubemaps will be outputed to");
            configOutputWidth = config.Bind("General", "OutputWidth", 4096, "The width of the outputed cubemap");
            configCaptureUI = config.Bind("General", "CaptureUI", true, "Whether to capture UI in a cubemap or not");
            configCaptureOrientation = config.Bind("General", "CaptureOrientation", "Yaw", new ConfigDescription("The orientation used to capture the cubemap in\nYaw: This will capture the cubemap in the direction your facing but will keep the world upright\nAccurate: This will accuratly capture rotation in the cubemap, so whereever you are looking will be the forward position\nNone: This won't take the camera's orientation into account at all", new AcceptableValueList<string>("Yaw", "Accurate", "None")));

            configCaptureKeybind = config.Bind("General.Keybinds", "CaptureButton", "f10", "The key used to capture a cubemap");
            configTransCaptureKeybind = config.Bind("General.Keybinds", "TransparentCaptureButton", "f11", "The key used to capture a cubemap with a transparent skybox");

            outputFolder = configOutputPath.Value;

            Directory.CreateDirectory(outputFolder);

            Log.LogInfo("Loaded Config");
        }

        private void CaptureCubemap(bool transparent, bool cgSecondPlayer = false) {
            renderCam ??= gameObject.AddComponent<Camera>();
            renderCam.CopyFrom(Camera.main);

            renderCam.transform.rotation = Camera.main.transform.rotation;
            renderCam.transform.position = Camera.main.transform.position;

            if (transparent) Log.LogInfo("Capturing Transparent Cubemap");
            else Log.LogInfo("Capturing Cubemap");

            if (cgSecondPlayer) {
                if (SceneHelper.CurrentScene != "Endless") {
                    cgSecondPlayer = false;
                    Log.LogWarning("Tried capturing a CG Second Player cubemap outside of the cyber grind. Capturing normal cubemap instead");
                } else if (Application.productName != "ULTRAKILL") {
                    cgSecondPlayer = false;
                    Log.LogWarning("Tried capturing a CG Second Player cubemap in a game that isn't ULTRAKILL. Capturing normal cubemap instead");
                } else {
                    Log.LogInfo("Capturing CG Second Player Cubemap");
                }
            }

            string fileName = $"{Application.productName} {DateTime.Now:yyyy-MM-dd HH-mm-ss}.png";
            string filePath = Path.GetFullPath(Path.Combine(configOutputPath.Value, fileName));

            // General Capture Vars
            RenderTexture cubeTex = new RenderTexture(configOutputWidth.Value, configOutputWidth.Value, 32);
            RenderTexture equirectTex = new RenderTexture(cubeTex.width, (int)(cubeTex.width * 0.5f), 32);
            Texture2D capturedTex = new Texture2D(equirectTex.width, equirectTex.height);
        

            // Setup the correct capture orientation
            if (!cgSecondPlayer) {
                switch (configCaptureOrientation.Value.ToLower()) {
                    case "none":
                        renderCam.transform.eulerAngles = Vector3.zero;
                        break;
                    case "accurate":
                        break;
                    default:
                        renderCam.transform.eulerAngles = new Vector3(0, renderCam.transform.eulerAngles.y, 0);
                        break;
                }
            } else {
                renderCam.transform.eulerAngles = Vector3.zero; // Set orientation to none for cg second player
                renderCam.transform.position = new Vector3(500, 30, 50); // TODO: Let the player set how far away the camera is
            }

            if (transparent) {
                renderCam.clearFlags = CameraClearFlags.Color;
                renderCam.backgroundColor = Color.clear;
            }

            // Disable UI
            Dictionary<Canvas, bool> originalCanvasActives = new Dictionary<Canvas, bool>();

            if (!configCaptureUI.Value) {
                foreach (Canvas canvas in FindObjectsOfType<Canvas>()) {
                    originalCanvasActives.Add(canvas, canvas.enabled);
                    canvas.enabled = false;
                }
            }

            // Capture the cubemap
            cubeTex.dimension = TextureDimension.Cube;
            renderCam.transform.Rotate(new Vector3(0, 90, 0));
            renderCam.RenderToCubemap(cubeTex, 63);
            cubeTex.ConvertToEquirect(equirectTex, Camera.MonoOrStereoscopicEye.Mono);

            // Enable UI
            if (!configCaptureUI.Value) {
                foreach (var canvas in originalCanvasActives) {
                    canvas.Key.enabled = canvas.Value;
                }
            }

            // Render the cubemap to a Texture2D
            RenderTexture.active = equirectTex;
            capturedTex.ReadPixels(new Rect(0, 0, equirectTex.width, equirectTex.height), 0, 0);
            RenderTexture.active = null;

            // Save the image to disk
            Directory.CreateDirectory(configOutputPath.Value);
            File.WriteAllBytes(filePath, capturedTex.EncodeToPNG());

            Log.LogInfo($"Captured cubemap to \"{filePath}\"");
        }

        private void Update() {
            if (Input.GetKeyDown(configCaptureKeybind.Value.ToLower())) CaptureCubemap(false, Input.GetKey(KeyCode.LeftShift));
            if (Input.GetKeyDown(configTransCaptureKeybind.Value.ToLower())) CaptureCubemap(true, Input.GetKey(KeyCode.LeftShift));
        }
    }
}
