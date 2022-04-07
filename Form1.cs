using System.Net.Mime;
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
        private CheckBox showHitCirclesBox;
        private CheckBox makeInstafadeBox;
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
        makeInstafadeBox,
    };
    bool debugMode = true;
    bool spamLogs = false;
    
    public void FormLayout(bool debugModeArgs, bool spamLogsArgs)
    {
        debugMode = debugModeArgs;
        spamLogs = spamLogsArgs;
        DebugLog("[STARTING UP]", false);
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
            Width = 139,
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
                    DebugLog("You cannot add \",\" as a prefix", true);
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
                DebugLog("You can't delete the \"All\" prefix silly.", true);

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
        {
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
            //toolTip.SetToolTip(makeInstafadeBox, "Makes hitcircles fade instantly\nMay not convert back from instafade correctly\nMake intermediate (grey) to disable editing");
        }

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
                DebugLog("Error searching skins", true);
            }
        }
        DebugLog("[STARTUP FINISHED. WAITING FOR INPUT]", false);
    }

    private void SetupSkinChangeCheckBoxes()
    {
        CheckBox current;
        int num;
        bool defaultCheckedState;

        for (int i = 0; i <= 6; i++)
        {
            if(i == 6)
                continue;
            defaultCheckedState =  false;
            current = new CheckBox();
            current.Height = 25;
            current.Width = 297;
            current.Font = mainFont;
            current.Left = 503;
            current.Top = 60 + (i*20);
            current.TextAlign = ContentAlignment.MiddleLeft;
            current.CheckStateChanged += new EventHandler(ChangeRegValue_Click);
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
                    showHitCirclesBox = (CheckBox)Controls[num];
                    defaultCheckedState = true;
                    break;
                case 6:
                    Controls[num].Name = RegValueNames.makeInstafadeBox.ToString();
                    Controls[num].Text = "Make Instafade";
                    makeInstafadeBox = (CheckBox)Controls[num];
                    makeInstafadeBox.ThreeState = true;
                    break;
                default:
                    DebugLog("Error bulding CheckBoxes. i = " + i.ToString(), true);
                    return;
            }
            string regValue = GetRegValue((RegValueNames)Enum.Parse(typeof(RegValueNames), Controls[num].Name, true));
            switch(regValue)
            {
                case "True":
                    ((CheckBox)Controls[num]).Checked = true;
                    break;
                case "False":
                    ((CheckBox)Controls[num]).Checked = false;
                    break;
                case "Indeterminate":
                    ((CheckBox)Controls[num]).CheckState = CheckState.Indeterminate;
                    break;
                case "Checked":
                    ((CheckBox)Controls[num]).CheckState = CheckState.Checked;
                    break;
                case "Unchecked":
                    ((CheckBox)Controls[num]).CheckState = CheckState.Unchecked;
                    break;
                default:
                    ((CheckBox)Controls[num]).Checked = defaultCheckedState;
                    break;
            }
            DebugLog($"CheckBox Name is: {Controls[num].Name} | Text is: {Controls[num].Text} | Checked: {((CheckBox)Controls[num]).Checked.ToString()}", false);
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
            DebugLog("osuPath set to: "+osuPath,false);
            ChangeRegValue_Click(sender, e);
        }
        directorySelector.Dispose();
    }

    private void SearchOsuSkins(object sender, EventArgs e)
    {
        List<string> hiddenSkinFoldersList = new List<string>();

        if(sender == hideSelectedSkinFolderButton)
        {
            DebugLog("hideSelectedSkinFolderButton called SearchOsuSkins()", false);
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
            DebugLog("showFilteredSkinsButton called SearchOsuSkins()", false);
            hiddenFoldersText.Text = "";
            ChangeRegValue(RegValueNames.hiddenSkinFolders, "");
            if(skinFolderSelector.Text == ",")
            {
                skinFolderSelector.Text = "All";
                DebugLog("You cannot use \",\" as a prefix", true);
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
                            if(spamLogs)
                                DebugLog($"Adding {skinName} to the skin list | {skin.FullName}", false);
                        }
                    }
                    else
                    {
                        osuSkinsPathList.Add(skin.FullName);
                        osuSkinsListBox.Items.Add(skinName);
                        if(spamLogs)
                            DebugLog($"Adding {skinName} to the skin list | {skin.FullName}", false);
                    }
                }
            }
        }
        osuSkinsListBox.EndUpdate();
        if(!Controls.Contains(osuSkinsListBox))
            Controls.Add(osuSkinsListBox);
        
        //if skin that was last selected is shown, select it
        string lastSkinName = GetRegValue(RegValueNames.skinName);
        if(!String.IsNullOrWhiteSpace(lastSkinName) && osuSkinsListBox.Items.IndexOf(lastSkinName) != -1)
        {
            osuSkinsListBox.SetSelected(osuSkinsListBox.Items.IndexOf(lastSkinName), true);
            DebugLog("Previous selected skin name: " + lastSkinName, false);
        }
            

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
            DebugLog("Select skin before trying to open its folder", true);
            return;
        }
        DebugLog("Attempting open skin folder: " + Path.Combine(osuPath, "skins", osuSkinsListBox.SelectedItem.ToString()), false);
        Process.Start("explorer.exe", Path.Combine(osuPath, "skins", osuSkinsListBox.SelectedItem.ToString()));
    }

    private void DeleteSelectedSkin(object sender, EventArgs e)
    {
        EnableAllControls(false);
        if(osuSkinsListBox.SelectedItem == null)
        {
            DebugLog("Find skins first. Error occurred when trying to delete skin", true);
            return;
        }
        string skinPath = osuSkinsPathList[osuSkinsListBox.SelectedIndex];
        Directory.Move(skinPath, Path.Combine(osuPath,  "skins", "Deleted Skins", osuSkinsListBox.SelectedItem.ToString()));
        
        DebugLog($"Moving {skinPath} to {Path.Combine(osuPath,  "skins", "Deleted Skins", osuSkinsListBox.SelectedItem.ToString())}", false);
        osuSkinsPathList.RemoveAt(osuSkinsListBox.SelectedIndex);
        osuSkinsListBox.Items.RemoveAt(osuSkinsListBox.SelectedIndex);
        EnableAllControls(true);
    }

