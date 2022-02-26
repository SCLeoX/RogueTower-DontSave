using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RogueTower_DontSave
{
    [BepInPlugin("me.tepis.roguetower.dontsave", "Don't Save", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        private void Awake()
        {
            Log = Logger;
            new Harmony("me.tepis.roguetower.dontsave").PatchAll();
            Log.LogInfo("Plugin me.tepis.roguetower.dontsave is loaded!");
        }
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "MainMenu")
            {
                return;
            }
            Font font = GameObject.Find("/Canvas/MainMenu/Play/Text").GetComponent<Text>().font;
            GameObject warningGameObject = new GameObject("Mod_DontSave_Warning", typeof(CanvasRenderer), typeof(Text));
            warningGameObject.transform.SetParent(GameObject.Find("/Canvas").transform, false);
            Text text = warningGameObject.GetComponent<Text>();
            text.font = font;
            text.alignment = TextAnchor.LowerCenter;
            text.color = Color.red;
            text.text = "Warning: Mod Don't Save is enabled! No progress will be saved!";
            text.fontSize = 30;
            text.raycastTarget = false;
            RectTransform transform = warningGameObject.GetComponent<RectTransform>();
            transform.anchoredPosition = new Vector2(0, 60);
            transform.anchorMin = new Vector2(0, 0);
            transform.anchorMax = new Vector2(1, 0);
            Log.LogInfo("Warning inserted.");
        }
        private static Dictionary<string, int> intMemSave = new();
        private static Dictionary<string, float> floatMemSave = new();
        private static Dictionary<string, string> stringMemSave = new();

        [HarmonyPatch(typeof(PlayerPrefs), nameof(PlayerPrefs.TrySetInt))]
        class PlayerPrefs_TrySetInt
        {
            static bool Prefix(ref bool __result, string key, int value)
            {
                Log.LogInfo($"Intercepted writing integer to key={key} with value={value}");
                intMemSave[key] = value;
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerPrefs), nameof(PlayerPrefs.TrySetFloat))]
        class PlayerPrefs_TrySetFloat
        {
            static bool Prefix(ref bool __result, string key, float value)
            {
                Log.LogInfo($"Intercepted writing float to key={key} with value={value}");
                floatMemSave[key] = value;
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerPrefs), nameof(PlayerPrefs.TrySetSetString))]
        class PlayerPrefs_TrySetString
        {
            static bool Prefix(ref bool __result, string key, string value)
            {
                Log.LogInfo($"Intercepted writing string to key={key} with value=\"{value}\"");
                stringMemSave[key] = value;
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerPrefs), nameof(PlayerPrefs.GetInt), typeof(string), typeof(int))]
        class PlayerPrefs_GetInt
        {
            static bool Prefix(ref int __result, string key)
            {
                return !intMemSave.TryGetValue(key, out __result);
            }

            static void Postfix(int __result, string key)
            {
                intMemSave[key] = __result;
            }
        }

        [HarmonyPatch(typeof(PlayerPrefs), nameof(PlayerPrefs.GetFloat), typeof(string), typeof(float))]
        class PlayerPrefs_GetFloat
        {
            static bool Prefix(ref float __result, string key)
            {
                return !floatMemSave.TryGetValue(key, out __result);
            }

            static void Postfix(float __result, string key)
            {
                floatMemSave[key] = __result;
            }
        }

        [HarmonyPatch(typeof(PlayerPrefs), nameof(PlayerPrefs.GetString), typeof(string), typeof(string))]
        class PlayerPrefs_GetString
        {
            static bool Prefix(ref string __result, string key)
            {
                return !stringMemSave.TryGetValue(key, out __result);
            }

            static void Postfix(string __result, string key)
            {
                stringMemSave[key] = __result;
            }
        }
    }
}
