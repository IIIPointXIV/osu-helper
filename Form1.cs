using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
public class Form1 : Form
{
    private TextBox osuFolderPathBox;
    private TextBox searchSkinBox;
    // Buttons
    private Button changeOsuPathButton;
    private Button searchOsuSkinsButton;
    private Button deleteSkinButton;
    private Button useSkinButton;
    private Button randomSkinButton;
    private Button openSkinFolderButton;
    private Button showFilteredSkinsButton;
    private Button hideSelectedSkinFilterButton;
    private Button deleteSkinSelectorButton;
    private Button renameSkinButton;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    // CheckBoxes
    //private CheckBox writeCurrSkinBox;
    private CheckBox showSkinNumbersBox;
    private CheckBox showSliderEndsBox;
    private CheckBox disableSkinChangesBox;
    private CheckBox disableCursorTrailBox;
    private CheckBox showComboBurstsBox;
    private CheckBox showHitLightingBox;
    private CheckBox hiddenSkinFiltersText;
    private CheckBox showHitCirclesBox;
    private CheckBox makeInstafadeBox;
    private CheckBox expandingCursorBox;
    private ToolTip toolTip;
    private ComboBox skinFilterSelector;
    private Font mainFont;
    private Font searchBoxFont;
    private ListBox osuSkinsListBox;
    private List<Skin> osuSkinsList = new List<Skin>();
    public static string osuPath { get; private set; }
    private Skin helperSkin;
    private string managerFolderName = "!!!Skin Manager";
    private enum ValueNames
    {
        disableCursorTrail,
        osuPath,
        selectedSkinFilter,
        showSkinNumbers,
        skinFilters,
        showSliderEnds,
        disableSkinChanges,
        showComboBursts,
        showHitLighting,
        hiddenSkinFilter,
        showHitCircles,
        makeInstafade,
        expandingCursor,
        //skinFilterSelector,
        writeCurrSkin,
        selectedSkin,
        searchSkinsText,
    };
    bool debugMode = false;
    bool spamLogs = false;
    Dictionary<ValueNames, string> loadedValues = new Dictionary<ValueNames, string>();

    public void FormLayout(bool debugMode, bool spamLogs)
    {
        this.debugMode = debugMode;
        this.spamLogs = spamLogs;
        this.FormClosing += new FormClosingEventHandler(SaveEditedValues);
        DebugLog("[STARTING UP]", false);
        LoadValues();
        mainFont = new Font("Segoe UI", 12);
        searchBoxFont = new Font("Segoe UI", 10);

        openFileDialog1 = new System.Windows.Forms.OpenFileDialog()
        {
            InitialDirectory = osuPath,
            Filter = "Directory|*.directory",
            FileName = "",
            Title = "Select osu! directory"
        };
        if (IsSavedValueEmpty(ValueNames.osuPath)) //If the path is not set in the reg, try to get default directory. If it is not there throw an error
        {
            osuPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "appdata", "Local", "osu!");
            if (!File.Exists(Path.Combine(osuPath, "osu!.exe")))
            {
                MessageBox.Show("Unable to find valid osu directory. Please select one.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ChangeOsuPathButton_Click(this, new EventArgs());
            }
        }
        else
            osuPath = GetValue(ValueNames.osuPath);

        helperSkin = new Skin(Path.Combine(osuPath, "skins", managerFolderName));

        this.MaximizeBox = false;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.Name = "Skin Manager";
        this.Text = "Skin Manager";
        this.Size = new Size(800, 800);
        this.Icon = new Icon("./Images/o_h_kDs_icon.ico");

        SetupOtherControls();
        SetupButtons();
        SetupCheckBoxes();
        SetupSkinChangeCheckBoxes();
        SetupToolTip();

        DirectoryInfo osuPathDI = new DirectoryInfo(Path.Combine(osuPath, "skins"));
        if (osuPathDI.Exists)
        {
            if (!Directory.Exists(helperSkin.path))
                osuPathDI.CreateSubdirectory(managerFolderName);

            if (!Directory.Exists(Path.Combine(osuPath, "skins", "Deleted Skins")))
                osuPathDI.CreateSubdirectory("Deleted Skins");

            try
            {
                FindOsuSkins(new Object(), new EventArgs());
            }
            catch
            {
                DebugLog("Error searching skins", true);
            }
        }
        DebugLog("[STARTUP FINISHED. WAITING FOR INPUT]", false);
    }

    //Setup Things
    private void SetupToolTip()
    {
        toolTip = new ToolTip();
        toolTip.SetToolTip(searchOsuSkinsButton, "Searches osu! folder for skins");
        toolTip.SetToolTip(changeOsuPathButton, "Set the path that houses the osu!.exe");
        //toolTip.SetToolTip(writeCurrSkinBox, "Writes the name of the current skin to a text\nfile in the \"Skins\" folder. Helpful for streamers.");
        toolTip.SetToolTip(randomSkinButton, "Selects random skin from visible skins");
        toolTip.SetToolTip(openSkinFolderButton, "Opens selected skin folder");
        toolTip.SetToolTip(useSkinButton, "Changes to selected skin. If multiple\nare selected, it chooses a random one.");
        toolTip.SetToolTip(deleteSkinButton, "Moves selected skin to \"Deleted Skins\" folder");
        toolTip.SetToolTip(showSkinNumbersBox, "Controls if numbers are shown on hitcircles");
        toolTip.SetToolTip(showSliderEndsBox, "Controls if slider ends are visible\nChecked means that they are shown.");
        toolTip.SetToolTip(skinFilterSelector, "Allows you to designate a prefix on the skin folders to categorize the skins");
        toolTip.SetToolTip(disableSkinChangesBox, "Disables all changes to skin files and only copies them over");
        toolTip.SetToolTip(disableCursorTrailBox, "Checked means no cursor trail is shown.\nWill not add trail to skin that does not have it.");
        toolTip.SetToolTip(showFilteredSkinsButton, "Shows only the skins with the selected prefix.\nResets hidden folders.");
        toolTip.SetToolTip(hideSelectedSkinFilterButton, "Hides skins with selected prefix.\nPress show button to reset them.");
        toolTip.SetToolTip(deleteSkinSelectorButton, "Deletes skin prefix from list");
        toolTip.SetToolTip(showComboBurstsBox, "If checked, combo bursts will be shown if the skin has them");
        toolTip.SetToolTip(hiddenSkinFiltersText, "The skins with these prefixes are hidden from the list");
        toolTip.SetToolTip(expandingCursorBox, "Checked means the cursor will expand on click");
        toolTip.SetToolTip(searchSkinBox, "Filter the skins by what you put here");
        toolTip.SetToolTip(renameSkinButton, "Renames selected skin.");
        //toolTip.SetToolTip(makeInstafadeBox, "Makes hitcircles fade instantly\nMay not convert back from instafade correctly\nMake intermediate (grey) to disable editing");
    }