//Skin Switching
    private void ChangeToSelectedSkin(object sender, EventArgs e)
    {
        DebugLog("[STARTING TO CHANGE SKIN]", false);
        if(osuSkinsListBox.SelectedItem == null)
        {
            DebugLog("Please select skin before trying to change to it.", true);
            return;
        }
        EnableAllControls(false);

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
            DebugLog($"Changing to random skin from selected | Selected: {osuSkinsListBox.SelectedItem.ToString()}", false);
        }
        ChangeRegValue(RegValueNames.skinName, osuSkinsListBox.SelectedItem.ToString());

        if(writeCurrSkinBox.Checked)
            UpdateSkinTextFile(skinPath.Replace(Path.Combine(osuPath, "skins") + "\\", ""));

        DirectoryInfo di = new DirectoryInfo(skinPath);
        
        foreach(FileInfo file in di.GetFiles())
        {
            file.CopyTo(Path.Combine(mainSkinPath, file.Name), true);
            if(spamLogs)
                DebugLog($"Copying \"{file.FullName}\" to \"{Path.Combine(mainSkinPath, file.Name)}\"", false);
        }
        RecursiveSkinFolderMove(skinPath, "\\");

        DebugLog("[FINISHED CHANGING TO SKIN]", false);
        
        if(!disableSkinChangesBox.Checked)
        {
            DebugLog("[STARTING EDITING SKIN]", false);
            ShowHitCircleNumbers(showSkinNumbersBox.Checked);
            ShowSliderEnds(showSliderEndsBox.Checked);
            DisableCursorTrail(disableCursorTrailBox.Checked);
            ShowCombobursts(showComboBurstsBox.Checked);
            ShowHitLighting(showHitlightingBox.Checked);
            ShowHitCircles(showHitCirclesBox.Checked);
            //MakeInstafade(makeInstafadeBox.CheckState);
            DebugLog("[FINISHED EDITING SKIN]", false);
        }
        EnableAllControls(true);
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
                if(spamLogs)
                    DebugLog($"Copying \"{file.FullName}\" to \"{Path.Combine(mainSkinPath + prevFolder, folder.Name, file.Name)}\"", false);
            }  
        }
    }
    
    private void RandomSkin_Click(object sender, EventArgs e)
    {
        EnableAllControls(false);
        var randomNumber = new Random();
        osuSkinsListBox.ClearSelected();
        osuSkinsListBox.SetSelected(randomNumber.Next(0, osuSkinsListBox.Items.Count), true);
        DebugLog($"Changed to random skin. Picked \"{osuSkinsListBox.SelectedItem.ToString()}\"", false);
        ChangeToSelectedSkin(sender, e);
    }

