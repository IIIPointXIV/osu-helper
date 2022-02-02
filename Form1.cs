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
        private Button showFilteredSkinsButton;
        private Button deleteSkinSelectorButton;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    // CheckBoxes
        private CheckBox writeCurrSkinBox;
        private CheckBox showSkinNumbersBox;
        private CheckBox showSliderEndsBox;
        private CheckBox disableSkinChangesBox;
        private CheckBox disableCursorTrailBox;
        private CheckBox showComboBurstsBox;
    private ToolTip toolTip;
    private ComboBox skinFolderSelector;
    private Font mainFont;
    private ListBox osuSkinsListBox;
    private List<string> osuSkinsPathList = new List<string>();
    private string osuPath;
    private string mainSkinPath;
    private enum RegValueNames
    {
        skinName,
        disableCursorTrailBox,
        osuPath,
        selectedSkinFolder,
        showSkinNumbersBox,
        skinFolders,
        writeCurrSkinToTXT,
        showSliderEndsBox,
        disableSkinChangesBox,
        showComboBurstsBox,
    };
    public void FormLayout()
    {
        mainFont = new Font("Segoe UI", 12);
        //registry stuff
            if(GetRegValue(RegValueNames.osuPath) == null)
            {
                osuPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "appdata", "Local", "osu!");
                if(!File.Exists(Path.Combine(osuPath, "osu!.exe")))
                {
                    MessageBox.Show("Unable to find valid osu directory. Please select one.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ChangeOsuPathButton_Click(this, new EventArgs());
                }
            }
            else
                osuPath = GetRegValue(RegValueNames.osuPath);

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
            Text = "Random Skin",
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
            Width = 43,
        };
        skinFolderSelector.KeyPress += (sender, ev) =>
        {
            if(ev.KeyChar.Equals((char)13))
            {
                if(!skinFolderSelector.Items.Contains(skinFolderSelector.Text) && skinFolderSelector.Text != "" && skinFolderSelector.Text != null)
                {
                    skinFolderSelector.Items.Add(skinFolderSelector.Text);
                    ChangeRegValue(RegValueNames.skinFolders, (GetRegValue(RegValueNames.skinFolders) == null ? "" : GetRegValue(RegValueNames.skinFolders) + ",") + skinFolderSelector.Text);
                }
                ev.Handled = true;
            }
        };
        if(GetRegValue(RegValueNames.skinFolders) != null && GetRegValue(RegValueNames.skinFolders) != "")
        {
            if(!skinFolderSelector.Items.Contains("All"))
                skinFolderSelector.Items.Add("All");
            
            string[] foldersArr = GetRegValue(RegValueNames.skinFolders).Split(',');

            foreach(string name in foldersArr)
            {
                skinFolderSelector.Items.Add(name);
            }
        }
        else
        {
            skinFolderSelector.Items.Add("All");
            skinFolderSelector.Text = "All";
        }
        if(GetRegValue(RegValueNames.selectedSkinFolder) != null && GetRegValue(RegValueNames.selectedSkinFolder) != "")
        {
            skinFolderSelector.Text = GetRegValue(RegValueNames.selectedSkinFolder);
        }
        else
        {
            skinFolderSelector.Text = "All";
        }
        Controls.Add(skinFolderSelector);

        showFilteredSkinsButton = new System.Windows.Forms.Button()
        {
            Top = 33,
            Font = mainFont,
            //Height = 23,
            Width = 53,
            Left = 46,
            Text = "Show",
        };
        showFilteredSkinsButton.Click += new EventHandler(SearchOsuSkins);
        Controls.Add(showFilteredSkinsButton);

        deleteSkinSelectorButton = new System.Windows.Forms.Button()
        {
            Top = 33,
            Font = mainFont,
            //Height = 23,
            Width = 61,
            Left = 102,
            Text = "Delete",
        };
        deleteSkinSelectorButton.Click += new EventHandler((sender, e) =>
        {
            if(skinFolderSelector.Items.Contains(skinFolderSelector.Text))
                skinFolderSelector.Items.Remove(skinFolderSelector.Text);
            else if(skinFolderSelector.Text == "All")
                DebugLog("You can't delete the \"All\" prefix silly");

            skinFolderSelector.Text = "All";

            GetRegValue(RegValueNames.selectedSkinFolder);
        });
        Controls.Add(deleteSkinSelectorButton);

        writeCurrSkinBox = new CheckBox()
        {
            Height = 25,
            Width = 100,
            Left = 483,
            Top = 3,
            Text = "Write current skin to .txt file",
            TextAlign = ContentAlignment.MiddleLeft,
        };
        if(GetRegValue(RegValueNames.writeCurrSkinToTXT) != null)
            writeCurrSkinBox.Checked = bool.Parse(GetRegValue(RegValueNames.writeCurrSkinToTXT));
        else
            writeCurrSkinBox.Checked = false;

        writeCurrSkinBox.CheckedChanged += new EventHandler(ChangeRegValue_Click);
        Controls.Add(writeCurrSkinBox);

        disableSkinChangesBox = new CheckBox()
        {
            Height = 25,
            Width = 200,
            Left = 585,
            Top = 3,
            Text = "Disable skin changes",
            TextAlign = ContentAlignment.MiddleLeft,
        };
        if(GetRegValue(RegValueNames.disableSkinChangesBox) != null)
            disableSkinChangesBox.Checked = bool.Parse(GetRegValue(RegValueNames.disableSkinChangesBox));
        else
            disableSkinChangesBox.Checked = true;
        
        disableSkinChangesBox.CheckedChanged += new EventHandler(ChangeRegValue_Click);
        Controls.Add(disableSkinChangesBox);

        showComboBurstsBox = new CheckBox()
        {
            Height = 25,
            Width = 297,
            Font = mainFont,
            Left = 503,
            Top = 120,
            Text = "Combo Bursts",
            TextAlign = ContentAlignment.MiddleLeft,
            //ThreeState = true,
        };
        if(GetRegValue(RegValueNames.showComboBurstsBox) != null)
            showComboBurstsBox.Checked = bool.Parse(GetRegValue(RegValueNames.showComboBurstsBox));
        else
            showComboBurstsBox.Checked = true;

        showComboBurstsBox.CheckedChanged += new EventHandler(ChangeRegValue_Click);
        Controls.Add(showComboBurstsBox);

        showSkinNumbersBox = new CheckBox()
        {
            Height = 25,
            Width = 297,
            Font = mainFont,
            Left = 503,
            Top = 60,
            Text = "Hitcircle Numbers",
            TextAlign = ContentAlignment.MiddleLeft,
        };
        showSkinNumbersBox.CheckedChanged += new EventHandler(ChangeRegValue_Click);
        Controls.Add(showSkinNumbersBox);
        if(GetRegValue(RegValueNames.showSkinNumbersBox) != null)
            showSkinNumbersBox.Checked = bool.Parse(GetRegValue(RegValueNames.showSkinNumbersBox));
        else
            showSkinNumbersBox.Checked = true;

        disableCursorTrailBox = new CheckBox()
        {
            Height = 25,
            Width = 297,
            Font = mainFont,
            Left = 503,
            Top = 100,
            Text = "Cursor Trail",
            TextAlign = ContentAlignment.MiddleLeft,
        };
        disableCursorTrailBox.CheckedChanged += new EventHandler(ChangeRegValue_Click);
        Controls.Add(disableCursorTrailBox);
        if(GetRegValue(RegValueNames.disableCursorTrailBox) != null)
            disableCursorTrailBox.Checked = bool.Parse(GetRegValue(RegValueNames.disableCursorTrailBox));
        else
            disableCursorTrailBox.Checked = true;
        
        showSliderEndsBox = new CheckBox()
        {
            Height = 25,
            Width = 297,
            Font = mainFont,
            Left = 503,
            Top = 80,
            Text = "Slider Ends",
            TextAlign = ContentAlignment.MiddleLeft,
        };
        showSliderEndsBox.CheckedChanged += new EventHandler(ChangeRegValue_Click);
        Controls.Add(showSliderEndsBox);
        if(GetRegValue(RegValueNames.showSliderEndsBox) != null)
            showSliderEndsBox.Checked = bool.Parse(GetRegValue(RegValueNames.showSliderEndsBox));
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
        toolTip.SetToolTip(disableSkinChangesBox, "Disables all changes to skin files and only copies them over");
        toolTip.SetToolTip(disableCursorTrailBox, "Checked means no cursor trail is shown.\nWill not add trail to skin that does not have it.");
        toolTip.SetToolTip(showFilteredSkinsButton, "Shows only the skins with the selected prefix");
        toolTip.SetToolTip(deleteSkinSelectorButton, "Deletes skin prefix from list");
        toolTip.SetToolTip(showComboBurstsBox, "If checked, combo bursts will be shown if the skin has them");

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
                if(skinFolderSelector.Text != "All")
                {
                    if(skinName.IndexOf(skinFolderSelector.Text) == 0)
                    {
                        osuSkinsPathList.Add(skin.FullName);
                        osuSkinsListBox.Items.Add(skinName);
                    }
                }
                else
                {
                    osuSkinsPathList.Add(skin.FullName);
                    osuSkinsListBox.Items.Add(skinName);
                }
            }
        }
        osuSkinsListBox.EndUpdate();
        if(!Controls.Contains(osuSkinsListBox))
            Controls.Add(osuSkinsListBox);
        
        osuSkinsListBox.ClearSelected();
        if(GetRegValue(RegValueNames.skinName) != null && osuSkinsListBox.Items.IndexOf(GetRegValue(RegValueNames.skinName)) != -1)
            osuSkinsListBox.SetSelected(osuSkinsListBox.Items.IndexOf(GetRegValue(RegValueNames.skinName)), true);

        if(sender == showFilteredSkinsButton)
        {
            if(!skinFolderSelector.Items.Contains(skinFolderSelector.Text))
                skinFolderSelector.Items.Add(skinFolderSelector.Text);

            ChangeRegValue_Click(sender, e);
        }

        if(!osuSkinsListBox.Items.Contains(skinFolderSelector.Text))
            ChangeRegValue(RegValueNames.skinName, "");
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
        ChangeRegValue(RegValueNames.skinName, osuSkinsListBox.SelectedItem.ToString());

        if(writeCurrSkinBox.Checked)
            UpdateSkinTextFile(skinPath.Replace(Path.Combine(osuPath, "skins") + "\\", ""));

        DirectoryInfo di = new DirectoryInfo(skinPath);
        
        foreach(FileInfo file in di.GetFiles())
        {
            file.CopyTo(mainSkinPath + "\\" + file.Name, true);
        }
        RecursiveSkinFolderMove(skinPath, "");

        if(!disableSkinChangesBox.Checked)
        {
            ShowHideHitCircleNumbers(showSkinNumbersBox.Checked);
            ShowHideSliderEnds(showSliderEndsBox.Checked);
            DisableCursorTrail(disableCursorTrailBox.Checked);
            ShowHideCombobursts(showComboBurstsBox.Checked);
        }
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
    private void ShowHideCombobursts(bool show)
    {
        Bitmap img = new Bitmap(1,1);
        List<string> fileNames = new List<string>()
        {
            "comboburst",
            "comboburst@2x",
            "comboburst-fruits",
            "comboburst-fruits@2x",
            "comboburst-mania",
            "comboburst-mania@2x",
        };

        foreach(string name in fileNames)
        {
            if(!File.Exists(Path.Combine(mainSkinPath, name + ".png")))
            {
                img.Save(Path.Combine(mainSkinPath, name + ".png"));
            }
            else if(!show)
            {
                img.Save(Path.Combine(mainSkinPath, name + ".png"));
            }
        }

        DirectoryInfo di = new DirectoryInfo(mainSkinPath);
        foreach(FileInfo file in di.GetFiles())
        {
            foreach(string name in fileNames)
            {
                if(file.Name.Contains(name))
                    img.Save(Path.Combine(mainSkinPath, file.Name));
            }
        }
        img.Dispose();
    }
    private void DisableCursorTrail(bool show)
    {
        if(GetCurrentSkinPath() == null)
            return;
        List<string> names = new List<string>()
        {
            "cursortrail@2x.png",
            "cursortrail.png",
        };
        if(show)
        {
            foreach(string name in names)
                if(File.Exists(Path.Combine(GetCurrentSkinPath(), name)))
                    File.Copy(Path.Combine(GetCurrentSkinPath(), name), Path.Combine(mainSkinPath, name), true);
        }
        else
        {
            Bitmap empty = new Bitmap(1, 1);
            foreach(string name in names)
            {
                if(File.Exists(Path.Combine(mainSkinPath, name)))
                    File.Delete(Path.Combine(mainSkinPath, name));
                empty.Save(Path.Combine(mainSkinPath, name));
            }
            empty.Dispose();
        }
    }
    
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
                using(Image image = (File.Exists(Path.Combine(GetCurrentSkinPath(), fileName)) ? Image.FromFile(Path.Combine(GetCurrentSkinPath(), fileName)) : null))
                {
                    if(File.Exists(Path.Combine(GetCurrentSkinPath(), fileName)) && image.Size.Height > 100)
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
                        sliderImage.Dispose(); 
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
            EditSkinIni("HitCirclePrefix:", "HitCirclePrefix: 727", "[Fonts]");
    }

    private void EditSkinIni(string searchFor, string replaceWith, string fallBackAdd)
    {
        string skinINIPath = Path.Combine(mainSkinPath, "skin.ini");
        File.Copy(skinINIPath, skinINIPath.Replace("skin.ini", "skin.ini.temp"));
        StreamReader reader = new StreamReader(skinINIPath.Replace("skin.ini", "skin.ini.temp"));
        StreamWriter writer = new StreamWriter(skinINIPath);
        string currLine;
        bool lineFound = false;
        while((currLine = reader.ReadLine()) != null)
        {
            if(currLine.Contains(searchFor))
            {
                writer.WriteLine(replaceWith);
                lineFound = true;
                continue;
            }
            writer.WriteLine(currLine);
        }

        currLine = null;
        reader.Close();
        reader.Dispose();
        writer.Close();
        writer.Dispose();

        if(!lineFound)
        {
            StreamWriter writerNew = new StreamWriter(skinINIPath);
            StreamReader readerNew = new StreamReader(skinINIPath.Replace("skin.ini", "skin.ini.temp"));

            while((currLine = readerNew.ReadLine()) != null)
            {   
                if(currLine.Contains(fallBackAdd))
                {
                    
                    writerNew.WriteLine(currLine);
                    writerNew.WriteLine(replaceWith);
                    continue;
                }
                writerNew.WriteLine(currLine);
            }
            writerNew.Dispose();
            readerNew.Dispose();
        }
        
        File.Delete(skinINIPath.Replace("skin.ini", "skin.ini.temp"));
    }

