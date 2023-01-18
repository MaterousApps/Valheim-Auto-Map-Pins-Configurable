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
      string[] spriteSearch = Directory.GetFiles(Paths.PluginPath, spriteName, SearchOption.AllDirectories);
      if (spriteSearch.Length == 0)
      {
        Mod.Log.LogWarning($"[AMP] Could not find pin icon asset ({spriteName}), using generic circle icon");
        spriteSearch = Directory.GetFiles(Paths.PluginPath, "mapicon_pin_iron.png", SearchOption.AllDirectories);

        if (spriteSearch.Length == 0)
        {
          Mod.Log.LogError("[AMP] Could not find pin icon sprite. AMP Enhanced is likely installed incorrectly.");
          return null;
        }
      }

      // If there are multiple of the same name, AMPED will use the first asset it finds in the plugins folder
      string spritePath = spriteSearch[0];

      if (Mod.loggingEnabled.Value) Mod.Log.LogInfo($"Successfully loaded sprite: {spriteName}");
      return File.ReadAllBytes(spritePath);
    }

    public static string GetDefaultPinConfig()
    {
      string[] defaultConfigs = null;
      defaultConfigs = Directory.GetFiles(Paths.PluginPath, "amp_pin_types.json", SearchOption.AllDirectories);

      if (defaultConfigs.Length == 0)
      {
        Mod.Log.LogWarning("Could not find the default AMP config file.");
        return "";
      }

      return defaultConfigs[0];
    }

    public static string[] GetPinConfigFiles()
    {
      string[] configs = null;

      if (Mod.loggingEnabled.Value) Mod.Log.LogInfo("Looking for pin configuration json files...");
      configs = Directory.GetFiles(Paths.PluginPath, "amp_*.json", SearchOption.AllDirectories);

      if (configs.Length == 0) Mod.Log.LogWarning("Could not find any AMP config files. No automatic pins will be added...");

      return configs;
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

    public static string LoadJsonText(string jsonFilePath)
    {
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