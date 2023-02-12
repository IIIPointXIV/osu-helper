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
        private static HelperSkin instance;

        public static HelperSkin Instance
        {
            get
            {
                if (instance == null)
                    instance = new HelperSkin();
                return instance;
            }
        }

        private HelperSkin() : base(getPath()) { }

        /// <summary>
        /// Gets the path of the helper skin.
        /// </summary>
        /// <returns>The path of the helper skin.</returns>
        private static string getPath()
        {
            if (OsuHelper.managerFolderName == null)
                throw new ArgumentNullException("managerFolderName is null.");

            return Skin.getPathFromName(OsuHelper.managerFolderName);
        }

        /// <summary>
        /// Deletes all files in the helper skin
        /// </summary>
        public void DeleteSkinElements()
        {
            DirectoryInfo rootFolder = new DirectoryInfo(path);

            foreach (FileInfo file in rootFolder.GetFiles())
            {
                if (OsuHelper.spamLogs)
                    MainForm.DebugLog($"Deleting {file.FullName}", false);
                file.Delete();
            }

            foreach (DirectoryInfo folder in rootFolder.GetDirectories())
            {
                if (OsuHelper.spamLogs)
                    MainForm.DebugLog($"Deleting {folder.FullName} (directory)", false);
                Directory.Delete(folder.FullName, true);
            }
        }
    }
}