using System.IO;
using System.Reflection;
using UnityEngine;

namespace Utilities
{
    internal static class ResourceUtils
    {
        public static bool ConvertInternalWarningsErrors = false;

        public static byte[] GetResource(Assembly asm, string ResourceName)
        {
            Stream manifestResourceStream = asm.GetManifestResourceStream(ResourceName);
            byte[] buffer = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(buffer, 0, (int)manifestResourceStream.Length);
            return buffer;
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