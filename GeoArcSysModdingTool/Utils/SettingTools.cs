using GeoArcSysModdingTool.Properties;

namespace GeoArcSysModdingTool.Utils
{
    public static class SettingTools
    {
        public static void UpdateSettings()
        {
            var settings = Settings.Default;
            settings.Save();
            Mediator.NotifyColleagues("UpdateSettings", new object[]
            {
                settings.ReverseScanEnabled,
                settings.ValueUpdateInterval,
                settings.ProcessUpdateInterval,
                settings.PreemptFileAnalysis,
                settings.OpenInApp
            });
        }
    }
}