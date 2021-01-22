using System;
using System.IO;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace GeoArcSysModdingTool.Utils
{
    public static class Dialogs
    {
        public static string OpenFileDialog(string Title, string Filter)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Title = Title;
            openFileDialog.Filter = Filter;
            if (openFileDialog.ShowDialog() == true)
                return openFileDialog.FileName;
            return null;
        }

        public static string SaveFileDialog(string Title, string Filter, string FileName = "")
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = Title;
            saveFileDialog.Filter = Filter;
            saveFileDialog.FileName = FileName;
            if (saveFileDialog.ShowDialog() == true)
                return saveFileDialog.FileName;
            return null;
        }

        public static string OpenFolderDialog(string Title)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = Title;
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory =
                Path.GetPathRoot(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.System));

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.EnsurePathExists = false;
            dlg.EnsureFileExists = false;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok) return dlg.FileName;

            return null;
        }
    }
}