//Skin editing
    private void MakeInstafade(CheckState instafade)
    {
        return;
        if(instafade == CheckState.Checked)
        {
            bool hitCircleOverlayAboveNumber = true;
            bool At2X;
            string searchINIForBool = SeachSkinINI("HitCircleOverlayAbove");
            if(searchINIForBool != null && searchINIForBool.Contains('0'))
                hitCircleOverlayAboveNumber = false;

            string hitcircleDefault = SeachSkinINI("HitCirclePrefix:").Replace("HitCirclePrefix:", "").Replace(" ", "").Replace("/", "\\");
            if(String.IsNullOrWhiteSpace(hitcircleDefault))
                hitcircleDefault = "default";

            if((File.Exists(Path.Combine(mainSkinPath, "hitcircle@2x.png")) || File.Exists(Path.Combine(mainSkinPath, "hitcircleoverlay@2x.png"))) &&
            File.Exists(Path.Combine(mainSkinPath, hitcircleDefault + "-1@2x.png")))
            {
                At2X = true;
            }
            else if((File.Exists(Path.Combine(mainSkinPath, "hitcircle.png")) || File.Exists(Path.Combine(mainSkinPath, "hitcircleoverlay.png"))) &&
            File.Exists(Path.Combine(mainSkinPath, hitcircleDefault + "-1.png")))
            {
                At2X = false;
            }
            else 
            {
                DebugLog($"Pair of hd and sd not found | Looking for prefix of {hitcircleDefault}", true);
                return;
            }


            Image hitcircleImage = (File.Exists(Path.Combine(mainSkinPath, "hitcircle@2x.png")) ? Image.FromFile(Path.Combine(mainSkinPath, "hitcircle@2x.png")) : new Bitmap(1,1));
            Image hitcircleOverlayImage = (File.Exists(Path.Combine(mainSkinPath, "hitcircleoverlay@2x.png")) ? Image.FromFile(Path.Combine(mainSkinPath, "hitcircleoverlay@2x.png")) : new Bitmap(1,1));
            Graphics hitcircleGraphics =  Graphics.FromImage(hitcircleImage);
            int imageWidth = hitcircleImage.Width/2;
            int imageHeight = hitcircleImage.Height/2;
            
            if(!hitCircleOverlayAboveNumber)
            {
                hitcircleGraphics.DrawImage(hitcircleOverlayImage, 0, 0);
                hitcircleGraphics.Save();
                //hitcircleImage.Save(Path.Combine(mainSkinPath, "!TEST.png"));
            }

            float x;
            float y;
            for (int i = 0; i <= 9; i++)
            {
                DebugLog(i, true);
                Image textImage = (File.Exists(Path.Combine(mainSkinPath, hitcircleDefault + "-"+i+"@2x.png")) ?
                    Image.FromFile(Path.Combine(mainSkinPath, hitcircleDefault + "-"+i+"@2x.png")) :
                    Image.FromFile(Path.Combine(mainSkinPath, hitcircleDefault + "-"+i+".png")));

                x = imageWidth - (textImage.Width/2);
                y = imageHeight - (textImage.Height/2);
                using(Image hitcircleImageTemp = (Image)hitcircleImage.Clone())
                {
                    Graphics tempGraphic = Graphics.FromImage(hitcircleImageTemp);

                    tempGraphic.DrawImage(textImage, x, y);
                    if(hitCircleOverlayAboveNumber)
                        tempGraphic.DrawImage(hitcircleOverlayImage, 0, 0);
                    
                    tempGraphic.Save();
                    //DebugLog(Path.Combine(mainSkinPath, hitcircleDefault + "-"+i+(At2X? "@2x": "")+".png"), true);
                    textImage.Dispose();
                    tempGraphic.Dispose();
                    //File.Delete(Path.Combine(mainSkinPath, hitcircleDefault + "-"+i+(At2X? "@2x": "@2x")+".png"));
                    string path = Path.Combine(mainSkinPath, hitcircleDefault + "-"+i+(At2X? "@2x": "")+".png");
                    hitcircleImageTemp.Save(path.Replace(".png", ".temp"));
                    //Process.Start("cmd.exe", $"/c ffmpeg -i {path.Replace(".png", ".temp")} -vf scale=320:320 -y {path}");
                    //File.Delete(path.Replace(".png", ".temp"));
                }
            }
            hitcircleImage.Dispose();
            hitcircleOverlayImage.Dispose();
            hitcircleGraphics.Dispose();
            EditSkinIni("HitCircleOverlap:", "HitCircleOverlap: " + imageWidth*2, "[Fonts]");
            ShowHitCircles(false);
        }
    }

    private void ShowHitCircles(bool show)
    {
        DebugLog($"ShowHitCircles({show}) called", false);
        EnableAllControls(false);
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
                {
                    File.Copy(Path.Combine(GetCurrentSkinPath(), name), Path.Combine(mainSkinPath, name), true);
                    if(spamLogs)
                        DebugLog($"Copying {Path.Combine(GetCurrentSkinPath(), name)} to {Path.Combine(mainSkinPath, name)}", false);
                }
                    
        }
        else
        {
            Bitmap emptyImage = new Bitmap(1,1);
            foreach(string name in names)
            {
                emptyImage.Save(Path.Combine(mainSkinPath, name));
                if(spamLogs)
                    DebugLog($"Copying empty image to {Path.Combine(mainSkinPath, name)}", false);
            }
                
            
            emptyImage.Dispose();
        }
        EnableAllControls(true);
    }
    
    private void ShowHitLighting(bool show)
    {
        DebugLog($"ShowHitLighting({show}) called", false);
        EnableAllControls(false);
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
                        if(spamLogs)
                            DebugLog($"Copying {Path.Combine(GetCurrentSkinPath(), name)} to {Path.Combine(mainSkinPath, name)}", false);
                        
                        continue;
                    }
                    thisImg.Dispose();
                    File.Delete(Path.Combine(mainSkinPath, name));
                    if(spamLogs)
                        DebugLog($"Deleting {Path.Combine(mainSkinPath, name)}", false);
                }
                else if(File.Exists(Path.Combine(mainSkinPath, name)))
                {
                    File.Delete(Path.Combine(mainSkinPath, name));
                    if(spamLogs)
                        DebugLog($"Deleting {Path.Combine(mainSkinPath, name)}", false);
                }
            }
        }
        else
        {
            Image emptyImage = new Bitmap(1,1);
            foreach(string name in fileNames)
            {
                emptyImage.Save(Path.Combine(mainSkinPath, name));
                if(spamLogs)
                    DebugLog($"Saving empty image to {Path.Combine(mainSkinPath, name)}", false);
            }
            emptyImage.Dispose();
        }
        EnableAllControls(true);
    }

    private void ShowCombobursts(bool show)
    {
        DebugLog($"ShowCombobursts({show}) called", false);
        EnableAllControls(false);
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
                if(spamLogs)
                    DebugLog($"Saving empty image to {Path.Combine(mainSkinPath, name + ".png")}", false);
            }
        }

        //incase there are multiple combobursts
        DirectoryInfo di = new DirectoryInfo(GetCurrentSkinPath());
        foreach(FileInfo file in di.GetFiles()) 
        {
            foreach(string name in fileNames)
                if(file.Name.Contains(name))
                {
                    if(show)
                        File.Copy(file.FullName, Path.Combine(mainSkinPath, file.Name), true);
                    else
                        emptyImage.Save(Path.Combine(mainSkinPath, file.Name));
                    
                    if(spamLogs)
                        DebugLog($"Saving empty image to {Path.Combine(mainSkinPath, file.Name)}", false);
                }
        }
        emptyImage.Dispose();
        EnableAllControls(true);
    }
    
    private void DisableCursorTrail(bool show)
    {
        DebugLog($"DisableCursorTrail({show}) called", false);
        EnableAllControls(false);
        List<string> names = new List<string>()
        {
            "cursortrail@2x.png",
            "cursortrail.png",
        };
        
        if(show)
        {
            foreach(string name in names)
                if(File.Exists(Path.Combine(GetCurrentSkinPath(), name)))
                {
                    File.Copy(Path.Combine(GetCurrentSkinPath(), name), Path.Combine(mainSkinPath, name), true);
                    if(spamLogs)
                        DebugLog($"Copying {Path.Combine(GetCurrentSkinPath(), name)} to {Path.Combine(mainSkinPath, name)}", false);
                }
                    
        }
        else
        {
            Bitmap emptyImage = new Bitmap(1, 1);
            foreach(string name in names)
            {
                emptyImage.Save(Path.Combine(mainSkinPath, name));
                if(spamLogs)
                    DebugLog($"Saving empty image to {Path.Combine(mainSkinPath, name)}", false);
            }
            emptyImage.Dispose();
        }
        EnableAllControls(true);
    }
    
    private void ShowSliderEnds(bool show)
    {
        DebugLog($"ShowSliderEnds({show}) called", false);
        EnableAllControls(false);
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
                        if(spamLogs)
                            DebugLog($"Copying {Path.Combine(GetCurrentSkinPath(), fileName)} to {Path.Combine(mainSkinPath, fileName)}", false);
                    }
                    else if(File.Exists(Path.Combine(GetCurrentSkinPath(), fileName.Replace("end", "start"))))
                    {
                        File.Copy(Path.Combine(GetCurrentSkinPath(), fileName.Replace("sliderendcircle", "sliderstartcircle")), Path.Combine(mainSkinPath, fileName), true);
                        if(spamLogs)
                            DebugLog($"Copying {Path.Combine(GetCurrentSkinPath(), fileName.Replace("sliderendcircle", "sliderstartcircle"))} to {Path.Combine(mainSkinPath, fileName)}", false);
                        if(!File.Exists(Path.Combine(GetCurrentSkinPath(), fileName.Replace(".png", "@2x.png"))))
                        {
                            skipAt2X = fileName.Replace(".png", "@2x.png");
                        }
                    }
                    else if(File.Exists(Path.Combine(GetCurrentSkinPath(), fileName.Replace("sliderend", "hit"))))
                    {
                        File.Copy(Path.Combine(GetCurrentSkinPath(), fileName.Replace("sliderend", "hit")), Path.Combine(mainSkinPath, fileName), true);
                        if(spamLogs)
                            DebugLog($"Copying {Path.Combine(GetCurrentSkinPath(), fileName.Replace("sliderend", "hit"))} to {Path.Combine(mainSkinPath, fileName)}", false);
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
        EnableAllControls(true);
    }
    
    private void DeleteSkinElementsInMainSkin()
    {
        DirectoryInfo rootFolder = new DirectoryInfo(mainSkinPath);
        
        foreach(FileInfo file in rootFolder.GetFiles())
        {
            if(spamLogs)
                DebugLog($"Deleting {file.FullName}", false);
            file.Delete();
        }
            
        
        foreach(DirectoryInfo folder in rootFolder.GetDirectories())
        {
            if(spamLogs)
                DebugLog($"Deleting {folder.FullName} (directory)", false);
            Directory.Delete(folder.FullName, true);
        }
    }

    private void ShowHitCircleNumbers(bool show)
    {
        EnableAllControls(false);
        if(show) //show skin numbers
        {
            File.Copy(Path.Combine(GetCurrentSkinPath(), "skin.ini"), Path.Combine(mainSkinPath, "skin.ini"), true);
            DebugLog($"Copying {Path.Combine(GetCurrentSkinPath(), "skin.ini")} to {Path.Combine(mainSkinPath, "skin.ini")}", false);
        }
        else //hide skin numbers
            EditSkinIni("HitCirclePrefix:", "HitCirclePrefix: 727", "[Fonts]");
        EnableAllControls(true);
    }

    private void EditSkinIni(string searchFor, string replaceWith, string fallBackSearch)
    {
        DebugLog($"Attempting to search for {searchFor}, replace it with {replaceWith}, with a fallback of {fallBackSearch}. In the skin.ini", false);
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
        if(osuSkinsListBox.SelectedIndex == -1)
        {
            return;
        }
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
            ShowHitCircleNumbers(showSkinNumbersBox.Checked);
        }
        else if(sender == showSliderEndsBox)
        {
            valName = RegValueNames.showSliderEndsBox;
            val = showSliderEndsBox.Checked.ToString();
            ShowSliderEnds(showSliderEndsBox.Checked);
        }
        else if(sender == disableSkinChangesBox)
        {
            if(!disableSkinChangesBox.Checked)
            {
                ShowHitCircleNumbers(showSkinNumbersBox.Checked);
                ShowSliderEnds(showSliderEndsBox.Checked);
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
            ShowCombobursts(showComboBurstsBox.Checked);
        }
        else if(sender == showHitlightingBox)
        {
            valName = RegValueNames.showHitlightingBox;
            val = showHitlightingBox.Checked.ToString();
            ShowHitLighting(showHitlightingBox.Checked);
        }
        else if(sender == showHitCirclesBox)
        {
            valName = RegValueNames.showHitCircles;
            val = showHitCirclesBox.Checked.ToString();
            ShowHitCircles(showHitCirclesBox.Checked);
        }
        else if(sender == makeInstafadeBox)
        {
            valName = RegValueNames.makeInstafadeBox;
            val = makeInstafadeBox.CheckState.ToString();
            MakeInstafade(makeInstafadeBox.CheckState);
        }
        else
        {
            DebugLog("Error with ChangeRegValue_Click" + sender.ToString(), true);
            return;
        }

        ChangeRegValue(valName, val);
    }

    private void ChangeRegValue(RegValueNames valueName, string value)
    {
        DebugLog($"Changed Reg value of {valueName.ToString()} to \"{value}\"", false);
        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", valueName.ToString(), value);
    }
    
    private string GetRegValue(RegValueNames valName)
    {
        try
        {
            DebugLog($"Reg value of {valName.ToString()} is \"{Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", valName.ToString(), null).ToString()}\"", false);
            return Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", valName.ToString(), null).ToString();
        }
        catch
        {
            return null;
        }
    }

