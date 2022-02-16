using System.Reflection;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
public class Form1 : Form
{
    private TextBox osuPathBox;
    // Buttons
        private Button changeOsuPathButton;
        private Button searchOsuSkinsButton;
        private Button deleteSkinButton;
        private Button useSkinButton;
        private Button randomSkinButton;
        private Button openSkinFolderButton;
        private Button showFilteredSkinsButton;
        private Button hideSelectedSkinFolderButton;
        private Button deleteSkinSelectorButton;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    // CheckBoxes
        private CheckBox writeCurrSkinBox;
        private CheckBox showSkinNumbersBox;
        private CheckBox showSliderEndsBox;
        private CheckBox disableSkinChangesBox;
        private CheckBox disableCursorTrailBox;
        private CheckBox showComboBurstsBox;
        private CheckBox showHitlightingBox;
        private CheckBox hiddenFoldersText;
        private CheckBox showHitCircles;
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
        showHitlightingBox,
        hiddenSkinFolders,
        showHitCircles,
    };
    //bool startUpTime = true;
    public void FormLayout()
    {
        mainFont = new Font("Segoe UI", 12);
        if(GetRegValue(RegValueNames.osuPath) == null) //If the path is not set in the reg, try to get default directory. If it is not there through an error
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
            Width = 500,
            Height = 676,
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
        osuPathBox.KeyPress += (sender, thisEvent) => 
        {
            if (thisEvent.KeyChar.Equals((char)13)) //(char)13 = enter
            {
                if (!File.Exists(Path.Combine(osuPathBox.Text, "osu!.exe")))
                {
                    MessageBox.Show("Not a valid osu! directory!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                osuPath = osuPathBox.Text;
                ChangeRegValue_Click(sender, thisEvent);
                thisEvent.Handled = true;
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
            if(ev.KeyChar.Equals((char)13)) //13 = enter
            {
                if(!skinFolderSelector.Items.Contains(skinFolderSelector.Text) && !String.IsNullOrWhiteSpace(skinFolderSelector.Text) &&
                        skinFolderSelector.Text != "All" && skinFolderSelector.Text != ",")
                {
                    skinFolderSelector.Items.Add(skinFolderSelector.Text);
                    ChangeRegValue(RegValueNames.skinFolders, (GetRegValue(RegValueNames.skinFolders) == null ? "" : GetRegValue(RegValueNames.skinFolders) + ",") + skinFolderSelector.Text);
                }
                else if(skinFolderSelector.Text == ",")
                {
                    skinFolderSelector.Text = "All";
                    DebugLog("You cannot add \",\" as a prefix");
                }
                ev.Handled = true;
            }
        };
        if(!String.IsNullOrWhiteSpace(GetRegValue(RegValueNames.skinFolders)))
        {
            if(!skinFolderSelector.Items.Contains("All"))
                skinFolderSelector.Items.Add("All");
            
            string[] foldersArr = GetRegValue(RegValueNames.skinFolders).Split(',');

            foreach(string name in foldersArr)
                skinFolderSelector.Items.Add(name);
        }
        else
        {
            skinFolderSelector.Items.Add("All");
            skinFolderSelector.Text = "All";
        }
        if(!String.IsNullOrWhiteSpace(GetRegValue(RegValueNames.selectedSkinFolder)))
            skinFolderSelector.Text = GetRegValue(RegValueNames.selectedSkinFolder);
        else
            skinFolderSelector.Text = "All";
        
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

        hideSelectedSkinFolderButton = new System.Windows.Forms.Button()
        {
            Top = 33,
            Font = mainFont,
            Width = 48,
            Left = 102,
            Text = "Hide",
        };
        hideSelectedSkinFolderButton.Click += new EventHandler(SearchOsuSkins);
        Controls.Add(hideSelectedSkinFolderButton);
        
        hiddenFoldersText = new CheckBox()
        {
            Top = 33,
            Font = mainFont,
            Left = 136,
            Text = "a",
            TextAlign = ContentAlignment.MiddleLeft,
        };
        
        deleteSkinSelectorButton = new System.Windows.Forms.Button()
        {
            Top = 33,
            Font = mainFont,
            Width = 61,
            Left = 156,
            Text = "Delete",
        };
        hiddenFoldersText.TextChanged += new EventHandler((sender, ev) =>
        {
            int textWidth = TextRenderer.MeasureText(hiddenFoldersText.Text, mainFont).Width;
            if(textWidth == 0)
            {
                if(Controls.Contains(hiddenFoldersText))
                    Controls.Remove(hiddenFoldersText);
                deleteSkinSelectorButton.Left = 153;
            }
            else
            {
                if(!Controls.Contains(hiddenFoldersText))
                    Controls.Add(hiddenFoldersText);

                deleteSkinSelectorButton.Left = textWidth + 152;
                hiddenFoldersText.Width =  textWidth + 30;
            }
        });
        Controls.Add(deleteSkinSelectorButton);
        
        if(!String.IsNullOrWhiteSpace(GetRegValue(RegValueNames.hiddenSkinFolders)))
            hiddenFoldersText.Text = hiddenFoldersText.Text = GetRegValue(RegValueNames.hiddenSkinFolders).Replace(",", ", ");
        else
            hiddenFoldersText.Text = "";

        deleteSkinSelectorButton.Click += new EventHandler((sender, e) =>
        {
            if(skinFolderSelector.Items.Contains(skinFolderSelector.Text))
            {
                string[] origValue = GetRegValue(RegValueNames.skinFolders).Split(',');
                string fixedValue = "";

                foreach(string name in origValue)
                {
                    if(skinFolderSelector.Text != name)
                        fixedValue += name + ",";
                }
                ChangeRegValue(RegValueNames.skinFolders, fixedValue.Remove(fixedValue.Length-1));

                skinFolderSelector.Items.Remove(skinFolderSelector.Text);
            }
            else if(skinFolderSelector.Text == "All")
                DebugLog("You can't delete the \"All\" prefix silly.");

            skinFolderSelector.Text = "All";
            ChangeRegValue(RegValueNames.selectedSkinFolder, "All");
        });

        writeCurrSkinBox = new CheckBox()
        {
            Height = 25,
            Width = 100,
            Left = 483,
            Top = 3,
            Text = "Write current skin to .txt file",
            TextAlign = ContentAlignment.MiddleLeft,
        };
        if(!String.IsNullOrWhiteSpace(GetRegValue(RegValueNames.writeCurrSkinToTXT)))
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
        if(!String.IsNullOrWhiteSpace(GetRegValue(RegValueNames.disableSkinChangesBox)))
            disableSkinChangesBox.Checked = bool.Parse(GetRegValue(RegValueNames.disableSkinChangesBox));
        else
            disableSkinChangesBox.Checked = true;
        
        disableSkinChangesBox.CheckedChanged += new EventHandler(ChangeRegValue_Click);
        Controls.Add(disableSkinChangesBox);
        
        SetupSkinChangeCheckBoxes();
        toolTip = new ToolTip();
        toolTip.SetToolTip(searchOsuSkinsButton, "Searches osu! folder for skins");
        toolTip.SetToolTip(changeOsuPathButton, "Set the path that houses the osu!.exe");
        toolTip.SetToolTip(writeCurrSkinBox, "Writes the name of the current skin to a text\nfile in the \"Skins\" folder. Helpful for streamers.");
        toolTip.SetToolTip(randomSkinButton, "Selects random skin from visible skins");
        toolTip.SetToolTip(openSkinFolderButton, "Opens current skin folder");
        toolTip.SetToolTip(useSkinButton, "Changes to selected skin. If multiple\nare selected, it chooses a random one.");
        toolTip.SetToolTip(deleteSkinButton, "Moves selected skin to \"Deleted Skins\" folder");
        toolTip.SetToolTip(showSkinNumbersBox, "Controls if numbers are shown on hitcircles");
        toolTip.SetToolTip(showSliderEndsBox, "Controls if slider ends are visible\nChecked means that they are shown.");
        toolTip.SetToolTip(skinFolderSelector, "Allows you to designate a prefix on the skin folders to categorize the skins");
        toolTip.SetToolTip(disableSkinChangesBox, "Disables all changes to skin files and only copies them over");
        toolTip.SetToolTip(disableCursorTrailBox, "Checked means no cursor trail is shown.\nWill not add trail to skin that does not have it.");
        toolTip.SetToolTip(showFilteredSkinsButton, "Shows only the skins with the selected prefix.\nResets hidden folders.");
        toolTip.SetToolTip(hideSelectedSkinFolderButton, "Hides skins with selected prefix.\nPress show button to reset them.");
        toolTip.SetToolTip(deleteSkinSelectorButton, "Deletes skin prefix from list");
        toolTip.SetToolTip(showComboBurstsBox, "If checked, combo bursts will be shown if the skin has them");
        toolTip.SetToolTip(hiddenFoldersText, "The skins with these prefixes are hidden from the list");

        openFileDialog1 = new System.Windows.Forms.OpenFileDialog()
        {
            InitialDirectory =  osuPath,
            Filter = "Directory|*.directory",
            FileName =  "",
            Title = "Select osu! directory"
        };
        
        //startUpTime = false;
        DirectoryInfo osuPathDI = new DirectoryInfo(osuPath + "\\skins");
        if(osuPathDI.Exists)
        {
            if(!Directory.Exists(mainSkinPath))
                osuPathDI.CreateSubdirectory("!!!osu!helper Skin");
            
            if(!Directory.Exists(mainSkinPath.Replace("!!!osu!helper Skin", "Deleted Skins")))
                osuPathDI.CreateSubdirectory("Deleted Skins");

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

    private void SetupSkinChangeCheckBoxes()
    {
        CheckBox current;
        int num;
        bool defaultCheckedState;

        for (int i = 0; i <= 5; i++)
        {
            defaultCheckedState =  false;
            current = new CheckBox();
            current.Height = 25;
            current.Width = 297;
            current.Font = mainFont;
            current.Left = 503;
            current.Top = 60 + (i*20);
            current.TextAlign = ContentAlignment.MiddleLeft;
            current.CheckedChanged += new EventHandler(ChangeRegValue_Click);
            Controls.Add(current);
            num = Controls.IndexOf(current);

            switch (i)
            {
                case 0:
                    Controls[num].Name = RegValueNames.showComboBurstsBox.ToString(); 
                    Controls[num].Text = "Combo Bursts";
                    showComboBurstsBox = (CheckBox)Controls[num];
                    break;
                case 1:
                    Controls[num].Name = RegValueNames.showHitlightingBox.ToString(); 
                    Controls[num].Text = "Hit Lighting";
                    showHitlightingBox = (CheckBox)Controls[num];
                    break;
                case 2:
                    Controls[num].Name = RegValueNames.showSkinNumbersBox.ToString();
                    Controls[num].Text = "Hitcircle Numbers";
                    showSkinNumbersBox = (CheckBox)Controls[num];
                    defaultCheckedState = true;
                    break;
                case 3:
                    Controls[num].Name = RegValueNames.disableCursorTrailBox.ToString();
                    Controls[num].Text = "Cursor Trail";
                    disableCursorTrailBox = (CheckBox)Controls[num];
                    defaultCheckedState = true;
                    break;
                case 4:
                    Controls[num].Name = RegValueNames.showSliderEndsBox.ToString();
                    Controls[num].Text = "Slider Ends";
                    showSliderEndsBox = (CheckBox)Controls[num];
                    break;
                case 5:
                    Controls[num].Name = RegValueNames.showHitCircles.ToString();
                    Controls[num].Text = "Show Hitcircles";
                    showHitCircles = (CheckBox)Controls[num];
                    defaultCheckedState = true;
                    break;
                default:
                    DebugLog("Error bulding CheckBoxes. i = " + i.ToString());
                    return;
            }

            if(!String.IsNullOrWhiteSpace(GetRegValue((RegValueNames)Enum.Parse(typeof(RegValueNames), Controls[num].Name, true))))
                ((CheckBox)Controls[num]).Checked = bool.Parse(GetRegValue((RegValueNames)Enum.Parse(typeof(RegValueNames), Controls[num].Name, true)));
            else
                ((CheckBox)Controls[num]).Checked = defaultCheckedState;
        }
    }

//MISC Skin handling
    private void ChangeOsuPathButton_Click(object sender, EventArgs e)
    {
        FolderBrowserDialog directorySelector = new FolderBrowserDialog()
        {
            ShowNewFolderButton = false,
            RootFolder = Environment.SpecialFolder.MyComputer,
            Description = "Select an osu! root directory:",
            SelectedPath = osuPath,
        };

        DialogResult givenPath = directorySelector.ShowDialog();
        if (givenPath == DialogResult.OK)
        {
            if (!File.Exists(directorySelector.SelectedPath + "\\osu!.exe"))
            {
                MessageBox.Show("Not a valid osu! directory!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                directorySelector.Dispose();
                ChangeOsuPathButton_Click(sender, e);
                return;
            }
            osuPathBox.Text = directorySelector.SelectedPath;
            osuPathBox.Text = directorySelector.SelectedPath;
            osuPath = directorySelector.SelectedPath;

            ChangeRegValue_Click(sender, e);
        }
        directorySelector.Dispose();
    }

    private void SearchOsuSkins(object sender, EventArgs e)
    {
        List<string> hiddenSkinFoldersList = new List<string>();

        if(sender == hideSelectedSkinFolderButton)
        {
            //Adds selected prefix to hidden list
            if(skinFolderSelector.Text != "All" && !String.IsNullOrWhiteSpace(GetRegValue(RegValueNames.hiddenSkinFolders)) &&
                        !GetRegValue(RegValueNames.hiddenSkinFolders).Contains(skinFolderSelector.Text))
            {
                ChangeRegValue(RegValueNames.hiddenSkinFolders, GetRegValue(RegValueNames.hiddenSkinFolders) + "," + skinFolderSelector.Text);
                hiddenFoldersText.Text = GetRegValue(RegValueNames.hiddenSkinFolders).Replace(",", ", ");
            }
            else if(String.IsNullOrWhiteSpace(GetRegValue(RegValueNames.hiddenSkinFolders)))
            {
                ChangeRegValue(RegValueNames.hiddenSkinFolders, skinFolderSelector.Text);
                hiddenFoldersText.Text = GetRegValue(RegValueNames.hiddenSkinFolders).Replace(",", ", ");
            }
            skinFolderSelector.Text = "All";
        }
        else if(sender == showFilteredSkinsButton)
        {
            hiddenFoldersText.Text = "";
            ChangeRegValue(RegValueNames.hiddenSkinFolders, "");
            if(skinFolderSelector.Text == ",")
            {
                skinFolderSelector.Text = "All";
                DebugLog("You cannot use \",\" as a prefix");
            }
        }

        if(!String.IsNullOrWhiteSpace(GetRegValue(RegValueNames.hiddenSkinFolders)))
            hiddenSkinFoldersList = GetRegValue(RegValueNames.hiddenSkinFolders).Split(',').ToList();
        else
            hiddenSkinFoldersList.Add(osuPath);
        
        osuSkinsListBox.ClearSelected();
        osuSkinsListBox.BeginUpdate();
        osuSkinsListBox.Items.Clear();
        osuSkinsPathList.Clear();
        DirectoryInfo di = new DirectoryInfo(Path.Combine(osuPath, "skins"));
        var osuSkins = di.GetDirectories();

        foreach(DirectoryInfo skin in osuSkins)
        {
            string skinName = skin.FullName.Replace(osuPath + "\\skins\\", "");

            if(skinName != "!!!osu!helper Skin" && skinName != "Deleted Skins")
            {
                if(!hiddenSkinFoldersList.Contains(skinName.ElementAt<char>(0).ToString())) //true if skin does not have prefix that is supposed to be hidden
                {
                    if(skinFolderSelector.Text != "All")
                    {
                        if(skinName.IndexOf(skinFolderSelector.Text) == 0) //true if skin has prefix that is only supposed to be shown
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
        }
        osuSkinsListBox.EndUpdate();
        if(!Controls.Contains(osuSkinsListBox))
            Controls.Add(osuSkinsListBox);
        
        //if skin that was last selected is shown, select it
        if(!String.IsNullOrWhiteSpace(GetRegValue(RegValueNames.skinName)) && osuSkinsListBox.Items.IndexOf(GetRegValue(RegValueNames.skinName)) != -1)
            osuSkinsListBox.SetSelected(osuSkinsListBox.Items.IndexOf(GetRegValue(RegValueNames.skinName)), true);

        if(sender == showFilteredSkinsButton)
        {
            if(!skinFolderSelector.Items.Contains(skinFolderSelector.Text))
                skinFolderSelector.Items.Add(skinFolderSelector.Text);

            ChangeRegValue_Click(sender, e);
        }
        
        if(!osuSkinsListBox.Items.Contains(skinFolderSelector.Text) && !String.IsNullOrWhiteSpace(GetRegValue(RegValueNames.skinName))) //if that was last selected is not shown,
            ChangeRegValue(RegValueNames.skinName, "");
    }

    private void OpenSkinFolder(object sender, EventArgs e)
    {
        if(osuSkinsListBox.SelectedItem == null)
        {
            DebugLog("Select skin before trying to open its folder");
            return;
        }
        Process.Start("explorer.exe", Path.Combine(osuPath, "skins", osuSkinsListBox.SelectedItem.ToString()));
    }

    private void DeleteSelectedSkin(object sender, EventArgs e)
    {
        if(osuSkinsListBox.SelectedItem == null)
        {
            DebugLog("Find skins first. Error occurred when trying to delete skin");
            return;
        }
        string skinPath = osuSkinsPathList[osuSkinsListBox.SelectedIndex];
        Directory.Move(skinPath, Path.Combine(osuPath,  "skins", "Deleted Skins", osuSkinsListBox.SelectedItem.ToString()));
        
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
        if(osuSkinsListBox.SelectedItems.Count == 1) //false if multiple skins are selected
            skinPath = osuSkinsPathList[osuSkinsListBox.SelectedIndex];
        else
        {
            Random random = new Random();
            int randomSkinIndex = osuSkinsListBox.Items.IndexOf(osuSkinsListBox.SelectedItems[random.Next(0, osuSkinsListBox.SelectedItems.Count)]);
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
            file.CopyTo(Path.Combine(mainSkinPath, file.Name), true);
        }
        RecursiveSkinFolderMove(skinPath, "");

        if(!disableSkinChangesBox.Checked)
        {
            ShowHideHitCircleNumbers(showSkinNumbersBox.Checked);
            ShowHideSliderEnds(showSliderEndsBox.Checked);
            DisableCursorTrail(disableCursorTrailBox.Checked);
            ShowHideCombobursts(showComboBurstsBox.Checked);
            ShowHitLighting(showHitlightingBox.Checked);
            ShowHideHitCircles(showHitCircles.Checked);
        }
    }

    private void RecursiveSkinFolderMove(string skinPath, string prevFolder)
    {
        DirectoryInfo rootFolder = new DirectoryInfo(skinPath + prevFolder);

        foreach(DirectoryInfo folder in rootFolder.GetDirectories())
        {
            Directory.CreateDirectory(Path.Combine(mainSkinPath + prevFolder, folder.Name));
            DirectoryInfo subFolder = new DirectoryInfo(Path.Combine(skinPath + prevFolder, folder.Name));

            if(subFolder.GetDirectories().Length != 0) //if there are still more subdirectories
                RecursiveSkinFolderMove(skinPath, Path.Combine(prevFolder, folder.Name));

            foreach(FileInfo file in subFolder.GetFiles())
                file.CopyTo(mainSkinPath + Path.Combine(prevFolder, folder.Name, file.Name), true);
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
    private void ShowHideHitCircles(bool show)
    {
        List<string> names = new List<string>()
        {
            "hitcircle.png",
            "hitcircle@2x.png",
            "hitcircleoverlay.png",
            "hitcircleoverlay@2x.png",
            "sliderstartcircle.png",
            "sliderstartcircle@2x.png",
            "sliderstartcircleoverlay.png",
            "sliderstartcircleoverlay@2x.png",
        };
        if(show)
        {
            foreach(string name in names)
                if(File.Exists(Path.Combine(GetCurrentSkinPath(), name)))
                    File.Copy(Path.Combine(GetCurrentSkinPath(), name), Path.Combine(mainSkinPath, name), true);
        }
        else
        {
            Bitmap emptyImage = new Bitmap(1,1);
            foreach(string name in names)
                emptyImage.Save(Path.Combine(mainSkinPath, name));
            
            emptyImage.Dispose();
        }
    }
    
    private void ShowHitLighting(bool show)
    {
        List<string> fileNames = new List<string>()
        {
            "lighting.png",
            "lighting@2x.png",
        };

        if(show)
        {
            foreach(string name in fileNames)
            {
                if(File.Exists(Path.Combine(GetCurrentSkinPath(), name)))
                {
                    Image thisImg = Image.FromFile(Path.Combine(GetCurrentSkinPath(), name));
                    if(thisImg.Height > 100)
                    {
                        File.Copy(Path.Combine(GetCurrentSkinPath(), name), Path.Combine(mainSkinPath, name), true);
                        thisImg.Dispose();
                        continue;
                    }
                    thisImg.Dispose();
                    File.Delete(Path.Combine(mainSkinPath, name));
                }
                else if(File.Exists(Path.Combine(mainSkinPath, name)))
                {
                    File.Delete(Path.Combine(mainSkinPath, name));
                }
            }
        }
        else
        {
            Image emptyImage = new Bitmap(1,1);
            foreach(string name in fileNames)
            {
                /* if(File.Exists(Path.Combine(mainSkinPath, name)))
                    File.Delete(Path.Combine(mainSkinPath, name)); */

                emptyImage.Save(Path.Combine(mainSkinPath, name));
            }
            emptyImage.Dispose();
        }
    }

    private void ShowHideCombobursts(bool show)
    {
        Bitmap emptyImage = new Bitmap(1,1);
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
                emptyImage.Save(Path.Combine(mainSkinPath, name + ".png"));
            }
            /* else if(!show)
            {
                emptyImage.Save(Path.Combine(mainSkinPath, name));
            } */
        }

        //incase there are multiple combobursts
        if(!show)
        {
            DirectoryInfo di = new DirectoryInfo(mainSkinPath);
            foreach(FileInfo file in di.GetFiles()) 
            {
                foreach(string name in fileNames)
                    if(file.Name.Contains(name))
                        emptyImage.Save(Path.Combine(mainSkinPath, file.Name));
            }
            emptyImage.Dispose();
        }
    }
    
    private void DisableCursorTrail(bool show)
    {
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
            Bitmap emptyImage = new Bitmap(1, 1);
            foreach(string name in names)
            {
                if(File.Exists(Path.Combine(mainSkinPath, name)))
                    File.Delete(Path.Combine(mainSkinPath, name));

                emptyImage.Save(Path.Combine(mainSkinPath, name));
            }
            emptyImage.Dispose();
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
        string skipAt2X = "";

        if(show)//Showing ends
        {
            foreach(string fileName in sliderEnds)
            {
                if(skipAt2X == fileName)
                {
                    skipAt2X = "";
                    continue;
                }
                
                using(Image image = (File.Exists(Path.Combine(GetCurrentSkinPath(), fileName)) ? Image.FromFile(Path.Combine(GetCurrentSkinPath(), fileName)) : null))
                {
                    if(File.Exists(Path.Combine(GetCurrentSkinPath(), fileName)) && (image == null ? true : image.Size.Height < 100))
                    {
                        File.Copy(Path.Combine(GetCurrentSkinPath(), fileName), Path.Combine(mainSkinPath, fileName), true);
                    }
                    else if(File.Exists(Path.Combine(GetCurrentSkinPath(), fileName.Replace("end", "start"))))
                    {
                        File.Copy(Path.Combine(GetCurrentSkinPath(), fileName.Replace("sliderendcircle", "sliderstartcircle")), Path.Combine(mainSkinPath, fileName), true);
                        if(!File.Exists(Path.Combine(GetCurrentSkinPath(), fileName.Replace(".png", "@2x.png"))))
                        {
                            skipAt2X = fileName.Replace(".png", "@2x.png");
                        }
                    }
                    else if(File.Exists(Path.Combine(GetCurrentSkinPath(), fileName.Replace("sliderend", "hit"))))
                    {
                        File.Copy(Path.Combine(GetCurrentSkinPath(), fileName.Replace("sliderend", "hit")), Path.Combine(mainSkinPath, fileName), true);
                    }
                    /* else if(File.Exists(Path.Combine(mainSkinPath, fileName)))
                    {
                        sliderImage = Image.FromFile(Path.Combine(mainSkinPath, fileName));
                        if(sliderImage.Size.Height < 100)
                        {
                            sliderImage.Dispose();
                            File.Delete(Path.Combine(mainSkinPath, fileName));
                        }
                        sliderImage.Dispose();
                    } */
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
            file.Delete();
        
        foreach(DirectoryInfo folder in rootFolder.GetDirectories())
            Directory.Delete(folder.FullName, true);
    }

    private void ShowHideHitCircleNumbers(bool show)
    {
        if(show) //show skin numbers 
            File.Copy(Path.Combine(GetCurrentSkinPath(), "skin.ini"), Path.Combine(mainSkinPath, "skin.ini"), true);
        else //hide skin numbers
            EditSkinIni("HitCirclePrefix:", "HitCirclePrefix: 727", "[Fonts]");
    }

    private void EditSkinIni(string searchFor, string replaceWith, string fallBackSearch)
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
                if(currLine.Contains(fallBackSearch))
                {
                    
                    writerNew.WriteLine(currLine);
                    writerNew.WriteLine(replaceWith);
                    continue;
                }
                writerNew.WriteLine(currLine);
            }
            writerNew.Close();
            writerNew.Dispose();
            readerNew.Close();
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
        else if(sender == showHitlightingBox)
        {
            valName = RegValueNames.showHitlightingBox;
            val = showHitlightingBox.Checked.ToString();
            ShowHitLighting(showHitlightingBox.Checked);
        }
        else if(sender == showHitCircles)
        {
            valName = RegValueNames.showHitCircles;
            val = showHitCircles.Checked.ToString();
            ShowHideHitCircles(showHitCircles.Checked);
        }
        else
        {
            DebugLog("Error with ChangeRegValue_Click" + sender.ToString());
            return;
        }

        ChangeRegValue(valName, val);
    }

    private void ChangeRegValue(RegValueNames valueName, string value)
    {
        //DebugLog("Changed Reg value of " + valueName.ToString() + " to " + value + ".");
        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", valueName.ToString(), value);
    }
    
    private string GetRegValue(RegValueNames valName)
    {
        try
        {
            //DebugLog("Reg value of " + valName.ToString() + " is " + Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", valName.ToString(), null).ToString() + ".");
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
        else if(String.IsNullOrWhiteSpace(GetRegValue(RegValueNames.skinName)) || osuSkinsListBox.SelectedItems.Count > 1)
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

    private void DebugLog(bool log)
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