    private void SetupOtherControls()
    {
        searchSkinBox = new TextBox()
        {
            Left = 218,
            Width = 100,
            Top = 32,
            Font = searchBoxFont,
            Name = ValueNames.searchSkinsText.ToString(),
            PlaceholderText = "Search for skin",
        };
        searchSkinBox.KeyUp += UserSearchSkins_Click;
        searchSkinBox.TextChanged += (sender, e) =>
        {
            int textWidth = TextRenderer.MeasureText(searchSkinBox.Text, searchBoxFont).Width;
            if (textWidth > 95)
                searchSkinBox.Width = textWidth + 5;
            else if (textWidth < searchSkinBox.Width - 5)
                searchSkinBox.Width = (textWidth < 95 ? 100 : textWidth + 5);
        };
        Controls.Add(searchSkinBox);
        if (!IsSavedValueEmpty(ValueNames.searchSkinsText))
            searchSkinBox.Text = GetValue(ValueNames.searchSkinsText);

        osuSkinsListBox = new ListBox()
        {
            Width = 500,
            Height = 676,
            Top = 60,
            Font = mainFont,
            SelectionMode = SelectionMode.MultiExtended,
        };

        osuFolderPathBox = new System.Windows.Forms.TextBox()
        {
            Text = osuPath,
            Left = 87,
            Width = 300,
            Font = mainFont,
            Name = ValueNames.osuPath.ToString(),
        };
        Controls.Add(osuFolderPathBox);
        osuFolderPathBox.KeyPress += (sender, thisEvent) =>
        {
            if (thisEvent.KeyChar.Equals((char)13))
            {
                if (!File.Exists(Path.Combine(osuFolderPathBox.Text, "osu!.exe")))
                {
                    MessageBox.Show("Not a valid osu! directory!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                osuPath = osuFolderPathBox.Text;
                helperSkin = new Skin(Path.Combine(osuPath, "skins", managerFolderName));
                OnClick(sender, thisEvent);
                thisEvent.Handled = true;
            }
        };

        skinFilterSelector = new ComboBox()
        {
            Top = 30,
            Font = mainFont,
            Height = 23,
            Width = 43,
            Name = ValueNames.skinFilters.ToString(),
        };
        skinFilterSelector.KeyUp += (sender, thisEvent) =>
        {
            if (thisEvent.KeyCode.Equals(Keys.Enter))
            {
                AddToSkinFilters();
                thisEvent.Handled = true;
            }
        };
        if (!IsSavedValueEmpty(ValueNames.skinFilters))
        {
            if (!skinFilterSelector.Items.Contains("All"))
                skinFilterSelector.Items.Add("All");

            string[] foldersArr = GetValue(ValueNames.skinFilters).Split(',');
            foreach (string name in foldersArr)
                skinFilterSelector.Items.Add(name);
        }
        else
        {
            skinFilterSelector.Items.Add("All");
            skinFilterSelector.Text = "All";
        }
        if (!IsSavedValueEmpty(ValueNames.selectedSkinFilter))
            skinFilterSelector.Text = GetValue(ValueNames.selectedSkinFilter);
        else
            skinFilterSelector.Text = "All";
        Controls.Add(skinFilterSelector);
    }

    private void SetupButtons()
    {
        deleteSkinSelectorButton = new System.Windows.Forms.Button()
        {
            Top = 33,
            Font = mainFont,
            Width = 61,
            Left = 156,
            Text = "Delete",
        };

        showFilteredSkinsButton = new System.Windows.Forms.Button()
        {
            Top = 33,
            Font = mainFont,
            //Height = 23,
            Width = 53,
            Left = 46,
            Text = "Show",
        };
        showFilteredSkinsButton.Click += new EventHandler(FindOsuSkins);
        Controls.Add(showFilteredSkinsButton);

        hideSelectedSkinFilterButton = new System.Windows.Forms.Button()
        {
            Top = 33,
            Font = mainFont,
            Width = 48,
            Left = 102,
            Text = "Hide",
        };
        hideSelectedSkinFilterButton.Click += new EventHandler(FindOsuSkins);
        Controls.Add(hideSelectedSkinFilterButton);

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
        changeOsuPathButton.Select();

        searchOsuSkinsButton = new System.Windows.Forms.Button()
        {
            Left = 390,
            Top = 3,
            Width = 90,
            Font = mainFont,
            Text = "Find Skins"
        };
        searchOsuSkinsButton.Click += new EventHandler(FindOsuSkins);
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
        deleteSkinButton.Click += new EventHandler(DeleteSelectedSkin_Click);
        Controls.Add(deleteSkinButton);

        openSkinFolderButton = new System.Windows.Forms.Button()
        {
            Left = 298,
            Top = 737,
            Width = 139,
            Font = mainFont,
            Text = "Open Skin Folder",
        };
        openSkinFolderButton.Click += new EventHandler(OpenSkinFolder_Click);
        Controls.Add(openSkinFolderButton);

        renameSkinButton = new System.Windows.Forms.Button()
        {
            Left = 440,
            Top = 737,
            Width = 107,
            Font = mainFont,
            Text = "Rename Skin",
        };
        renameSkinButton.Click += new EventHandler(RenameSkin_Click);
        Controls.Add(renameSkinButton);
    }

    private void SetupCheckBoxes()
    {
        hiddenSkinFiltersText = new CheckBox()
        {
            Top = 33,
            Font = mainFont,
            Left = 136,
            Text = "a",
            Name = ValueNames.hiddenSkinFilter.ToString(),
            TextAlign = ContentAlignment.MiddleLeft,
        };
        hiddenSkinFiltersText.TextChanged += new EventHandler((sender, ev) =>
        {
            int textWidth = TextRenderer.MeasureText(hiddenSkinFiltersText.Text, mainFont).Width;
            if (textWidth == 0)
            {
                if (Controls.Contains(hiddenSkinFiltersText))
                    Controls.Remove(hiddenSkinFiltersText);
                deleteSkinSelectorButton.Left = 153;
                searchSkinBox.Left = 218;
            }
            else
            {
                if (!Controls.Contains(hiddenSkinFiltersText))
                    Controls.Add(hiddenSkinFiltersText);

                deleteSkinSelectorButton.Left = textWidth + 152;
                hiddenSkinFiltersText.Width = textWidth + 30;
                searchSkinBox.Left = textWidth + 218;
            }
        });
        Controls.Add(deleteSkinSelectorButton);

        if (!IsSavedValueEmpty(ValueNames.hiddenSkinFilter))
            hiddenSkinFiltersText.Text = hiddenSkinFiltersText.Text = GetValue(ValueNames.hiddenSkinFilter).Replace(",", ", ");
        else
            hiddenSkinFiltersText.Text = "";

        deleteSkinSelectorButton.Click += new EventHandler((sender, e) =>
        {
            if (skinFilterSelector.Items.Contains(skinFilterSelector.Text))
            {
                string[] origValue = GetValue(ValueNames.skinFilters).Split(',');
                string fixedValue = "";

                foreach (string name in origValue)
                {
                    if (skinFilterSelector.Text != name)
                        fixedValue += name + ",";
                }
                //ChangeRegValue(ValueNames.skinFilters, fixedValue.Remove(fixedValue.Length-1));

                skinFilterSelector.Items.Remove(skinFilterSelector.Text);
            }
            else if (skinFilterSelector.Text == "All")
                DebugLog("You can't delete the \"All\" prefix silly.", true);

            skinFilterSelector.Text = "All";
            //ChangeRegValue(ValueNames.selectedSkinFilter, "All");
        });

        /* writeCurrSkinBox = new CheckBox()
        {
            Height = 25,
            Width = 100,
            Left = 483,
            Top = 3,
            Text = "Write current skin to .txt file",
            Name = ValueNames.writeCurrSkin.ToString(),
            TextAlign = ContentAlignment.MiddleLeft,
        };
        if (!IsSavedValueEmpty(ValueNames.writeCurrSkin))
            writeCurrSkinBox.CheckState = GetCheckState(ValueNames.writeCurrSkin);
        else
            writeCurrSkinBox.Checked = false;

        writeCurrSkinBox.CheckedChanged += new EventHandler(OnClick);
        Controls.Add(writeCurrSkinBox); */

        disableSkinChangesBox = new CheckBox()
        {
            Height = 25,
            Width = 200,
            Left = 483,
            Top = 3,
            Text = "Disable skin changes",
            Name = ValueNames.disableSkinChanges.ToString(),
            TextAlign = ContentAlignment.MiddleLeft,
        };
        if (!IsSavedValueEmpty(ValueNames.disableSkinChanges))
            disableSkinChangesBox.CheckState = GetCheckState(ValueNames.disableSkinChanges);
        else
            disableSkinChangesBox.Checked = false;

        disableSkinChangesBox.CheckedChanged += new EventHandler(OnClick);
        Controls.Add(disableSkinChangesBox);
    }

    private void SetupSkinChangeCheckBoxes()
    {
        CheckBox workingCheckBox;
        int indexInControls;
        CheckState defaultCheckState = CheckState.Indeterminate;

        for (int i = 0; i <= 6; i++)
        {
            if (i == 7) //skipping instafade for now
                continue;

            workingCheckBox = new CheckBox();
            workingCheckBox.Height = 25;
            workingCheckBox.Width = 297;
            workingCheckBox.Font = mainFont;
            workingCheckBox.Left = 503;
            workingCheckBox.Top = 60 + (i * 20);
            workingCheckBox.TextAlign = ContentAlignment.MiddleLeft;
            workingCheckBox.CheckStateChanged += new EventHandler(OnClick);
            workingCheckBox.ThreeState = true;
            Controls.Add(workingCheckBox);
            indexInControls = Controls.IndexOf(workingCheckBox);

            switch (i)
            {
                case 0:
                    Controls[indexInControls].Name = ValueNames.showComboBursts.ToString();
                    Controls[indexInControls].Text = "Combo Bursts";
                    showComboBurstsBox = (CheckBox)Controls[indexInControls];
                    break;
                case 1:
                    Controls[indexInControls].Name = ValueNames.showHitLighting.ToString();
                    Controls[indexInControls].Text = "Hit Lighting";
                    showHitLightingBox = (CheckBox)Controls[indexInControls];
                    break;
                case 2:
                    Controls[indexInControls].Name = ValueNames.showSkinNumbers.ToString();
                    Controls[indexInControls].Text = "Hitcircle Numbers";
                    showSkinNumbersBox = (CheckBox)Controls[indexInControls];
                    break;
                case 3:
                    Controls[indexInControls].Name = ValueNames.disableCursorTrail.ToString();
                    Controls[indexInControls].Text = "Cursor Trail";
                    disableCursorTrailBox = (CheckBox)Controls[indexInControls];
                    break;
                case 4:
                    Controls[indexInControls].Name = ValueNames.showSliderEnds.ToString();
                    Controls[indexInControls].Text = "Slider Ends";
                    showSliderEndsBox = (CheckBox)Controls[indexInControls];
                    break;
                case 5:
                    Controls[indexInControls].Name = ValueNames.showHitCircles.ToString();
                    Controls[indexInControls].Text = "Show Hitcircles";
                    defaultCheckState = CheckState.Checked;
                    showHitCirclesBox = (CheckBox)Controls[indexInControls];
                    break;
                case 6:
                    Controls[indexInControls].Name = ValueNames.expandingCursor.ToString();
                    Controls[indexInControls].Text = "Expanding Cursor";
                    expandingCursorBox = (CheckBox)Controls[indexInControls];
                    break;
                case 7:
                    Controls[indexInControls].Name = ValueNames.makeInstafade.ToString();
                    Controls[indexInControls].Text = "Make Instafade";
                    makeInstafadeBox = (CheckBox)Controls[indexInControls];
                    break;
                default:
                    DebugLog("Error building CheckBoxes. i = " + i.ToString(), true);
                    return;
            }
            string regValue = GetValue(Controls[indexInControls].Name);
            switch (regValue)
            {
                case "True":
                    ((CheckBox)Controls[indexInControls]).Checked = true;
                    break;
                case "False":
                    ((CheckBox)Controls[indexInControls]).Checked = false;
                    break;
                case "Indeterminate":
                    ((CheckBox)Controls[indexInControls]).CheckState = CheckState.Indeterminate;
                    break;
                case "Checked":
                    ((CheckBox)Controls[indexInControls]).CheckState = CheckState.Checked;
                    break;
                case "Unchecked":
                    ((CheckBox)Controls[indexInControls]).CheckState = CheckState.Unchecked;
                    break;
                default:
                    ((CheckBox)Controls[indexInControls]).CheckState = defaultCheckState;
                    break;
            }
            DebugLog($"CheckBox Name is: {Controls[indexInControls].Name} | Text is: {Controls[indexInControls].Text} | Checked: {((CheckBox)Controls[indexInControls]).Checked.ToString()}", false);
        }
    }

    //MISC Skin handling
    private void UserSearchSkins_Click(object sender, EventArgs e)
    {
        FindOsuSkins(sender, e);
        //searchSkinBox.Text = searchSkinBox.Text += (char)keyEvent.KeyCode;

        /* if (keyEvent.KeyCode == Keys.Back)
            FindOsuSkins(sender, new EventArgs());
        osuSkinsListBox.BeginUpdate();

        List<int> remove = new List<int>();

        string searchFor = searchSkinBox.Text;
        for (int i = 0; i < osuSkinsListBox.Items.Count; i++)
        {
            if (!osuSkinsListBox.Items[i].ToString().Contains(searchFor, StringComparison.OrdinalIgnoreCase))
            {
                remove.Add(i);
            }
        }

        remove.Reverse();
        foreach (int i in remove)
        {
            osuSkinsListBox.Items.RemoveAt(i);
            osuSkinsPathList.RemoveAt(i);
        }

        if (osuSkinsListBox.Items.Count == 1)
            osuSkinsListBox.SetSelected(0, true);

        osuSkinsListBox.EndUpdate(); */
    }

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
            osuFolderPathBox.Text = directorySelector.SelectedPath;
            osuPath = directorySelector.SelectedPath;
            DebugLog("osuPath set to: " + osuPath, false);
            OnClick(sender, e);
        }
        directorySelector.Dispose();
    }

