using System;
using System.IO;

namespace osu_helper
{
    /// <summary>
    /// Class used to store the Helper skin information
    /// </summary>
    /// <remarks>
    /// Should only have one object made.
    /// </remarks>
    public sealed class HelperSkin : Skin
    {
        private static HelperSkin? instance;

        public static HelperSkin Instance
        {
            get
            {
                instance ??= new HelperSkin();
                return instance;
            }
        }

        private HelperSkin() : base(GetPath()) { }

        /// <summary>
        /// Gets the path of the helper skin.
        /// </summary>
        /// <returns>The path of the helper skin.</returns>
        private static string GetPath()
        {
            if (OsuHelper.ManagerFolderName == null)
                throw new ArgumentNullException($"{nameof(OsuHelper.ManagerFolderName)} is null.");

            return GetPathFromName(OsuHelper.ManagerFolderName);
        }

        /// <summary>
        /// Deletes all files in the helper skin
        /// </summary>
        public void DeleteSkinElements()
        {
            DirectoryInfo rootFolder = new(Path);

            foreach (FileInfo file in rootFolder.GetFiles())
            {
                if (OsuHelper.SpamLogs)
                    MainForm.DebugLog($"Deleting {file.FullName}", false);
                file.Delete();
            }

            foreach (DirectoryInfo folder in rootFolder.GetDirectories())
            {
                if (OsuHelper.SpamLogs)
                    MainForm.DebugLog($"Deleting {folder.FullName} (directory)", false);
                Directory.Delete(folder.FullName, true);
            }
        }
    }
}