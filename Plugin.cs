using BepInEx;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace LCUltrawide
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static ConfigEntry<int> configResW;
        private static ConfigEntry<int> configResH;

        private static ConfigEntry<float> configUIScale;
        private static ConfigEntry<float> configUIAspect;

        //How often the screen size will be checked in seconds
        private static float aspectUpdateTime = 1.0f;

        private static bool aspectAutoDetect = false;

        //Previous aspect ratio update
        private static float prevAspect = 0f;
        private static float prevTime = 0f;

        //Default Helmet width
        private static float fDefaultHelmetWidth = 0.3628f;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            configResW = Config.Bind("Resolution Override", "Width", 0, "Horizontal rendering resolution override.\nIf set to 0, the resolution will be automatically adjusted to fit your monitors aspect ratio.\nGame default value: 860");
            configResH = Config.Bind("Resolution Override", "Height", 0, "Vertical rendering resolution override.\nIf set to 0, the original resolution will be used.\nGame default value: 520");

            configUIScale = Config.Bind("UI", "Scale", 1f, "Changes the size of UI elements on the screen.");
            configUIAspect = Config.Bind("UI", "AspectRatio", 0f, "Changes the aspect ratio of the ingame HUD, a higher number makes the HUD wider.\n(0 = auto, 1.33 = 4:3, 1.77 = 16:9, 2.33 = 21:9, 3.55 = 32:9)");

            aspectAutoDetect = configResW.Value <= 0;

            Harmony.CreateAndPatchAll( typeof( Plugin ) );
        }

        public static void ChangeAspectRatio(float newAspect)
        {
            HUDManager hudManager = HUDManager.Instance;

            //Change camera render texture resolution
            RenderTexture screenTex = hudManager.playerScreenTexture.texture as RenderTexture;
            screenTex.Release();
            screenTex.height = configResH.Value > 0 ? configResH.Value : screenTex.height;
            screenTex.width = configResW.Value > 0 ? configResW.Value : Convert.ToInt32(screenTex.height * newAspect);

            //Change terminal camera render texture resolution
            GameObject terminalObject = GameObject.Find("TerminalScript");
            if (terminalObject != null)
            {
                if (terminalObject.TryGetComponent(out Terminal terminal))
                {
                    RenderTexture terminalTexHighRes = terminal.playerScreenTexHighRes;
                    terminalTexHighRes.Release();
                    terminalTexHighRes.height = configResH.Value > 0 ? configResH.Value : terminalTexHighRes.height;
                    terminalTexHighRes.width = configResW.Value > 0 ? configResW.Value : Convert.ToInt32(terminalTexHighRes.height * newAspect);
                }
            }

            if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null)
            {
                Camera camera = GameNetworkManager.Instance.localPlayerController.gameplayCamera;
                camera.ResetAspect();
            }

            //Correct aspect ratio for camera view
            GameObject panelObject = GameObject.Find("Systems/UI/Canvas/Panel");
            if (panelObject != null)
            {
                if (panelObject.TryGetComponent(out AspectRatioFitter arf))
                {
                    arf.aspectRatio = newAspect;
                }
            }

            //Change UI scale
            GameObject canvasObject = GameObject.Find("Systems/UI/Canvas");
            if (canvasObject != null)
            {
                if (canvasObject.TryGetComponent(out CanvasScaler canvasScaler))
                {
                    float refHeight = 500 / configUIScale.Value;
                    float refWidth = refHeight * newAspect;
                    canvasScaler.referenceResolution = new Vector2(refWidth, refHeight);
                }
            }

            //Change HUD aspect ratio
            GameObject hudObject = hudManager.HUDContainer;
            if (hudObject != null)
            {
                if (hudObject.TryGetComponent(out AspectRatioFitter arf))
                {
                    arf.aspectRatio = configUIAspect.Value > 0 ? configUIAspect.Value : newAspect;
                }
            }

            //Fix stretched HUD elements
            GameObject uiCameraObject = GameObject.Find("Systems/UI/UICamera");
            if (uiCameraObject != null)
            {
                if (uiCameraObject.TryGetComponent(out Camera uiCamera))
                {
                    uiCamera.fieldOfView = Math.Min(106 / (configUIAspect.Value > 0 ? configUIAspect.Value : newAspect), 60);
                }
            }

            //Fix Inventory position
            GameObject inventoryObject = hudManager.Inventory.canvasGroup.gameObject;
            if (inventoryObject != null)
            {
                if (inventoryObject.TryGetComponent(out RectTransform rectTransform))
                {
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.anchorMax = new Vector2(0.5f, 0f);
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0f);
                }
            }

            //Scale up width of helmet model
            GameObject helmetModel = GameObject.Find("PlayerHUDHelmetModel");
            if (helmetModel != null)
            {
                if (helmetModel.TryGetComponent<Transform>(out Transform transform))
                {
                    Vector3 helmetScale = transform.localScale;
                    // Helmet width is good up until an aspect ratio of 2.3~
                    helmetScale.x = fDefaultHelmetWidth * Math.Max(newAspect / 2.3f, 1);
                    transform.localScale = helmetScale;
                }
            }
        }

        [HarmonyPatch(typeof(HUDManager), "Start")]
        [HarmonyPostfix]
        static void HUDManagerStart(HUDManager __instance)
        {
            if (!aspectAutoDetect)
            {
                ChangeAspectRatio(1.77f);
            }
        }

        [HarmonyPatch(typeof(HUDManager), "Update")]
        [HarmonyPostfix]
        static void HUDManagerUpdate(HUDManager __instance)
        {
            //Check screen aspect ratio and update resolution and UI if it changed
            if(aspectAutoDetect && Time.time > (prevTime + aspectUpdateTime))
            {
                Vector2 canvasSize = __instance.playerScreenTexture.canvas.renderingDisplaySize;
                float currentAspect = canvasSize.x / canvasSize.y;

                if (currentAspect != prevAspect)
                {
                    ChangeAspectRatio(currentAspect);
                    prevAspect = currentAspect;

                    Debug.Log("New Aspect Ratio: " + currentAspect);
                }

                prevTime = Time.time;
            }
        }

        [HarmonyPatch(typeof(HUDManager), "UpdateScanNodes")]
        [HarmonyPostfix]
        static void HUDManagerUpdateScanNodes(PlayerControllerB playerScript, HUDManager __instance, Dictionary<RectTransform, ScanNodeProperties> ___scanNodes)
        {
            //Correct UI marker positions for scanned objects
            RectTransform[] scanElements = __instance.scanElements;
            
            GameObject playerScreen = __instance.playerScreenTexture.gameObject;
            if(!playerScreen.TryGetComponent(out RectTransform screenTransform))
            {
                return;
            }
            Rect rect = screenTransform.rect;

            for (int i = 0; i < scanElements.Length; i++)
            {
                if(___scanNodes.TryGetValue(scanElements[i], out ScanNodeProperties scanNode))
                {
                    Vector3 viewportPos = playerScript.gameplayCamera.WorldToViewportPoint(scanNode.transform.position);
                    scanElements[i].anchoredPosition = new Vector2(rect.xMin + rect.width * viewportPos.x, rect.yMin + rect.height * viewportPos.y);
                }
            }
        }

    }
}