    private void FindOsuSkins(object sender, EventArgs e)
    {
        if (sender != searchSkinBox)
            EnableAllControls(false);

        if (sender == hideSelectedSkinFilterButton)
        {
            DebugLog("hideSelectedSkinFilterButton called SearchOsuSkins()", false);
            //Adds selected prefix to hidden list
            if (skinFilterSelector.Text != "All" && !hiddenSkinFiltersText.Text.Contains(skinFilterSelector.Text))
            {
                if (hiddenSkinFiltersText.Text == "")
                    hiddenSkinFiltersText.Text = skinFilterSelector.Text;
                else
                    hiddenSkinFiltersText.Text += ", " + skinFilterSelector.Text;
            }
            AddToSkinFilters();
            skinFilterSelector.Text = "All";
        }
        else if (sender == showFilteredSkinsButton)
        {
            DebugLog("showFilteredSkinsButton called SearchOsuSkins()", false);
            hiddenSkinFiltersText.Text = "";
            if (skinFilterSelector.Text == ",")
            {
                skinFilterSelector.Text = "All";
                DebugLog("You cannot use \",\" as a prefix", true);
            }
            AddToSkinFilters();

            /* if (!skinFilterSelector.Items.Contains(skinFilterSelector.Text))
                skinFilterSelector.Items.Add(skinFilterSelector.Text); */
        }

        /* List<string> hiddenSkinFiltersList = new List<string>();
        if (hiddenSkinFiltersText.Text.Contains(','))
            hiddenSkinFiltersList = hiddenSkinFiltersText.Text.Replace(" ", "").Split(',').ToList();
        else if (!String.IsNullOrWhiteSpace(hiddenSkinFiltersText.Text))
            hiddenSkinFiltersList.Add(hiddenSkinFiltersText.Text); */

        osuSkinsListBox.BeginUpdate();
        osuSkinsListBox.ClearSelected();
        osuSkinsListBox.Items.Clear();
        osuSkinsList.Clear();
        DirectoryInfo skinFolderDI = new DirectoryInfo(Path.Combine(osuPath, "skins"));
        DirectoryInfo[] osuSkins = skinFolderDI.GetDirectories();

        foreach (DirectoryInfo workingSkin in osuSkins)
        {
            AddSkin(new Skin(workingSkin.FullName));
        }
        osuSkinsListBox.EndUpdate();
        if (!Controls.Contains(osuSkinsListBox))
            Controls.Add(osuSkinsListBox);

        //if skin that was last selected is shown, select it
        string lastSkinName = GetValue(ValueNames.selectedSkin);
        if (osuSkinsListBox.Items.Contains(lastSkinName))
        {
            osuSkinsListBox.SetSelected(osuSkinsListBox.Items.IndexOf(lastSkinName), true);
            DebugLog("Previous selected skin name: " + lastSkinName, false);
        }

        if (sender != searchSkinBox)
            EnableAllControls(true);
    }

