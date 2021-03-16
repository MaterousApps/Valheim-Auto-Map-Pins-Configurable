using System.IO;
using System.Reflection;

namespace Utilities
{
    internal static class ResourceUtils
    {
        public static byte[] GetResource(Assembly asm, string ResourceName)
        {
            Stream manifestResourceStream = asm.GetManifestResourceStream(ResourceName);
            byte[] buffer = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(buffer, 0, (int)manifestResourceStream.Length);
            return buffer;
        }
    }
}