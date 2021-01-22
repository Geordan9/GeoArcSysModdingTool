using System.Linq;

namespace GeoArcSysModdingTool.Utils.Extensions
{
    public static class ArrayExtension
    {
        public static void Populate<T>(this T[] arr, T value)
        {
            for (var i = 0; i < arr.Length; i++) arr[i] = value;
        }

        public static bool ContainsAny<T>(this T[] haystack, params T[] needles)
        {
            foreach (var needle in needles)
                if (haystack.Contains(needle))
                    return true;

            return false;
        }

        public static bool ContainsAny<T>(this T needle, params T[] haystack)
        {
            if (haystack.Contains(needle))
                return true;
            return false;
        }
    }
}