    private void AddSkin(Skin skin)
    {
        bool shouldAdd = false;

        if (skin.name != managerFolderName && skin.name != "Deleted Skins")
        {
            if (!hiddenSkinFiltersText.Text.Contains(skin.name.First<char>().ToString())) //true if skin does not have prefix that is supposed to be hidden
            {
                if (skinFilterSelector.Text != "All" && skin.name.IndexOf(skinFilterSelector.Text) == 0)
                    shouldAdd = true;
                else if(skinFilterSelector.Text == "All")
                    shouldAdd = true;
            }
        }

        if (shouldAdd && skin.name.Contains(searchSkinBox.Text, StringComparison.OrdinalIgnoreCase))
        {
            osuSkinsList.Add(skin);
            osuSkinsListBox.Items.Add(skin.name);
            if (spamLogs)
                DebugLog($"Adding {skin.name} to the skin list | {skin.path}", false);
        }
    }

    private void AddToSkinFilters()
    {
        if (skinFilterSelector.Text == ",")
        {
            DebugLog("You cannot add \",\" as a prefix", true);
        }
        else if (skinFilterSelector.Text != "All" && !skinFilterSelector.Items.Contains(skinFilterSelector.Text))
        {
            skinFilterSelector.Items.Add(skinFilterSelector.Text);
            //ChangeRegValue(ValueNames.skinFilters, (String.IsNullOrWhiteSpace(GetValue(ValueNames.skinFilters)) ? "" : GetValue(ValueNames.skinFilters) + ",") + skinFilterSelector.Text);
        }
    }

    private void OpenSkinFolder_Click(object sender, EventArgs e)
    {
        if (osuSkinsListBox.SelectedItem == null)
        {
            DebugLog("Select skin before trying to open its folder", true);
            return;
        }
        DebugLog("Attempting open skin folder: " + Path.Combine(osuPath, "skins", osuSkinsListBox.SelectedItem.ToString()), false);
        Process.Start("explorer.exe", Path.Combine(osuPath, "skins", osuSkinsListBox.SelectedItem.ToString()));
    }

    private void DeleteSelectedSkin_Click(object sender, EventArgs e)
    {
        //PopUp confirm = new PopUp();
        if (!PopUp.Conformation("Are you sure you want to delete this skin?"))
            return;

        EnableAllControls(false);
        if (osuSkinsListBox.SelectedItem == null)
        {
            DebugLog("Find skins first. Error occurred when trying to delete skin", true);
            return;
        }
        string currentSkinPath = osuSkinsList[osuSkinsListBox.SelectedIndex].path;
        Directory.Move(currentSkinPath, Path.Combine(osuPath, "skins", "Deleted Skins", osuSkinsListBox.SelectedItem.ToString()));
        DebugLog($"Moving {currentSkinPath} to {Path.Combine(osuPath, "skins", "Deleted Skins", osuSkinsListBox.SelectedItem.ToString())}", false);
        osuSkinsList.RemoveAt(osuSkinsListBox.SelectedIndex);
        osuSkinsListBox.Items.RemoveAt(osuSkinsListBox.SelectedIndex);
        EnableAllControls(true);
    }