//MISC
    private string SeachSkinINI(string searchFor)
    {
        DebugLog($"Searching skin.ini for {searchFor}", false);
        StreamReader reader = new StreamReader(Path.Combine(mainSkinPath, "skin.ini"));
        string currLine = null;
        while((currLine = reader.ReadLine()) != null)
        {
            if(currLine.Contains(searchFor))
                break;
        }
        reader.Close();
        reader.Dispose();
        return currLine;
    }

    private void EnableAllControls(bool enable)
    {
        DebugLog($"EnableAllControls({enable}) called", false);
        foreach (Control obj in Controls)
        {
            if(obj != null)
                obj.Enabled = enable;
        }
    }

    private string GetCurrentSkinPath()
    {
        if(osuSkinsListBox.SelectedItems.Count == 1)
            return osuSkinsPathList[osuSkinsListBox.SelectedIndex];
        else if(String.IsNullOrWhiteSpace(GetRegValue(RegValueNames.skinName)) || osuSkinsListBox.SelectedItems.Count > 1)
            DebugLog("Multiple/no skins selected. Unable to get skin path.", true);
        else
            return Path.Combine(osuPath, "skins", GetRegValue(RegValueNames.skinName));

        return null;
    }
    
    private void DebugLog(string log, bool alwaysLog)
    {
        if(alwaysLog || debugMode)
            Console.WriteLine(log);
    }

    private void DebugLog(int log, bool alwaysLog)
    {
        if(alwaysLog || debugMode)
            Console.WriteLine(log);
    }

    private void DebugLog(bool log, bool alwaysLog)
    {
        if(alwaysLog || debugMode)
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