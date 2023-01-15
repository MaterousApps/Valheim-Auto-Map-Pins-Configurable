using AMP_Configurable;
using AMP_Configurable.PinConfig;
using BepInEx;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Utilities
{
    internal static class ResourceUtils
    {
        public static bool ConvertInternalWarningsErrors = false;

        public static byte[] GetResource(string spriteName)
        {
            if (spriteName == "") return null;
            if (Mod.loggingEnabled.Value) Mod.Log.LogInfo($"Attempting to load sprite {spriteName}");

            string spritePath = GetAssetPath(Path.Combine("pin-icons", spriteName));
            if (!File.Exists(spritePath))
            {
                if (Mod.loggingEnabled.Value) Mod.Log.LogInfo($"{spriteName} not found in pin-icons folder, looking in assembly path");
                spritePath = GetAssetPath(Path.Combine(spriteName));
            }
            if (File.Exists(spritePath))
            {
                return File.ReadAllBytes(spritePath);
            }

            Mod.Log.LogWarning($"[AMP] Could not find pin icon asset ({spritePath}), attempting to load generic circle icon");
            spritePath = GetAssetPath(Path.Combine("pin-icons", "mapicon_pin_iron.png"));
            if (!File.Exists(spritePath))
            {
                spritePath = GetAssetPath(Path.Combine("mapicon_pin_iron.png"));
            }
            if (File.Exists(spritePath))
            {
                byte[] fileData = File.ReadAllBytes(spritePath);
                return fileData;
            }

            Mod.Log.LogError($"[AMP] Could not find pin icon assets. AMP Enhanced is likely installed incorrectly.");
            return null;
        }

        public static PinConfig LoadPinConfig(string filename)
        {
            var pinTypesJson = LoadJsonText(filename);
            if (pinTypesJson == null)
            {
                Mod.Log.LogError($"[AMP] Could not find pin types config json. AMP Enhanced is likely installed incorrectly.");
                return null;
            }
            return JsonConvert.DeserializeObject<PinConfig>(pinTypesJson);
        }

        public static string LoadJsonText(string filename)
        {
            var jsonFilePath = GetAssetPath(filename);
            if (string.IsNullOrEmpty(jsonFilePath))
                return null;

            return File.ReadAllText(jsonFilePath);
        }

        public static string GetAssetPath(string assetName)
        {
            var assetFileName = Path.Combine(Paths.PluginPath, "AMP_Enhanced", assetName);
            if (!File.Exists(assetFileName))
            {
                assetFileName = GenerateAssetPathAtAssembly(assetName);
                if (!File.Exists(assetFileName))
                {
                    // Check if it's in the main folder because r2modmanager doesn't keep folder structures
                    assetFileName = Path.Combine(Paths.PluginPath, "raziell74-AMPED_Auto_Map_Pins_Enhanced", assetName);
                    if (!File.Exists(assetFileName))
                    {
                        assetFileName = GenerateAssetPathAtAssembly(assetName);
                        if (!File.Exists(assetFileName))
                        {
                            Mod.Log.LogInfo($"[AMP] Could not find asset ({assetName})");
                            return null;
                        }
                    }
                }
            }

            return assetFileName;
        }

        public static string GenerateAssetPathAtAssembly(string assetName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return Path.Combine(Path.GetDirectoryName(assembly.Location) ?? string.Empty, assetName);
        }

        public static BindingFlags BindFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static void SetPrivateField(this object obj, string fieldName, object value) => obj.GetType().GetField(fieldName, BindFlags).SetValue(obj, value);

        public static T GetPrivateField<T>(this object obj, string fieldName) => (T)obj.GetType().GetField(fieldName, BindFlags).GetValue(obj);

        public static void SetPrivateProperty(this object obj, string propertyName, object value) => obj.GetType().GetProperty(propertyName, BindFlags).SetValue(obj, value, (object[])null);

        public static void InvokePrivateMethod(
          this object obj,
          string methodName,
          object[] methodParams)
        {
            obj.GetType().GetMethod(methodName, BindFlags).Invoke(obj, methodParams);
        }
        public enum Status
        {
            Initialized,
            Loading,
            Ready,
            Error,
            Unload,
        }

        public static void Logz(string[] categories, string[] messages, LogType logType = LogType.Log)
        {
            string str = string.Empty;
            if (categories != null)
            {
                foreach (string category in categories)
                    str = str + " (" + category + ") -> ";
            }
            if (messages != null)
            {
                foreach (string message in messages)
                    str = message == null ? str + "NULL | " : str + message + " | ";
                str = str.Remove(str.Length - 2, 1);
            }
            if (!ConvertInternalWarningsErrors)
            {
                if (logType != LogType.Error)
                {
                    if (logType == LogType.Warning)
                        Debug.LogWarning(("[AMP Commands]" + str));
                    else
                        Debug.Log(("[AMP Commands]" + str));
                }
                else
                    Debug.LogError(("[AMP Commands]" + str));
            }
            else
                Debug.Log(("[AMP Commands]" + str));
        }

        public static string Logr(string[] categories, string[] messages)
        {
            string str = string.Empty;
            if (categories != null)
            {
                foreach (string category in categories)
                    str = str + " (" + category + ")";
            }
            if (messages != null)
            {
                foreach (string message in messages)
                    str = message == null ? str + "NULL | " : str + message + " | ";
                str = str.Remove(str.Length - 2, 1);
            }
            return "[AMP Commands]" + str;
        }

    }
}