    //Skin Switching
    private void ChangeToSelectedSkin(object sender, EventArgs e)
    {
        DebugLog("[STARTING TO CHANGE SKIN]", false);
        if (osuSkinsListBox.SelectedItem == null)
        {
            DebugLog("Please select skin before trying to change to it.", true);
            return;
        }
        EnableAllControls(false);

        DeleteSkinElementsInHelperSkin();
        string workingSkinPath;
        if (osuSkinsListBox.SelectedItems.Count == 1) //false if multiple skins are selected
            workingSkinPath = osuSkinsList[osuSkinsListBox.SelectedIndex].path;
        else
        {
            Random r = new Random();
            int randomSkinIndex = osuSkinsListBox.Items.IndexOf(osuSkinsListBox.SelectedItems[r.Next(0, osuSkinsListBox.SelectedItems.Count)]);
            workingSkinPath = osuSkinsList[randomSkinIndex].path;

            osuSkinsListBox.ClearSelected();
            osuSkinsListBox.SetSelected(randomSkinIndex, true);
            DebugLog($"Changing to random skin from selected | Selected: {osuSkinsListBox.SelectedItem.ToString()}", false);
        }
        //ChangeRegValue(ValueNames.selectedSkinName, osuSkinsListBox.SelectedItem.ToString());

        /* if (writeCurrSkinBox.Checked)
            UpdateSkinTextFile(workingSkinPath.Replace(Path.Combine(osuPath, "skins") + "\\", "")); */

        DirectoryInfo workingSkinPathDi = new DirectoryInfo(workingSkinPath);

        foreach (FileInfo currentFile in workingSkinPathDi.GetFiles())
        {
            currentFile.CopyTo(Path.Combine(helperSkin.path, currentFile.Name), true);
            if (spamLogs)
                DebugLog($"Copying \"{currentFile.FullName}\" to \"{Path.Combine(helperSkin.path, currentFile.Name)}\"", false);
        }
        RecursiveSkinFolderMove(workingSkinPath, "\\");

        DebugLog("[FINISHED CHANGING TO SKIN]", false);

        if (!disableSkinChangesBox.Checked)
        {
            DebugLog("[STARTING EDITING SKIN]", false);
            ShowHitCircleNumbers(showSkinNumbersBox.CheckState);
            ShowSliderEnds(showSliderEndsBox.CheckState);
            DisableCursorTrail(disableCursorTrailBox.CheckState);
            ShowComboBursts(showComboBurstsBox.CheckState);
            ShowHitLighting(showHitLightingBox.CheckState);
            ShowHitCircles(showHitCirclesBox.CheckState);
            ChangeExpandingCursor(expandingCursorBox.CheckState);
            //MakeInstafade(makeInstafadeBox.CheckState);
            DebugLog("[FINISHED EDITING SKIN]", false);
        }
        EnableAllControls(true);
    }

    private void RecursiveSkinFolderMove(string skinPath, string prevFolder)
    {
        DirectoryInfo rootFolder = new DirectoryInfo(skinPath + prevFolder);

        foreach (DirectoryInfo folder in rootFolder.GetDirectories())
        {
            Directory.CreateDirectory(helperSkin + prevFolder + "\\" + folder.Name);
            DirectoryInfo subFolder = new DirectoryInfo(skinPath + prevFolder + "\\" + folder.Name);

            if (subFolder.GetDirectories().Length != 0)
            {
                RecursiveSkinFolderMove(skinPath, prevFolder + "\\" + folder.Name);
            }

            foreach (FileInfo file in subFolder.GetFiles())
            {
                file.CopyTo(helperSkin + prevFolder + "\\" + folder.Name + "\\" + file.Name, true);
                if (spamLogs)
                    DebugLog($"Copying \"{file.FullName}\" to \"{Path.Combine(helperSkin + prevFolder, folder.Name, file.Name)}\"", false);
            }
        }
    }

    private void RandomSkin_Click(object sender, EventArgs e)
    {
        EnableAllControls(false);
        Random r = new Random();
        osuSkinsListBox.ClearSelected();
        osuSkinsListBox.SetSelected(r.Next(0, osuSkinsListBox.Items.Count), true);
        DebugLog($"Changed to random skin. Picked \"{osuSkinsListBox.SelectedItem.ToString()}\"", false);
        ChangeToSelectedSkin(sender, e);
    }

    //Skin editing
    private void ChangeExpandingCursor(CheckState expand)
    {
        DebugLog($"ChangeExpandingCursor({expand.ToString()}) called", false);

        if (expand == CheckState.Indeterminate)
        {
            RevertSkinIni("CursorExpand:");
            return;
        }

        EnableAllControls(false);
        if (expand == CheckState.Checked)
            EditSkinIni("CursorExpand:", "CursorExpand: 1", "[General]");
        else
            EditSkinIni("CursorExpand:", "CursorExpand: 0", "[General]");
        EnableAllControls(true);
    }

    /*     private void MakeInstafade(CheckState instafade)
        {
            return;
            if (instafade == CheckState.Checked)
            {
                bool hitCircleOverlayAboveNumber = true;
                bool At2X;
                string searchINIForBool = SearchSkinINI("HitCircleOverlayAbove");
                if (searchINIForBool != null && searchINIForBool.Contains('0'))
                    hitCircleOverlayAboveNumber = false;

                string hitcircleDefault = SearchSkinINI("HitCirclePrefix:").Replace("HitCirclePrefix:", "").Replace(" ", "").Replace("/", "\\");
                if (String.IsNullOrWhiteSpace(hitcircleDefault))
                    hitcircleDefault = "default";

                if ((File.Exists(Path.Combine(helperSkinPath, "hitcircle@2x.png")) || File.Exists(Path.Combine(helperSkinPath, "hitcircleoverlay@2x.png"))) &&
                File.Exists(Path.Combine(helperSkinPath, hitcircleDefault + "-1@2x.png")))
                {
                    At2X = true;
                }
                else if ((File.Exists(Path.Combine(helperSkinPath, "hitcircle.png")) || File.Exists(Path.Combine(helperSkinPath, "hitcircleoverlay.png"))) &&
                File.Exists(Path.Combine(helperSkinPath, hitcircleDefault + "-1.png")))
                {
                    At2X = false;
                }
                else
                {
                    DebugLog($"Pair of hd and sd not found | Looking for prefix of {hitcircleDefault}", true);
                    return;
                }


                Image hitcircleImage = (File.Exists(Path.Combine(helperSkinPath, "hitcircle@2x.png")) ? Image.FromFile(Path.Combine(helperSkinPath, "hitcircle@2x.png")) : new Bitmap(1, 1));
                Image hitcircleOverlayImage = (File.Exists(Path.Combine(helperSkinPath, "hitcircleoverlay@2x.png")) ? Image.FromFile(Path.Combine(helperSkinPath, "hitcircleoverlay@2x.png")) : new Bitmap(1, 1));
                Graphics hitcircleGraphics = Graphics.FromImage(hitcircleImage);
                int imageWidth = hitcircleImage.Width / 2;
                int imageHeight = hitcircleImage.Height / 2;

                if (!hitCircleOverlayAboveNumber)
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
                    Image textImage = (File.Exists(Path.Combine(helperSkinPath, hitcircleDefault + "-" + i + "@2x.png")) ?
                        Image.FromFile(Path.Combine(helperSkinPath, hitcircleDefault + "-" + i + "@2x.png")) :
                        Image.FromFile(Path.Combine(helperSkinPath, hitcircleDefault + "-" + i + ".png")));

                    x = imageWidth - (textImage.Width / 2);
                    y = imageHeight - (textImage.Height / 2);
                    using (Image hitcircleImageTemp = (Image)hitcircleImage.Clone())
                    {
                        Graphics tempGraphic = Graphics.FromImage(hitcircleImageTemp);

                        tempGraphic.DrawImage(textImage, x, y);
                        if (hitCircleOverlayAboveNumber)
                            tempGraphic.DrawImage(hitcircleOverlayImage, 0, 0);

                        tempGraphic.Save();
                        //DebugLog(Path.Combine(mainSkinPath, hitcircleDefault + "-"+i+(At2X? "@2x": "")+".png"), true);
                        textImage.Dispose();
                        tempGraphic.Dispose();
                        //File.Delete(Path.Combine(mainSkinPath, hitcircleDefault + "-"+i+(At2X? "@2x": "@2x")+".png"));
                        string path = Path.Combine(helperSkinPath, hitcircleDefault + "-" + i + (At2X ? "@2x" : "") + ".png");
                        hitcircleImageTemp.Save(path.Replace(".png", ".temp"));
                        //Process.Start("cmd.exe", $"/c ffmpeg -i {path.Replace(".png", ".temp")} -vf scale=320:320 -y {path}");
                        //File.Delete(path.Replace(".png", ".temp"));
                    }
                }
                hitcircleImage.Dispose();
                hitcircleOverlayImage.Dispose();
                hitcircleGraphics.Dispose();
                EditSkinIni("HitCircleOverlap:", "HitCircleOverlap: " + imageWidth * 2, "[Fonts]");
                ShowHitCircles(CheckState.Unchecked);
            }
        } */

