using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using GeoArcSysModdingTool.Properties;

namespace GeoArcSysModdingTool
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly FieldInfo _menuDropAlignmentField;

        public App()
        {
            _menuDropAlignmentField =
                typeof(SystemParameters).GetField("_menuDropAlignment", BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(_menuDropAlignmentField != null);

            EnsureStandardPopupAlignment();
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;

            var settingsFile = "geoarcsysmoddingtool.config";

            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            var path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                       $"{versionInfo.CompanyName}\\{Path.GetFileNameWithoutExtension(versionInfo.FileName)}\\";
            Directory.CreateDirectory(path);
            Settings.Default.SettingsKey = path + settingsFile;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            EnsureStandardPopupAlignment();
        }

        private void EnsureStandardPopupAlignment()
        {
            if (SystemParameters.MenuDropAlignment && _menuDropAlignmentField != null)
                _menuDropAlignmentField.SetValue(null, false);
        }
    }
}