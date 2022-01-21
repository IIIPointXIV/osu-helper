using System.Security.Cryptography;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Runtime.ConstrainedExecution;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
public class Form1 : Form
{
    private TextBox osuPathBox;
    //buttons
        private Button changeOsuPathButton;
        private Button searchOsuSkinsButton;
        private Button deleteSkinButton;
        private Button useSkinButton;
        private Button randomSkinButton;
        private Button openSkinFolderButton;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private CheckBox writeCurrSkinBox;
    private Font mainFont;
    private ListBox osuSkinsListBox;
    private List<string> osuSkinsPathList = new List<string>();
    private string osuPath;
    private string mainSkinPath;
    public void FormLayout()
    {
        mainFont = new Font("Segoe UI", 12);
        //registry stuff
            if(Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", "osuPath", null) == null)
                osuPath = Environment.GetEnvironmentVariable("USERPROFILE") + "\\appdata\\Local\\osu!";
            else
                osuPath = Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", "osuPath", null).ToString();

            mainSkinPath = osuPath + "\\skins\\!!!osu!helper Skin";

        //general form stuff
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Icon = new Icon(".\\Images\\o_h_kDs_icon.ico");
            this.Name = "osu!helper";
            this.Text = "osu!helper";
            this.Size = new Size(800, 800);

        osuPathBox = new System.Windows.Forms.TextBox()
        {
            Text = osuPath,
            Left = 87,
            Width = 300,
            Font = mainFont,
        };
        Controls.Add(osuPathBox);

        changeOsuPathButton = new System.Windows.Forms.Button()
        {
            Left = 0,
            Top = 3,
            Width = 84,
            Font = mainFont,
            Text = "osu! Path",
        };
        changeOsuPathButton.Click += new EventHandler(changeOsuPathButton_Click);
        Controls.Add(changeOsuPathButton);

        searchOsuSkinsButton = new System.Windows.Forms.Button()
        {
            Left = 390,
            Top = 3,
            Width = 90,
            Font = mainFont,
            Text = "Find Skins"
        };
        searchOsuSkinsButton.Click += new EventHandler(SearchOsuSkins);
        Controls.Add(searchOsuSkinsButton);

        osuSkinsListBox = new ListBox()
        {
            Size = new Size(500, 700),
            Top = 30,
            Font = mainFont,
            SelectionMode = SelectionMode.MultiExtended,
        };

        deleteSkinButton = new System.Windows.Forms.Button()
        {
            Left = 3,
            Top = 737,
            Width = 96,
            Font = mainFont,
            Text = "Delete Skin",
        };
        deleteSkinButton.Click += new EventHandler(DeleteSelectedSkin);

        openSkinFolderButton = new System.Windows.Forms.Button()
        {
            Left = 298,
            Top = 737,
            Width = 140,
            Font = mainFont,
            Text = "Open Skin Folder",
        };
        openSkinFolderButton.Click += new EventHandler(OpenSkinFolder);

        writeCurrSkinBox = new CheckBox()
        {
            Height = 25,
            Width = 100,
            Left = 483,
            Top = 3,
            Text = "Write current skin to .txt file",
            TextAlign = ContentAlignment.MiddleLeft,
        };
        if(Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", "writeCurrSkinToTXT", null) != null)
            writeCurrSkinBox.Checked = bool.Parse(Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", "writeCurrSkinToTXT", null).ToString());
        else
            writeCurrSkinBox.Checked = false;

        writeCurrSkinBox.CheckedChanged += new EventHandler(WriteCurrSkinBoxChanged);
        Controls.Add(writeCurrSkinBox);

        useSkinButton = new System.Windows.Forms.Button()
        {
            Left = 102,
            Top = 737,
            Width = 80,
            Font = mainFont,
            Text = "Use Skin"
        };
        useSkinButton.Click += new EventHandler(ChangeToSelectedSkin);

        randomSkinButton = new System.Windows.Forms.Button()
        {
            Left = 185,
            Top = 737,
            Width = 110,
            Font = mainFont,
            Text = "Random Skin"
        };
        randomSkinButton.Click += new EventHandler(RandomSkin_Click);

        //fileDialog stuff
        openFileDialog1 = new System.Windows.Forms.OpenFileDialog()
        {
            InitialDirectory =  osuPath,
            Filter = "Directory|*.directory",
            FileName =  "",
            Title = "Select osu! directory"
        };

        DirectoryInfo osuPathDI = new DirectoryInfo(osuPath + "\\skins");
        if(osuPathDI.Exists)
        {
            try
            {
                osuPathDI.CreateSubdirectory("!!!osu!helper Skin");
                osuPathDI.CreateSubdirectory("Deleted Skins");
            }
            catch
            {
                DebugLog("Error Creating Sub-Directories");
            }
        }
    }
    
    private void changeOsuPathButton_Click(object sender, EventArgs e)
    {
        FolderBrowserDialog directorySelector = new FolderBrowserDialog();
            directorySelector.ShowNewFolderButton = false;
            directorySelector.RootFolder = Environment.SpecialFolder.MyComputer;
            directorySelector.Description = "Select an osu! root directory:";
            directorySelector.SelectedPath = osuPath;

        DialogResult givenPath = directorySelector.ShowDialog();
        if (givenPath == DialogResult.OK)
        {
            //check if osu!.exe is present
            if (!File.Exists(directorySelector.SelectedPath + "\\osu!.exe"))
            {
                MessageBox.Show("Not a valid osu! directory!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                changeOsuPathButton_Click(sender, e);
                directorySelector.Dispose();
                return;
            }
            SetRegOsuPath(directorySelector.SelectedPath);
            directorySelector.Dispose();
        }
        //DialogResult result = openFileDialog1.ShowDialog();
    }
    
    private void SetRegOsuPath(string path)
    {
        osuPathBox.Text = path;
        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", "osuPath", path);
    }

    private void SearchOsuSkins(object sender, EventArgs e)
    {
        osuSkinsListBox.BeginUpdate();
        osuSkinsListBox.Items.Clear();
        osuSkinsPathList.Clear();
        DirectoryInfo di = new DirectoryInfo(osuPath + "\\skins");
        var osuSkins = di.GetDirectories();
        foreach(DirectoryInfo skin in osuSkins)
        {
            string skinName = skin.FullName.Replace(osuPath + "\\skins\\", "");

            if(skinName != "!!!osu!helper Skin")
            {
                osuSkinsPathList.Add(skin.FullName);
                osuSkinsListBox.Items.Add(skinName);
                //DebugLog(skinName);
            }
        }
        osuSkinsListBox.EndUpdate();
        Controls.Add(osuSkinsListBox);
        Controls.Add(deleteSkinButton);
        Controls.Add(useSkinButton);
        Controls.Add(randomSkinButton);
        Controls.Add(openSkinFolderButton);
    }

    private void OpenSkinFolder(object sender, EventArgs e)
    {
        DebugLog(Path.Combine(osuPath, osuSkinsListBox.SelectedItem.ToString()));
        Process.Start("explorer.exe", Path.Combine(osuPath, "skins", osuSkinsListBox.SelectedItem.ToString()));
    }

    private void DeleteSelectedSkin(object sender, EventArgs e)
    {
        try
        {
            string skinPath = osuSkinsPathList[osuSkinsListBox.SelectedIndex];
            Directory.Move(skinPath, osuPath + "\\skins\\Deleted Skins\\" + osuSkinsListBox.SelectedItem);
        }
        catch
        {
            DebugLog("Find skins first. Error occurred when trying to delete skin");
        }
        osuSkinsPathList.RemoveAt(osuSkinsListBox.SelectedIndex);
        osuSkinsListBox.Items.RemoveAt(osuSkinsListBox.SelectedIndex);
    }

    private void ChangeToSelectedSkin(object sender, EventArgs e)
    {
        if(osuSkinsListBox.SelectedItem == null)
        {
            DebugLog("Please select skin before trying to change to it.");
            return;
        }

        DeleteSkinElementsInMainSkin();
        string skinPath;
        if(osuSkinsListBox.SelectedItems.Count == 1)
        {
            skinPath = osuSkinsPathList[osuSkinsListBox.SelectedIndex];
        }
        else
        {
            Random random = new Random();
            var randomSkinIndex = osuSkinsListBox.Items.IndexOf(osuSkinsListBox.SelectedItems[random.Next(0, osuSkinsListBox.SelectedItems.Count)]);
            skinPath = osuSkinsPathList[randomSkinIndex];

            osuSkinsListBox.ClearSelected();
            osuSkinsListBox.SetSelected(randomSkinIndex, true);
        }
        UpdateSkinTextFile(skinPath.Replace(Path.Combine(osuPath, "skins") + "\\", ""));

        DirectoryInfo di = new DirectoryInfo(skinPath);
        
        foreach(FileInfo file in di.GetFiles())
        {
            file.CopyTo(mainSkinPath + "\\" + file.Name, true);
        }
        RecursiveSkinFolderMove(skinPath, "");
    }

    private void RecursiveSkinFolderMove(string skinPath, string prevFolder)
    {
        DirectoryInfo rootFolder = new DirectoryInfo(skinPath + prevFolder);

        foreach(DirectoryInfo folder in rootFolder.GetDirectories())
        {
            Directory.CreateDirectory(mainSkinPath + prevFolder +"\\" + folder.Name);
            DirectoryInfo subFolder = new DirectoryInfo(skinPath + prevFolder + "\\" + folder.Name);

            if(subFolder.GetDirectories().Length != 0)
            {
                RecursiveSkinFolderMove(skinPath, prevFolder + "\\" + folder.Name);
            }

            foreach(FileInfo file in subFolder.GetFiles())
            {
                file.CopyTo(mainSkinPath + prevFolder + "\\" + folder.Name + "\\" + file.Name, true);
            }  
        }
    }

    private void UpdateSkinTextFile(string skinName)
    {
        File.WriteAllText(Path.Combine(osuPath, "skins", "currentSkin.txt"), skinName);
    }

    private void RandomSkin_Click(object sender, EventArgs e)
    {
        var randomNumber = new Random();
        osuSkinsListBox.SetSelected(randomNumber.Next(0, osuSkinsListBox.Items.Count), true);
        ChangeToSelectedSkin(sender, e);
    }

    private void DeleteSkinElementsInMainSkin()
    {
        DirectoryInfo rootFolder = new DirectoryInfo(mainSkinPath);
        
        foreach(FileInfo file in rootFolder.GetFiles())
        {
            try
            {
                file.Delete();
            }
            catch{}
        }
        foreach(DirectoryInfo folder in rootFolder.GetDirectories())
        {
            Directory.Delete(folder.FullName, true);
        }
    }

    private void WriteCurrSkinBoxChanged(object sender, EventArgs e)
    {
        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", "writeCurrSkinToTXT", writeCurrSkinBox.Checked);
    }
    
    private void DebugLog(string log)
    {
        Console.WriteLine(log);
    }
}