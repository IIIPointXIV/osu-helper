using System;
using System.IO;
using System.Windows.Forms;

namespace osu_helper
{
    public class OsuHelper
    {
        public static string? OsuFolderPath { get; set; }
        public static string OsuExePath { get { return Path.Combine(OsuFolderPath!, "osu!.exe"); } private set { } }
        public static string OsuSkinsFolderPath { get { return Path.Combine(OsuFolderPath!, "skins"); } private set { } }
        public static string DeletedSkinsFolderPath { get { return Path.Combine(OsuSkinsFolderPath, "Deleted Skins"); } private set { } }
        public static string ManagerFolderName { get; private set; } = "!!!Skin Manager";
        public static bool DebugMode { get; protected set; } = false;
        public static bool SpamLogs { get; protected set; } = false;
        public static MainForm Form { get; private set; } = new MainForm();

        [STAThread]
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (i)
                {
                    case 1:
                        DebugMode = bool.Parse(args[0]);
                        break;
                    case 2:
                        SpamLogs = bool.Parse(args[1]);
                        break;
                }
            }
            //Form.SetupForm(/* (args.Length != 0 ? bool.Parse(args[0]) : false), (args.Length == 2 ? bool.Parse(args[1]) : false) */);
            Application.Run(Form);
        }

        /// <summary>
        /// Asks the user where the osu folder is.
        /// </summary>
        /// <remarks>
        /// Re-prompts if invalid directory is given.
        /// </remarks>
        /// <returns>The string path of where the folder is.</returns>
        public static string? ChangeOsuPath()
        {
            using FolderBrowserDialog directorySelector = new()
            {
                ShowNewFolderButton = false,
                RootFolder = Environment.SpecialFolder.MyComputer,
                Description = "Select an osu! root directory:",
                SelectedPath = OsuFolderPath,
            };

            DialogResult res = directorySelector.ShowDialog();
            if (directorySelector.SelectedPath != null && res == DialogResult.OK)
            {
                if (!File.Exists(Path.Combine(directorySelector.SelectedPath, "osu!.exe")))
                {
                    MessageBox.Show("Not a valid osu! directory!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return ChangeOsuPath();
                }
                return OsuFolderPath = directorySelector.SelectedPath;
                /* osuFolderPathBox.Text = directorySelector.SelectedPath;
                OsuHelper.osuPath = directorySelector.SelectedPath;
                MainForm.DebugLog("osuPath set to: " + OsuHelper.osuPath, false); */
            }
            return null;
        }
    }
}