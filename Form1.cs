//using System.Reflection.Emit;
//using System.Security.Cryptography;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Win32;
//using System.Runtime.ConstrainedExecution;
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
    // CheckBoxes
        private CheckBox writeCurrSkinBox;
        private CheckBox showSkinNumbersBox;
        private CheckBox showSliderEndsBox;
    private ToolTip toolTip;
    private ComboBox skinFolderSelector;
    private Font mainFont;
    private ListBox osuSkinsListBox;
    private List<string> osuSkinsPathList = new List<string>();
    private string osuPath;
    private string mainSkinPath;
    public void FormLayout()
    {
        mainFont = new Font("Segoe UI", 12);
        //registry stuff
            if(GetRegValue("osuPath") == null)
            {
                osuPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "appdata", "Local", "osu!");
                if(!File.Exists(Path.Combine(osuPath, "osu!.exe")))
                {
                    MessageBox.Show("Unable to find valid osu directory. Please select one.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ChangeOsuPathButton_Click(this, new EventArgs());
                }
            }
            else
                osuPath = GetRegValue("osuPath");

            mainSkinPath = Path.Combine(osuPath, "skins", "!!!osu!helper Skin");

        //general form stuff
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            //this.Icon = new Icon(".\\Images\\o_h_kDs_icon.ico");
            this.Name = "osu!helper";
            this.Text = "osu!helper";
            this.Size = new Size(800, 800);

        osuSkinsListBox = new ListBox()
        {
            Size = new Size(500, 676),
            Top = 60,
            Font = mainFont,
            SelectionMode = SelectionMode.MultiExtended,
        };

        osuPathBox = new System.Windows.Forms.TextBox()
        {
            Text = osuPath,
            Left = 87,
            Width = 300,
            Font = mainFont,
        };
        Controls.Add(osuPathBox);
        osuPathBox.KeyPress += (sender, ev) => 
        {
            if (ev.KeyChar.Equals((char)13))
            {
                if (!File.Exists(osuPathBox.Text + "\\osu!.exe"))
                {
                    MessageBox.Show("Not a valid osu! directory!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                osuPath = osuPathBox.Text;

                ChangeRegValue_Click(sender, ev);
                ev.Handled = true;
            }
        };

        changeOsuPathButton = new System.Windows.Forms.Button()
        {
            Left = 0,
            Top = 3,
            Width = 84,
            Font = mainFont,
            Text = "osu! Path",
        };
        changeOsuPathButton.Click += new EventHandler(ChangeOsuPathButton_Click);
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

        useSkinButton = new System.Windows.Forms.Button()
        {
            Left = 102,
            Top = 737,
            Width = 80,
            Font = mainFont,
            Text = "Use Skin"
        };
        useSkinButton.Click += new EventHandler(ChangeToSelectedSkin);
        Controls.Add(useSkinButton);

        randomSkinButton = new System.Windows.Forms.Button()
        {
            Left = 185,
            Top = 737,
            Width = 110,
            Font = mainFont,
            Text = "Random Skin"
        };
        randomSkinButton.Click += new EventHandler(RandomSkin_Click);
        Controls.Add(randomSkinButton);

        deleteSkinButton = new System.Windows.Forms.Button()
        {
            Left = 3,
            Top = 737,
            Width = 96,
            Font = mainFont,
            Text = "Delete Skin",
        };
        deleteSkinButton.Click += new EventHandler(DeleteSelectedSkin);
        Controls.Add(deleteSkinButton);

        openSkinFolderButton = new System.Windows.Forms.Button()
        {
            Left = 298,
            Top = 737,
            Width = 140,
            Font = mainFont,
            Text = "Open Skin Folder",
        };
        openSkinFolderButton.Click += new EventHandler(OpenSkinFolder);
        Controls.Add(openSkinFolderButton);

        skinFolderSelector = new ComboBox()
        {
            Top = 30,
            Font = mainFont,
            Height = 23,
            Width = 50,
        };
        skinFolderSelector.TextChanged += new EventHandler(SkinFolderSelectorChanged);

        if(GetRegValue("skinFolders") != null)
        {
            if(!skinFolderSelector.Items.Contains("All"))
                skinFolderSelector.Items.Add("All");
            
            string[] foldersArr = GetRegValue("skinFolders").Split(',');

            foreach(string name in foldersArr)
            {
                skinFolderSelector.Items.Add(name);
            }
            skinFolderSelector.Text = GetRegValue("selectedSkinFolder");
        }
        else
        {
            skinFolderSelector.Items.Add("All");
            skinFolderSelector.Text = "All";
        }
        Controls.Add(skinFolderSelector);

        writeCurrSkinBox = new CheckBox()
        {
            Height = 25,
            Width = 100,
            Left = 483,
            Top = 3,
            Text = "Write current skin to .txt file",
            TextAlign = ContentAlignment.MiddleLeft,
        };
        if(GetRegValue("writeCurrSkinToTXT") != null)
            writeCurrSkinBox.Checked = bool.Parse(GetRegValue("writeCurrSkinToTXT"));
        else
            writeCurrSkinBox.Checked = false;

        writeCurrSkinBox.CheckedChanged += new EventHandler(ChangeRegValue_Click);
        Controls.Add(writeCurrSkinBox);

        showSkinNumbersBox = new CheckBox()
        {
            Height = 25,
            Width = 297,
            Font = mainFont,
            Left = 503,
            Top = 30,
            Text = "Hitcircle Numbers",
            TextAlign = ContentAlignment.MiddleLeft,
        };
        showSkinNumbersBox.CheckedChanged += new EventHandler(ChangeRegValue_Click);
        Controls.Add(showSkinNumbersBox);
        
        if(GetRegValue("showSkinNumbersBox") != null)
            showSkinNumbersBox.Checked = bool.Parse(GetRegValue("showSkinNumbersBox"));
        else
            showSkinNumbersBox.Checked = true;

        showSliderEndsBox = new CheckBox()
        {
            Height = 25,
            Width = 297,
            Font = mainFont,
            Left = 503,
            Top = 50,
            Text = "Slider Ends",
            TextAlign = ContentAlignment.MiddleLeft,
        };
        showSliderEndsBox.CheckedChanged += new EventHandler(ChangeRegValue_Click);
        Controls.Add(showSliderEndsBox);

        if(GetRegValue("showSliderEndsBox") != null)
            showSliderEndsBox.Checked = bool.Parse(GetRegValue("showSliderEndsBox"));
        else
            showSliderEndsBox.Checked = false;
        
        toolTip = new ToolTip();
        toolTip.SetToolTip(searchOsuSkinsButton, "Searches osu! folder for skins");
        toolTip.SetToolTip(changeOsuPathButton, "Set the path that houses the osu!.exe");
        toolTip.SetToolTip(writeCurrSkinBox, "Writes the name of the current skin to a text\nfile in the \"Skins\" folder. Helpful for streamers.");
        toolTip.SetToolTip(randomSkinButton, "Selects random skin from visible skins");
        toolTip.SetToolTip(openSkinFolderButton, "Opens current skin folder");
        toolTip.SetToolTip(useSkinButton, "Changes to selected skin. If multiple \nare selected, it chooses a random one.");
        toolTip.SetToolTip(deleteSkinButton, "Moves selected skin to \"Deleted Skins\" folder");
        toolTip.SetToolTip(showSkinNumbersBox, "Controls if numbers are shown on hitcircles");
        toolTip.SetToolTip(showSliderEndsBox, "Controls if slider ends are visible.\nChecked means that they are shown.");
        toolTip.SetToolTip(skinFolderSelector, "Allows you to designate a prefix on the skin folders to categorize the skins");
        
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
            try
            {
                SearchOsuSkins(new Object(), new EventArgs());

            }
            catch
            {
                DebugLog("Error searching skins");
            }
        }

    }

//MISC Skin handling
    private void ChangeOsuPathButton_Click(object sender, EventArgs e)
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
                MessageBox.Show("Not a valid osu! directory!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ChangeOsuPathButton_Click(sender, e);
                directorySelector.Dispose();
                return;
            }
            osuPathBox.Text = directorySelector.SelectedPath;
            osuPathBox.Text = directorySelector.SelectedPath;
            osuPath = directorySelector.SelectedPath;

            ChangeRegValue_Click(sender, e);
            directorySelector.Dispose();
        }
        //DialogResult result = openFileDialog1.ShowDialog();
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
        if(!Controls.Contains(osuSkinsListBox))
            Controls.Add(osuSkinsListBox);
        
        osuSkinsListBox.ClearSelected();
        if(GetRegValue("skinName") != null)
            osuSkinsListBox.SetSelected(osuSkinsListBox.Items.IndexOf(GetRegValue("skinName")), true);
    }

    private void OpenSkinFolder(object sender, EventArgs e)
    {
        //DebugLog(Path.Combine(osuPath, osuSkinsListBox.SelectedItem.ToString()));
        if(osuSkinsListBox.SelectedItem == null)
        {
            DebugLog("Select skin before trying to open its folder");
            return;
        }
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

    private void SkinFolderSelectorChanged(object sender, EventArgs e)
    {

    }

//Skin Switching
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
            skinPath = osuSkinsPathList[osuSkinsListBox.SelectedIndex];
        else
        {
            Random random = new Random();
            var randomSkinIndex = osuSkinsListBox.Items.IndexOf(osuSkinsListBox.SelectedItems[random.Next(0, osuSkinsListBox.SelectedItems.Count)]);
            skinPath = osuSkinsPathList[randomSkinIndex];

            osuSkinsListBox.ClearSelected();
            osuSkinsListBox.SetSelected(randomSkinIndex, true);
        }
        ChangeRegValue("skinName", osuSkinsListBox.SelectedItem.ToString());

        if(writeCurrSkinBox.Checked)
            UpdateSkinTextFile(skinPath.Replace(Path.Combine(osuPath, "skins") + "\\", ""));

        DirectoryInfo di = new DirectoryInfo(skinPath);
        
        foreach(FileInfo file in di.GetFiles())
        {
            file.CopyTo(mainSkinPath + "\\" + file.Name, true);
        }
        RecursiveSkinFolderMove(skinPath, "");
        ShowHideHitCircleNumbers(showSkinNumbersBox.Checked);
        ShowHideSliderEnds(showSliderEndsBox.Checked);
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

    private void RandomSkin_Click(object sender, EventArgs e)
    {
        var randomNumber = new Random();
        osuSkinsListBox.ClearSelected();
        osuSkinsListBox.SetSelected(randomNumber.Next(0, osuSkinsListBox.Items.Count), true);
        ChangeToSelectedSkin(sender, e);
    }

//Skin editing
    private void ShowHideSliderEnds(bool show)
    {
        string[] sliderEnds =
        {
            "sliderendcircle.png",
            "sliderendcircle@2x.png",
            "sliderendcircleoverlay.png",
            "sliderendcircleoverlay@2x.png"
        };
        Bitmap emptyImage = new Bitmap(1, 1);
        Image sliderImage = new Bitmap(1, 1);

        if(show)//Showing ends
        {
            foreach(string fileName in sliderEnds)
            {
                if(File.Exists(Path.Combine(GetCurrentSkinPath(), fileName)) && Image.FromFile(Path.Combine(GetCurrentSkinPath(), fileName)).Size.Height > 100)
                {
                    File.Copy(Path.Combine(GetCurrentSkinPath(), fileName), Path.Combine(mainSkinPath, fileName), true);
                }
                else if(File.Exists(Path.Combine(mainSkinPath, fileName)))
                {
                    sliderImage = Image.FromFile(Path.Combine(mainSkinPath, fileName));
                    if(sliderImage.Size.Height < 100)
                    {
                        sliderImage.Dispose();
                        File.Delete(Path.Combine(mainSkinPath, fileName));
                    }
                }
            }
        }
        else//hiding ends
        {
            foreach(string fileName in sliderEnds)
                emptyImage.Save(Path.Combine(mainSkinPath, fileName));
        }
        emptyImage.Dispose();
        sliderImage.Dispose();
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

    private void ShowHideHitCircleNumbers(bool show)
    {
        if(osuSkinsPathList.Count == 0 || osuSkinsListBox.SelectedItems.Count == 0)
            return;
        
        if(show) //show skin numbers 
            File.Copy(Path.Combine(osuSkinsPathList[osuSkinsListBox.SelectedIndex], "skin.ini"), Path.Combine(mainSkinPath, "skin.ini"), true);
        else //hide skin numbers
            EditSkinIni("HitCirclePrefix:", "HitCirclePrefix: 727");
    }

    private void EditSkinIni(string searchFor, string replaceWith)
    {
        string skinINIPath = Path.Combine(mainSkinPath, "skin.ini");
        File.Copy(skinINIPath, skinINIPath.Replace("skin.ini", "skin.ini.temp"));
        StreamReader reader = new StreamReader(skinINIPath.Replace("skin.ini", "skin.ini.temp"));
        StreamWriter writer = new StreamWriter(skinINIPath);
        string currLine;
        int count = 0;
        while((currLine = reader.ReadLine()) != null)
        {
            if(currLine.Contains(searchFor))
            {
                writer.WriteLine(replaceWith);
                count++;
                continue;
            }
            writer.WriteLine(currLine);
            count++;
        }
        reader.Close();
        reader.Dispose();
        writer.Close();
        writer.Dispose();
        File.Delete(skinINIPath.Replace("skin.ini", "skin.ini.temp"));
    }

//Edit Reg Stuff
    private void ChangeRegValue_Click(object sender, EventArgs e)
    {
        string valName = "";
        string val = "";
        if(sender == writeCurrSkinBox)
        {
            valName = "writeCurrSkinToTXT";
            val = writeCurrSkinBox.Checked.ToString();
        }
        else if(sender == changeOsuPathButton || sender == osuPathBox)
        {
            valName = "osuPath";
            val = osuPath;
        }
        else if(sender == showSkinNumbersBox)
        {
            valName = "showSkinNumbersBox";
            val = showSkinNumbersBox.Checked.ToString();
            ShowHideHitCircleNumbers(showSkinNumbersBox.Checked);
        }
        else if(sender == showSliderEndsBox)
        {
            valName = "showSliderEndsBox";
            val = showSliderEndsBox.Checked.ToString();
            ShowHideSliderEnds(showSliderEndsBox.Checked);
        }

        ChangeRegValue(valName, val);
    }

    private void ChangeRegValue(string valueName, string value)
    {
        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", valueName, value);
    }
    
    private string GetRegValue(string valName)
    {
        try
        {
            return Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", valName, null).ToString();
        }catch
        {
            return null;
        }
    }

//MISC
    private string GetCurrentSkinPath()
    {
        if(osuSkinsListBox.SelectedItems.Count == 1)
            return osuSkinsPathList[osuSkinsListBox.SelectedIndex];
        else if(GetRegValue("skinName") == null || osuSkinsListBox.SelectedItems.Count > 1 || osuSkinsListBox.SelectedItems.Count == 1)
            DebugLog("Multiple\\no skins selected. Unable to get skin path.");
        else
            return Path.Combine(osuPath, "skins", GetRegValue("skinName"));

        return null;
    }
    
    private void DebugLog(string log)
    {
        Console.WriteLine(log);
    }

    private void DebugLog(int log)
    {
        Console.WriteLine(log);
    }

    private void DebugLog()
    {
        Console.WriteLine("Test debug");
    }

    private void UpdateSkinTextFile(string skinName)
    {
        File.WriteAllText(Path.Combine(osuPath, "skins", "currentSkin.txt"), skinName);
    }
}