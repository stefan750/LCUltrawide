using BepInEx;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LCUltrawide
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static ConfigEntry<int> configResW;
        private static ConfigEntry<int> configResH;

        private static ConfigEntry<float> configUIScale;
        private static ConfigEntry<float> configUIAspect;

        // Aspect Ratio
        public static float fDefaultAspect = 860 / (float)520;
        public static float fNewAspect;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            configResW = Config.Bind("Resolution", "Width", 860, "Horizontal rendering resolution");
            configResH = Config.Bind("Resolution", "Height", 520, "Vertical rendering resolution");
            
            configUIScale = Config.Bind("UI", "Scale", 3.2f, "Changes the size of UI elements on the screen");
            configUIAspect = Config.Bind("UI", "AspectRatio", 1.77f, "Changes the aspect ratio of the ingame HUD, a higher number makes the HUD wider (4:3 = 1.33, 16:9 = 1.77, 21:9 = 2.33, 32:9 = 3.55)");

            Harmony.CreateAndPatchAll( typeof( Plugin ) );
        }

        [HarmonyPatch(typeof(PlayerControllerB), "Start")]
        [HarmonyPrefix]
        static void PlayerControllerStart(PlayerControllerB __instance)
        {
            //Change camera render texture resolution
            RenderTexture screenTex = __instance.gameplayCamera.targetTexture;

            screenTex.width = configResW.Value;
            screenTex.height = configResH.Value;

            //Correct aspect ratio for camera view
            GameObject panelObject = GameObject.Find("Systems/UI/Canvas/Panel");
            if(panelObject != null)
            {
                if(panelObject.TryGetComponent<AspectRatioFitter>(out AspectRatioFitter arf))
                {
                    arf.aspectRatio = configResW.Value / (float)configResH.Value;
                }
            }

            //Change UI scale
            GameObject canvasObject = GameObject.Find("Systems/UI/Canvas");
            if(canvasObject.TryGetComponent<Canvas>(out Canvas canvas))
            {
                canvas.scaleFactor = configUIScale.Value;
            }

            //Change HUD aspect ratio
            GameObject hudObject = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD");
            if (hudObject != null)
            {
                if (hudObject.TryGetComponent<AspectRatioFitter>(out AspectRatioFitter arf))
                {
                    arf.aspectRatio = configUIAspect.Value;
                }
            }

            //Fix Inventory position
            GameObject inventoryObject = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/Inventory");
            if (inventoryObject != null)
            {
                if (inventoryObject.TryGetComponent<RectTransform>(out RectTransform rectTransform))
                {
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.anchorMax = new Vector2(0.5f, 0f);
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0f);
                }
            }

            //Scale up width of helmet model
            GameObject helmetModel = GameObject.Find("ScavengerHelmet");
            if (helmetModel != null)
            {
                if (helmetModel.TryGetComponent<Transform>(out Transform transform))
                {
                    float fDefaultHelmetWidth = 0.5136268f;
                    Vector3 helmetScale = transform.localScale;
                    // Helmet width is good up until an aspect ratio of 1.925~
                    helmetScale.x = fDefaultHelmetWidth * (fNewAspect / 1.925f);
                    transform.localScale = helmetScale;
                }
            }
        }

        [HarmonyPatch(typeof(HUDManager), "UpdateScanNodes")]
        [HarmonyPostfix]
        static void HUDManagerUpdateScanNodes(PlayerControllerB playerScript, HUDManager __instance)
        {
            //Correct UI marker positions for scanned objects
            RectTransform[] scanElements = __instance.scanElements;
            Dictionary<RectTransform, ScanNodeProperties> scanNodes = Traverse.Create(__instance).Field("scanNodes").GetValue() as Dictionary<RectTransform, ScanNodeProperties>;

            GameObject playerScreen = GameObject.Find("Systems/UI/Canvas/Panel/GameObject/PlayerScreen");
            if(!playerScreen.TryGetComponent<RectTransform>(out RectTransform screenTransform))
            {
                return;
            }
            Rect rect = screenTransform.rect;

            for (int i = 0; i < scanElements.Length; i++)
            {
                if(scanNodes.TryGetValue(scanElements[i], out ScanNodeProperties scanNode))
                {
                    Vector3 viewportPos = playerScript.gameplayCamera.WorldToViewportPoint(scanNode.transform.position);
                    scanElements[i].anchoredPosition = new Vector2(rect.xMin + rect.width * viewportPos.x, rect.yMin + rect.height * viewportPos.y);
                }
            }
        }

    }
}
