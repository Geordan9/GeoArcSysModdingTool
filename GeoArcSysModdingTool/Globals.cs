using System.IO;
using System.Reflection;

namespace GeoArcSysModdingTool
{
    public static class Globals
    {
        public static string CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
}