    private void ShowHitCircles(CheckState showState)
    {
        DebugLog($"ShowHitCircles({showState}) called", false);

        if (showState == CheckState.Indeterminate)
            showState = CheckState.Checked;

        bool show = (showState == CheckState.Checked);

        EnableAllControls(false);
        string[] fileNames =
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
        if (show)
        {
            CopyFilesToHelperSkin(fileNames);
        }
        else
        {
            Bitmap emptyImage = new Bitmap(1, 1);
            foreach (string curName in fileNames)
            {
                string helperSkinFilePath = Path.Combine(helperSkin.path, curName);
                emptyImage.Save(helperSkinFilePath);
                if (spamLogs)
                    DebugLog($"Copying empty image to {helperSkinFilePath}", false);
            }

            emptyImage.Dispose();
        }
        EnableAllControls(true);
    }

    private void ShowHitLighting(CheckState showState)
    {
        DebugLog($"ShowHitLighting({showState}) called", false);

        EnableAllControls(false);
        string[] fileNames =
        {
            "lighting.png",
            "lighting@2x.png",
        };

        if (showState == CheckState.Indeterminate)
        {
            CopyFilesToHelperSkin(fileNames);
        }
        else if (showState == CheckState.Checked)
        {
            string currentSkinFilePath;
            string helperSkinFilePath;
            foreach (string curFileName in fileNames)
            {
                currentSkinFilePath = Path.Combine(GetCurrentSkin().path, curFileName);
                helperSkinFilePath = Path.Combine(helperSkin.path, curFileName);

                if (File.Exists(currentSkinFilePath))
                {
                    Image thisImg = Image.FromFile(currentSkinFilePath);
                    if (thisImg.Height > 100)
                    {
                        File.Copy(currentSkinFilePath, helperSkinFilePath, true);
                        thisImg.Dispose();
                        if (spamLogs)
                            DebugLog($"Copying {currentSkinFilePath} to {helperSkinFilePath}", false);

                        continue;
                    }
                    thisImg.Dispose();
                    File.Delete(helperSkinFilePath);
                    if (spamLogs)
                        DebugLog($"Deleting {helperSkinFilePath}", false);
                }
                else if (File.Exists(helperSkinFilePath))
                {
                    File.Delete(helperSkinFilePath);
                    if (spamLogs)
                        DebugLog($"Deleting {helperSkinFilePath}", false);
                }
            }
        }
        else
        {
            Image emptyImage = new Bitmap(1, 1);
            string helperSkinFilePath;
            foreach (string name in fileNames)
            {
                helperSkinFilePath = Path.Combine(helperSkin.path, name);

                emptyImage.Save(helperSkinFilePath);
                if (spamLogs)
                    DebugLog($"Saving empty image to {helperSkinFilePath}", false);
            }
            emptyImage.Dispose();
        }
        EnableAllControls(true);
    }

    private void ShowComboBursts(CheckState showState)
    {
        DebugLog($"ShowComboBursts({showState}) called", false);

        if (showState == CheckState.Indeterminate)
            showState = CheckState.Unchecked;

        bool show = (showState == CheckState.Checked);

        EnableAllControls(false);
        Bitmap emptyImage = new Bitmap(1, 1);
        string[] fileNames =
        {
            "comboburst",
            "comboburst@2x",
            "comboburst-fruits",
            "comboburst-fruits@2x",
            "comboburst-mania",
            "comboburst-mania@2x",
        };

        foreach (string name in fileNames)
        {
            if (!File.Exists(Path.Combine(helperSkin.path, name + ".png")))
            {
                emptyImage.Save(Path.Combine(helperSkin.path, name + ".png"));
                if (spamLogs)
                    DebugLog($"Saving empty image to {Path.Combine(helperSkin.path, name + ".png")}", false);
            }
        }

        //incase there are multiple combobursts
        DirectoryInfo di = new DirectoryInfo(GetCurrentSkin().path);
        foreach (FileInfo file in di.GetFiles())
        {
            foreach (string name in fileNames)
                if (file.Name.Contains(name))
                {
                    if (show)
                        File.Copy(file.FullName, Path.Combine(helperSkin.path, file.Name), true);
                    else
                        emptyImage.Save(Path.Combine(helperSkin.path, file.Name));

                    if (spamLogs)
                        DebugLog($"Saving empty image to {Path.Combine(helperSkin.path, file.Name)}", false);
                }
        }
        emptyImage.Dispose();
        EnableAllControls(true);
    }

    private void DisableCursorTrail(CheckState showState)
    {
        DebugLog($"DisableCursorTrail({showState}) called", false);

        if (showState == CheckState.Indeterminate)
            return;

        bool show = (showState == CheckState.Checked);

        EnableAllControls(false);
        List<string> names = new List<string>()
        {
            "cursortrail@2x.png",
            "cursortrail.png",
        };

        if (show)
        {
            foreach (string name in names)
                if (File.Exists(Path.Combine(GetCurrentSkin().path, name)))
                {
                    File.Copy(Path.Combine(GetCurrentSkin().path, name), Path.Combine(helperSkin.path, name), true);
                    if (spamLogs)
                        DebugLog($"Copying {Path.Combine(GetCurrentSkin().path, name)} to {Path.Combine(helperSkin.path, name)}", false);
                }

        }
        else
        {
            Bitmap emptyImage = new Bitmap(1, 1);
            foreach (string name in names)
            {
                emptyImage.Save(Path.Combine(helperSkin.path, name));
                if (spamLogs)
                    DebugLog($"Saving empty image to {Path.Combine(helperSkin.path, name)}", false);
            }
            emptyImage.Dispose();
        }
        EnableAllControls(true);
    }