//Edit Reg Stuff
    private void ChangeRegValue_Click(object sender, EventArgs e)
    {
        RegValueNames valName;
        string val = "";
        if(sender == writeCurrSkinBox)
        {
            valName = RegValueNames.writeCurrSkinToTXT;
            val = writeCurrSkinBox.Checked.ToString();
        }
        else if(sender == changeOsuPathButton || sender == osuPathBox)
        {
            valName = RegValueNames.osuPath;
            val = osuPath;
        }
        else if(sender == showSkinNumbersBox)
        {
            valName = RegValueNames.showSkinNumbersBox;
            val = showSkinNumbersBox.Checked.ToString();
            ShowHideHitCircleNumbers(showSkinNumbersBox.Checked);
        }
        else if(sender == showSliderEndsBox)
        {
            valName = RegValueNames.showSliderEndsBox;
            val = showSliderEndsBox.Checked.ToString();
            ShowHideSliderEnds(showSliderEndsBox.Checked);
        }
        else if(sender == disableSkinChangesBox)
        {
            if(!disableSkinChangesBox.Checked)
            {
                ShowHideHitCircleNumbers(showSkinNumbersBox.Checked);
                ShowHideSliderEnds(showSliderEndsBox.Checked);
            }
            valName = RegValueNames.disableSkinChangesBox;
            val = disableSkinChangesBox.Checked.ToString();
        }
        else if(sender == disableCursorTrailBox)
        {
            valName = RegValueNames.disableCursorTrailBox;
            val = disableCursorTrailBox.Checked.ToString();
            DisableCursorTrail(disableCursorTrailBox.Checked);
        }
        else if(sender == showFilteredSkinsButton)
        {
            valName = RegValueNames.selectedSkinFolder;
            val = skinFolderSelector.Text;
        }
        else if(sender == showComboBurstsBox)
        {
            valName = RegValueNames.showComboBurstsBox;
            val = showComboBurstsBox.Checked.ToString();
            ShowHideCombobursts(showComboBurstsBox.Checked);
        }
        else
        {
            DebugLog("Error with ChangeRegValue_Click");
            return;
        }

        ChangeRegValue(valName, val);
    }

    private void ChangeRegValue(RegValueNames valueName, string value)
    {
        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", valueName.ToString(), value);
    }
    
    private string GetRegValue(RegValueNames valName)
    {
        try
        {
            return Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", valName.ToString(), null).ToString();
        }
        catch
        {
            return null;
        }
    }

//MISC
    private string GetCurrentSkinPath()
    {
        if(osuSkinsListBox.SelectedItems.Count == 1)
            return osuSkinsPathList[osuSkinsListBox.SelectedIndex];
        else if(GetRegValue(RegValueNames.skinName) == null || osuSkinsListBox.SelectedItems.Count > 1 || osuSkinsListBox.SelectedItems.Count == 1)
            DebugLog("Multiple/no skins selected. Unable to get skin path.");
        else
            return Path.Combine(osuPath, "skins", GetRegValue(RegValueNames.skinName));

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

    private void DebugLog(bool log )
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