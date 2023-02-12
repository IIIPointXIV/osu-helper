using System;
using System.IO;
using System.Windows.Forms;

namespace osu_helper
{
    public class OsuHelper
    {
        public static string osuPath { get; set; }
        public static string managerFolderName { get; private set; } = "!!!Skin Manager";
        public static bool debugMode { get; protected set; } = false;
        public static bool spamLogs { get; protected set; } = false;
        public static MainForm form { get; private set; } = new MainForm();

        [STAThread]
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (i)
                {
                    case 1:
                        debugMode = bool.Parse(args[0]);
                        break;
                    case 2:
                        spamLogs = bool.Parse(args[1]);
                        break;
                }
            }
            form.SetupForm(/* (args.Length != 0 ? bool.Parse(args[0]) : false), (args.Length == 2 ? bool.Parse(args[1]) : false) */);
            Application.Run(form);
        }

        /// <summary>
        /// Asks the user where the osu folder is.
        /// </summary>
        /// <remarks>
        /// Re-prompts if invalid directory is given.
        /// </remarks>
        /// <returns>The string path of where the folder is.</returns>
        public static string ChangeOsuPath()
        {
            FolderBrowserDialog directorySelector = new FolderBrowserDialog()
            {
                ShowNewFolderButton = false,
                RootFolder = Environment.SpecialFolder.MyComputer,
                Description = "Select an osu! root directory:",
                SelectedPath = OsuHelper.osuPath,
            };

            DialogResult givenPath = directorySelector.ShowDialog();
            if (givenPath == DialogResult.OK)
            {
                if (!File.Exists(directorySelector.SelectedPath + Path.DirectorySeparatorChar + "osu!.exe"))
                {
                    MessageBox.Show("Not a valid osu! directory!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    directorySelector.Dispose();
                    return ChangeOsuPath();
                }
                /* osuFolderPathBox.Text = directorySelector.SelectedPath;
                OsuHelper.osuPath = directorySelector.SelectedPath;
                MainForm.DebugLog("osuPath set to: " + OsuHelper.osuPath, false); */
            }
            directorySelector.Dispose();
            osuPath = directorySelector.SelectedPath;
            return osuPath;
        }
    }
}