    private void ShowSliderEnds(CheckState showState)
    {
        DebugLog($"ShowSliderEnds({showState}) called", false);

        if (showState == CheckState.Indeterminate)
            return;

        bool show = (showState == CheckState.Checked);

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

        if (show)//Showing ends
        {
            foreach (string fileName in sliderEnds)
            {
                if (skipAt2X == fileName)
                {
                    skipAt2X = "";
                    continue;
                }

                using (Image image = (File.Exists(Path.Combine(GetCurrentSkin().path, fileName)) ? Image.FromFile(Path.Combine(GetCurrentSkin().path, fileName)) : null))
                {
                    if (File.Exists(Path.Combine(GetCurrentSkin().path, fileName)) && (image == null ? true : image.Size.Height < 100))
                    {
                        File.Copy(Path.Combine(GetCurrentSkin().path, fileName), Path.Combine(helperSkin.path, fileName), true);
                        if (spamLogs)
                            DebugLog($"Copying {Path.Combine(GetCurrentSkin().path, fileName)} to {Path.Combine(helperSkin.path, fileName)}", false);
                    }
                    else if (File.Exists(Path.Combine(GetCurrentSkin().path, fileName.Replace("end", "start"))))
                    {
                        File.Copy(Path.Combine(GetCurrentSkin().path, fileName.Replace("sliderendcircle", "sliderstartcircle")), Path.Combine(helperSkin.path, fileName), true);
                        if (spamLogs)
                            DebugLog($"Copying {Path.Combine(GetCurrentSkin().path, fileName.Replace("sliderendcircle", "sliderstartcircle"))} to {Path.Combine(helperSkin.path, fileName)}", false);
                        if (!File.Exists(Path.Combine(GetCurrentSkin().path, fileName.Replace(".png", "@2x.png"))))
                        {
                            skipAt2X = fileName.Replace(".png", "@2x.png");
                        }
                    }
                    else if (File.Exists(Path.Combine(GetCurrentSkin().path, fileName.Replace("sliderend", "hit"))))
                    {
                        File.Copy(Path.Combine(GetCurrentSkin().path, fileName.Replace("sliderend", "hit")), Path.Combine(helperSkin.path, fileName), true);
                        if (spamLogs)
                            DebugLog($"Copying {Path.Combine(GetCurrentSkin().path, fileName.Replace("sliderend", "hit"))} to {Path.Combine(helperSkin.path, fileName)}", false);
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
            foreach (string fileName in sliderEnds)
                emptyImage.Save(Path.Combine(helperSkin.path, fileName));
        }
        emptyImage.Dispose();
        sliderImage.Dispose();
        EnableAllControls(true);
    }

    private void DeleteSkinElementsInHelperSkin()
    {
        DirectoryInfo rootFolder = new DirectoryInfo(helperSkin.path);

        foreach (FileInfo file in rootFolder.GetFiles())
        {
            if (spamLogs)
                DebugLog($"Deleting {file.FullName}", false);
            file.Delete();
        }


        foreach (DirectoryInfo folder in rootFolder.GetDirectories())
        {
            if (spamLogs)
                DebugLog($"Deleting {folder.FullName} (directory)", false);
            Directory.Delete(folder.FullName, true);
        }
    }

    private void ShowHitCircleNumbers(CheckState showState)
    {
        DebugLog($"ShowHitCircleNumbers({showState}) called", false);

        if (showState == CheckState.Indeterminate)
            return;

        bool show = (showState == CheckState.Checked);

        EnableAllControls(false);
        if (show) //show skin numbers
        {
            File.Copy(Path.Combine(GetCurrentSkin().path, "skin.ini"), Path.Combine(helperSkin.path, "skin.ini"), true);
            DebugLog($"Copying {Path.Combine(GetCurrentSkin().path, "skin.ini")} to {Path.Combine(helperSkin.path, "skin.ini")}", false);
        }
        else //hide skin numbers
            EditSkinIni("HitCirclePrefix:", "HitCirclePrefix: 727", "[Fonts]");
        EnableAllControls(true);
    }

    private void CopyFilesToHelperSkin(string[] fileNames)
    {
        foreach (string curName in fileNames)
        {
            string currentSkinFilePath = Path.Combine(GetCurrentSkin().path, curName);
            string helperSkinFilePath = Path.Combine(helperSkin.path, curName);
            if (File.Exists(currentSkinFilePath))
            {
                File.Copy(currentSkinFilePath, helperSkinFilePath, true);
                if (spamLogs)
                    DebugLog($"Copying {currentSkinFilePath} to {helperSkinFilePath}", false);
            }
        }
    }

    //Ini things
    private void EditSkinIni(string searchFor, string replaceWith, string fallBackSearch)
    {
        DebugLog($"Attempting to search for {searchFor}, replace it with {replaceWith}, with a fallback of {fallBackSearch}. In the skin.ini", false);
        File.Copy(helperSkin.iniPath, helperSkin.iniPath.Replace("skin.ini", "skin.ini.temp"));
        StreamReader reader = new StreamReader(helperSkin.iniPath.Replace("skin.ini", "skin.ini.temp"));
        StreamWriter writer = new StreamWriter(helperSkin.iniPath);
        string currLine;
        bool lineFound = false;

        while ((currLine = reader.ReadLine()) != null)
        {
            if (currLine.Contains(searchFor))
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

        if (!lineFound)
        {
            StreamWriter writerNew = new StreamWriter(helperSkin.iniPath);
            StreamReader readerNew = new StreamReader(helperSkin.iniPath.Replace("skin.ini", "skin.ini.temp"));

            while ((currLine = readerNew.ReadLine()) != null)
            {
                if (currLine.Contains(fallBackSearch))
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

        File.Delete(helperSkin.iniPath.Replace("skin.ini", "skin.ini.temp"));
    }

    private void RevertSkinIni(string searchFor)
    {
        DebugLog($"Attempting to restore {searchFor}. In the skin.ini", false);
        StreamReader origINIReader = new StreamReader(GetCurrentSkin().iniPath);
        string currLine;

        while ((currLine = origINIReader.ReadLine()) != null)
        {
            if (currLine.ToLower().Contains(searchFor.ToLower()))
                break;
        }

        origINIReader.Close();
        origINIReader.Dispose();

        if (currLine == null)
            EditSkinIni(searchFor, "", "really long word that should not be there");
        else
            EditSkinIni(searchFor, currLine, "really long word that should not be there");
    }

    private string SearchSkinINI(string searchFor)
    {
        DebugLog($"Searching skin.ini for {searchFor}", false);
        StreamReader reader = new StreamReader(helperSkin.iniPath);
        string currLine = null;
        while ((currLine = reader.ReadLine()) != null)
        {
            if (currLine.Contains(searchFor))
                break;
        }
        reader.Close();
        reader.Dispose();
        return currLine;
    }

    //Edit Saving Stuff
    private void OnClick(object sender, EventArgs e)
    {
        if (osuSkinsListBox.SelectedIndex == -1)
            return;
        else if (sender == showSkinNumbersBox)
            ShowHitCircleNumbers(showSkinNumbersBox.CheckState);
        else if (sender == showSliderEndsBox)
            ShowSliderEnds(showSliderEndsBox.CheckState);
        else if (sender == disableCursorTrailBox)
            DisableCursorTrail(disableCursorTrailBox.CheckState);
        else if (sender == showComboBurstsBox)
            ShowComboBursts(showComboBurstsBox.CheckState);
        else if (sender == showHitLightingBox)
            ShowHitLighting(showHitLightingBox.CheckState);
        else if (sender == showHitCirclesBox)
            ShowHitCircles(showHitCirclesBox.CheckState);
        else if (sender == expandingCursorBox)
            ChangeExpandingCursor(expandingCursorBox.CheckState);
        else if (sender == disableSkinChangesBox) { }
        /*         else if (sender == makeInstafadeBox)
                    MakeInstafade(makeInstafadeBox.CheckState); */
        else
        {
            DebugLog("Error with OnClick | " + sender.ToString(), true);
            return;
        }

        //ChangeRegValue(valName, val);
    }

    private string GetValue(ValueNames valName)
    {
        if (!IsSavedValueEmpty(valName))
            return loadedValues[valName];
        else
            return null;
        /* try
        {
            DebugLog($"Reg value of {valName.ToString()} is \"{Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", ((int)valName).ToString(), null).ToString()}\"", false);
            return Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", ((int)valName).ToString(), null).ToString();
        }
        catch
        {
            return null;
        } */
    }

    private string GetValue(string valName)
    {
        return GetValue((ValueNames)Enum.Parse(typeof(ValueNames), valName, true));
    }

    private bool IsSavedValueEmpty(ValueNames valName)
    {
        if (!loadedValues.Keys.Contains(valName))
            return true;

        return String.IsNullOrWhiteSpace(loadedValues[valName]);
    }

    private CheckState GetCheckState(ValueNames name)
    {
        if (!IsSavedValueEmpty(name))
            return (CheckState)Enum.Parse(typeof(CheckState), GetValue(name), true);
        else
            return CheckState.Unchecked;

    }

    private void LoadValues()
    {
        if (!File.Exists("settings.txt"))
            return;

        DebugLog("[Starting loading values from settings.txt]", false);
        StreamReader reader = new StreamReader("settings.txt");

        string curLine;

        while ((curLine = reader.ReadLine()) != null)
        {
            if (spamLogs)
                DebugLog("Read | " + curLine, false);

            string[] curLineArr = curLine.Split(',');
            if (curLineArr[0].Equals(ValueNames.skinFilters.ToString()))
            {
                curLineArr[1] = curLine.Replace(ValueNames.skinFilters.ToString() + ",", "");
            }
            loadedValues.Add((ValueNames)Enum.Parse(typeof(ValueNames), curLineArr[0], true), curLineArr[1]);
        }
        reader.Dispose();
        DebugLog("[Finished loading values from settings.txt]", false);
    }

    public void SaveEditedValues(object sender, EventArgs e)
    {
        //save the values of things
        if (!File.Exists("settings.txt"))
            File.Create("settings.txt");

        StreamWriter writer = new StreamWriter("settings.txt");

        foreach (Control currentObj in Controls)
        {
            if (currentObj.Name == ValueNames.skinFilters.ToString()) //is the skin filters
            {
                ComboBox skinFiltersBox = (ComboBox)currentObj;
                if (skinFiltersBox.Items.Count <= 1)
                    continue;

                string toWrite = skinFiltersBox.Name + ",";
                for (int i = 0; i < skinFiltersBox.Items.Count; i++)
                {
                    if (skinFiltersBox.Items[i].ToString() != "All")
                        toWrite += skinFiltersBox.Items[i] + (i == skinFiltersBox.Items.Count - 1 ? "" : ",");
                }
                writer.WriteLine(toWrite);
            }
            else
            {
                if (currentObj.Name == ValueNames.hiddenSkinFilter.ToString())
                    continue;
                else if (currentObj is CheckBox/*  &&
                    (((CheckBox)currentObj).CheckState != CheckState.Indeterminate) */)
                    writer.WriteLine(((CheckBox)currentObj).Name + "," + ((CheckBox)currentObj).CheckState.ToString());
                else if (currentObj.Name == ValueNames.selectedSkinFilter.ToString())
                {
                    if (currentObj.Text != "All" && !String.IsNullOrWhiteSpace(currentObj.Text))
                        writer.WriteLine(ValueNames.selectedSkinFilter.ToString() + "," + currentObj.Text);
                }
                else if (currentObj is TextBox)
                {
                    if (String.IsNullOrWhiteSpace(currentObj.Text))
                        continue;
                    if (currentObj.Name == ValueNames.osuPath.ToString())
                        continue;

                    writer.WriteLine(currentObj.Name + "," + currentObj.Text);
                }
            }
        }
        if (osuSkinsListBox.SelectedItems.Count == 1)
            writer.WriteLine(ValueNames.selectedSkin.ToString() + "," + osuSkinsListBox.SelectedItem.ToString());

        if (osuPath != Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "appdata", "Local", "osu!"))
            writer.WriteLine(ValueNames.osuPath.ToString() + "," + osuPath);

        writer.Dispose();
    }

    //MISC
    private void RenameSkin_Click(object sender, EventArgs e)
    {
        if (osuSkinsListBox.SelectedItems.Count != 1)
            return;

        string renameTo = osuSkinsListBox.SelectedItem.ToString();

        if (PopUp.InputBox("Rename", "Rename:", ref renameTo) == DialogResult.Cancel)
            return;

        while (renameTo.Contains('\\'))
            if (PopUp.InputBox("Cannot contain \"\\\"", "Rename:", ref renameTo) == DialogResult.Cancel)
                return;


        renameTo = osuSkinsList[osuSkinsListBox.SelectedIndex].path.Replace(osuSkinsListBox.SelectedItem.ToString(), renameTo);
        if (osuSkinsList[osuSkinsListBox.SelectedIndex].path != renameTo)
            Directory.Move(osuSkinsList[osuSkinsListBox.SelectedIndex].path, renameTo);

        FindOsuSkins(sender, e);

        osuSkinsListBox.ClearSelected();

        osuSkinsListBox.SetSelected(osuSkinsList.IndexOf(new Skin(renameTo)), true);
    }

    private void EnableAllControls(bool enable)
    {
        DebugLog($"EnableAllControls({enable}) called", false);
        foreach (Control obj in Controls)
        {
            if (obj != null)
                obj.Enabled = enable;
        }
    }

    private Skin GetCurrentSkin()
    {
        if (osuSkinsListBox.SelectedItems.Count == 1)
            return osuSkinsList[osuSkinsListBox.SelectedIndex];
        else if (String.IsNullOrWhiteSpace(GetValue(ValueNames.selectedSkin)) || osuSkinsListBox.SelectedItems.Count > 1)
            DebugLog("Multiple/no skins selected. Unable to get skin path.", true);
        else
            return new Skin(GetValue(ValueNames.selectedSkin), false);

        return null;
    }

    private void DebugLog(string log, bool alwaysLog)
    {
        if (alwaysLog || debugMode)
            Console.WriteLine(log);
    }

    private void DebugLog(int log, bool alwaysLog)
    {
        if (alwaysLog || debugMode)
            Console.WriteLine(log);
    }

    private void DebugLog(bool log, bool alwaysLog)
    {
        if (alwaysLog || debugMode)
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