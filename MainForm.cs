using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
namespace osu_helper
{
    public class MainForm : Form
    {
        private TextBox /* osuFolderPathBox, */ searchSkinBox;
        private Button changeOsuPathButton, /* searchOsuSkinsButton, */ deleteSkinButton, useSkinButton, randomSkinButton, openSkinFolderButton, showFilteredSkinsButton,
        hideSelectedSkinFilterButton, deleteSkinSelectorButton, renameSkinButton;

        private CheckBox showSkinNumbersBox, showSliderEndsBox, disableSkinChangesBox, disableCursorTrailBox, showComboBurstsBox, showHitLightingBox, hiddenSkinFiltersText, showHitCirclesBox,
        makeInstafadeBox, expandingCursorBox;

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private ToolTip toolTip;
        private ComboBox skinFilterSelector;
        private Font mainFont, searchBoxFont;
        private ListBox osuSkinsListBox;
        private List<UserSkin> osuSkinsList = new List<UserSkin>();
        public static HelperSkin helperSkin { get; private set; }
        public enum ValueName
        {
            disableCursorTrail,
            osuPath,
            selectedSkinFilter,
            showHitCircleNumbers,
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
            defaultValue,
        };
        Dictionary<ValueName, string> loadedValues = new Dictionary<ValueName, string>();

        #region Setup

        /// <summary>
        /// Pops up box asking user to change the osu! path
        /// </summary>
        public void ChangeOsuPath()
        {
            /* osuFolderPathBox.Text =  */
            OsuHelper.ChangeOsuPath();
        }

        /// <summary>
        /// Sets up the form for the user
        /// </summary>
        /// <param name="debugModeArgs">Should it be run in debug mode?</param>
        /// <param name="spamLogsArgs">Should it log everything it does?</param>
        public void SetupForm()
        {
            this.FormClosing += new FormClosingEventHandler(SaveEditedValues);
            DebugLog("[STARTING UP]", false);
            LoadValues();
            mainFont = new Font("Segoe UI", 12);
            searchBoxFont = new Font("Segoe UI", 10);

            //TODO: Fix changing osu path so OsuHelper.osuPath can be private set
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog()
            {
                InitialDirectory = OsuHelper.osuPath,
                Filter = "Directory|*.directory",
                FileName = "",
                Title = "Select osu! directory"
            };
            if (IsSavedValueEmpty(ValueName.osuPath)) //If the path is not set, try to get default directory. If it is not there throw an error
            {
                OsuHelper.osuPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE")!, "appdata", "Local", "osu!");
                if (!File.Exists(Path.Combine(OsuHelper.osuPath, "osu!.exe")))
                {
                    MessageBox.Show("Unable to find valid osu directory. Please select one.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ChangeOsuPath();
                }
            }
            else
                OsuHelper.osuPath = GetValue(ValueName.osuPath);

            helperSkin = HelperSkin.Instance;

            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Name = "Skin Manager";
            this.Text = "Skin Manager";
            this.Size = new Size(800, 800);
            this.Icon = new Icon(Path.Combine(".", "Images", "o_h_kDs_icon.ico"));

            List<Control> topRow = new List<Control>();
            List<Control> searchRow = new List<Control>();
            List<Control> bottomRow = new List<Control>();
            SetupTopRowControls(ref topRow);
            AddControls(topRow, 3);
            SetupSearchRowControls(ref searchRow);
            AddControls(searchRow, 33);
            SetupBottomRowControls(ref bottomRow);
            AddControls(bottomRow, 737);
            SetupSkinChangeCheckBoxes();

            SetupToolTip();

            DirectoryInfo osuPathDI = new DirectoryInfo(Path.Combine(OsuHelper.osuPath, "skins"));
            if (osuPathDI.Exists)
            {
                if (!Directory.Exists(helperSkin.path))
                    osuPathDI.CreateSubdirectory(OsuHelper.managerFolderName);

                if (!Directory.Exists(Path.Combine(OsuHelper.osuPath, "skins", "Deleted Skins")))
                    osuPathDI.CreateSubdirectory("Deleted Skins");


                FindOsuSkins(new Object());
                /*catch
                {
                    DebugLog("Error searching skins", true);
                } */
            }
            DebugLog("[STARTUP FINISHED. WAITING FOR INPUT]", false);
        }

        private void SetupToolTip()
        {
            toolTip = new ToolTip();
            //toolTip.SetToolTip(searchOsuSkinsButton, "Searches osu! folder for skins");
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

        private void AddControls(List<Control> controls, int fromTop)
        {
            int leftOffset = 1;
            foreach (Control obj in controls)
            {
                if (!(obj is ComboBox) && !(obj is TextBox))
                    obj.Width = TextRenderer.MeasureText(obj.Text, mainFont).Width + 10;
                else
                    DebugLog();

                obj.Top = fromTop;
                obj.Left = leftOffset;
                leftOffset += obj.Width + 3;
                Controls.Add(obj);
            }
        }

        private void SetupTopRowControls(ref List<Control> controls)
        {
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

            changeOsuPathButton = new System.Windows.Forms.Button()
            {
                Left = 0,
                Top = 3,
                Width = 84,
                Font = mainFont,
                Text = "Change osu! Path",
                Name = "changeOsuPathButton",
            };
            changeOsuPathButton.Click += new EventHandler(OnButtonClick);
            changeOsuPathButton.Select();

            disableSkinChangesBox = new CheckBox()
            {
                Height = 25,
                Width = 200,
                Left = 483,
                Top = 3,
                Text = "Disable skin changes",
                Name = ValueName.disableSkinChanges.ToString(),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            if (!IsSavedValueEmpty(ValueName.disableSkinChanges))
                disableSkinChangesBox.CheckState = GetCheckState(ValueName.disableSkinChanges);
            else
                disableSkinChangesBox.Checked = false;

            disableSkinChangesBox.CheckedChanged += new EventHandler(OnCheckBoxClick);

            osuSkinsListBox = new ListBox()
            {
                Width = 500,
                Height = 676,
                Top = 60,
                Font = mainFont,
                SelectionMode = SelectionMode.MultiExtended,
                BorderStyle = BorderStyle.Fixed3D,
            };

            /*
            osuFolderPathBox = new System.Windows.Forms.TextBox()
            {
                Text = OsuHelper.osuPath,
                Left = 87,
                Width = 300,
                Font = mainFont,
                Name = ValueName.osuPath.ToString(),
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

                    //OsuHelper.osuPath = osuFolderPathBox.Text;
                    //helperSkin = new Skin(Path.Combine(osuPath, "skins", managerFolderName));
                    OnCheckBoxClick(sender, thisEvent);
                    thisEvent.Handled = true;
                }
            }; */
        
            controls.Add(changeOsuPathButton);
            controls.Add(disableSkinChangesBox);
        }

        private void SetupSearchRowControls(ref List<Control> controls)
        {
            searchSkinBox = new TextBox()
            {
                Left = 218,
                Width = 100,
                Top = 32,
                Font = searchBoxFont,
                Name = ValueName.searchSkinsText.ToString(),
                PlaceholderText = "Search for skin",
            };
            searchSkinBox.KeyUp += UserSearchSkins;
            searchSkinBox.TextChanged += (sender, e) =>
            {
                int textWidth = TextRenderer.MeasureText(searchSkinBox.Text, searchBoxFont).Width;
                if (textWidth > 95)
                    searchSkinBox.Width = textWidth + 5;
                else if (textWidth < searchSkinBox.Width - 5)
                    searchSkinBox.Width = (textWidth < 95 ? 100 : textWidth + 5);
            };
            if (!IsSavedValueEmpty(ValueName.searchSkinsText))
                searchSkinBox.Text = GetValue(ValueName.searchSkinsText);

            hiddenSkinFiltersText = new CheckBox()
            {
                Top = 33,
                Font = mainFont,
                Left = 136,
                Text = "a",
                Name = ValueName.hiddenSkinFilter.ToString(),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            deleteSkinSelectorButton = new System.Windows.Forms.Button()
            {
                Top = 33,
                Font = mainFont,
                Width = 61,
                Left = 156,
                Text = "Delete",
                Name = "deleteSkinSelectorButton",
            };

            /* hiddenSkinFiltersText.TextChanged += new EventHandler((sender, ev) =>
            {
                int textWidth = TextRenderer.MeasureText(hiddenSkinFiltersText.Text, mainFont).Width;
                if (textWidth == 0)
                {
                    if (Controls.Contains(hiddenSkinFiltersText))
                        Controls.Remove(hiddenSkinFiltersText);
                    //deleteSkinSelectorButton.Left = 153;
                    //searchSkinBox.Left = 218;
                }
                else
                {
                    if (!Controls.Contains(hiddenSkinFiltersText))
                        Controls.Add(hiddenSkinFiltersText);

                    //deleteSkinSelectorButton.Left = textWidth + 152;
                    hiddenSkinFiltersText.Width = textWidth + 30;
                    //searchSkinBox.Left = textWidth + 218;
                }
            }); */

            if (!IsSavedValueEmpty(ValueName.hiddenSkinFilter))
                hiddenSkinFiltersText.Text = hiddenSkinFiltersText.Text = GetValue(ValueName.hiddenSkinFilter).Replace(",", ", ");
            else
                hiddenSkinFiltersText.Text = "";

            deleteSkinSelectorButton.Click += new EventHandler((sender, e) =>
            {
                if (skinFilterSelector.Items.Contains(skinFilterSelector.Text))
                {
                    string[] origValue = GetValue(ValueName.skinFilters).Split(',');
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

            skinFilterSelector = new ComboBox()
            {
                Top = 30,
                Font = mainFont,
                Height = 23,
                Width = 43,
                Name = ValueName.skinFilters.ToString(),
            };
            skinFilterSelector.KeyUp += (sender, thisEvent) =>
            {
                if (thisEvent.KeyCode.Equals(Keys.Enter))
                {
                    AddToSkinFilters();
                    thisEvent.Handled = true;
                }
            };
            if (!IsSavedValueEmpty(ValueName.skinFilters))
            {
                if (!skinFilterSelector.Items.Contains("All"))
                    skinFilterSelector.Items.Add("All");

                string[] foldersArr = GetValue(ValueName.skinFilters).Split(',');
                foreach (string name in foldersArr)
                    skinFilterSelector.Items.Add(name);
            }
            else
            {
                skinFilterSelector.Items.Add("All");
                skinFilterSelector.Text = "All";
            }
            if (!IsSavedValueEmpty(ValueName.selectedSkinFilter))
                skinFilterSelector.Text = GetValue(ValueName.selectedSkinFilter);
            else
                skinFilterSelector.Text = "All";

            showFilteredSkinsButton = new System.Windows.Forms.Button()
            {
                Top = 33,
                Font = mainFont,
                //Height = 23,
                Width = 53,
                Left = 46,
                Text = "Show",
                Name = "showFilteredSkinsButton",
            };
            showFilteredSkinsButton.Click += new EventHandler(OnButtonClick);

            hideSelectedSkinFilterButton = new System.Windows.Forms.Button()
            {
                Top = 33,
                Font = mainFont,
                Width = 48,
                Left = 102,
                Text = "Hide",
                Name = "hideSelectedSkinFilterButton",
            };
            hideSelectedSkinFilterButton.Click += new EventHandler(OnButtonClick);

            /* searchOsuSkinsButton = new System.Windows.Forms.Button()
            {
                Left = 390,
                Top = 3,
                Width = 90,
                Font = mainFont,
                Text = "Find Skins"
            };
            searchOsuSkinsButton.Click += new EventHandler(OnButtonClick);
            Controls.Add(searchOsuSkinsButton); */

            controls.Add(searchSkinBox);
            controls.Add(showFilteredSkinsButton);
            controls.Add(hideSelectedSkinFilterButton);
            controls.Add(deleteSkinSelectorButton);
            controls.Add(skinFilterSelector);
        }

        private void SetupBottomRowControls(ref List<Control> controls)
        {
            useSkinButton = new System.Windows.Forms.Button()
            {
                Left = 102,
                Top = 737,
                Width = 80,
                Font = mainFont,
                Text = "Use Skin",
                Name = "useSkinButton",
            };
            useSkinButton.Click += new EventHandler(OnButtonClick);

            randomSkinButton = new System.Windows.Forms.Button()
            {
                Left = 185,
                Top = 737,
                Width = 110,
                Font = mainFont,
                Text = "Random Skin",
                Name = "randomSkinButton",
            };
            randomSkinButton.Click += new EventHandler(OnButtonClick);

            deleteSkinButton = new System.Windows.Forms.Button()
            {
                Left = 3,
                Top = 737,
                Width = 96,
                Font = mainFont,
                Text = "Delete Skin",
                Name = "deleteSkinButton",
            };
            deleteSkinButton.Click += new EventHandler(OnButtonClick);

            openSkinFolderButton = new System.Windows.Forms.Button()
            {
                Left = 298,
                Top = 737,
                Width = 139,
                Font = mainFont,
                Text = "Open Skin Folder",
                Name = "openSkinFolderButton",
            };
            openSkinFolderButton.Click += new EventHandler(OnButtonClick);

            renameSkinButton = new System.Windows.Forms.Button()
            {
                Left = 440,
                Top = 737,
                Width = 107,
                Font = mainFont,
                Text = "Rename Skin",
                Name = "renameSkinButton",
            };
            renameSkinButton.Click += new EventHandler(OnButtonClick);
            
            controls.Add(useSkinButton);
            controls.Add(randomSkinButton);
            controls.Add(renameSkinButton);
            controls.Add(openSkinFolderButton);
            controls.Add(deleteSkinButton);
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
                workingCheckBox.CheckStateChanged += new EventHandler(OnCheckBoxClick);
                workingCheckBox.ThreeState = true;
                Controls.Add(workingCheckBox);
                indexInControls = Controls.IndexOf(workingCheckBox);

                switch (i)
                {
                    case 0:
                        Controls[indexInControls].Name = ValueName.showComboBursts.ToString();
                        Controls[indexInControls].Text = "Combo Bursts";
                        showComboBurstsBox = (CheckBox)Controls[indexInControls];
                        break;
                    case 1:
                        Controls[indexInControls].Name = ValueName.showHitLighting.ToString();
                        Controls[indexInControls].Text = "Hit Lighting";
                        showHitLightingBox = (CheckBox)Controls[indexInControls];
                        break;
                    case 2:
                        Controls[indexInControls].Name = ValueName.showHitCircleNumbers.ToString();
                        Controls[indexInControls].Text = "Hitcircle Numbers";
                        showSkinNumbersBox = (CheckBox)Controls[indexInControls];
                        break;
                    case 3:
                        Controls[indexInControls].Name = ValueName.disableCursorTrail.ToString();
                        Controls[indexInControls].Text = "Cursor Trail";
                        disableCursorTrailBox = (CheckBox)Controls[indexInControls];
                        break;
                    case 4:
                        Controls[indexInControls].Name = ValueName.showSliderEnds.ToString();
                        Controls[indexInControls].Text = "Slider Ends";
                        showSliderEndsBox = (CheckBox)Controls[indexInControls];
                        break;
                    case 5:
                        Controls[indexInControls].Name = ValueName.showHitCircles.ToString();
                        Controls[indexInControls].Text = "Show Hitcircles";
                        defaultCheckState = CheckState.Checked;
                        showHitCirclesBox = (CheckBox)Controls[indexInControls];
                        break;
                    case 6:
                        Controls[indexInControls].Name = ValueName.expandingCursor.ToString();
                        Controls[indexInControls].Text = "Expanding Cursor";
                        expandingCursorBox = (CheckBox)Controls[indexInControls];
                        break;
                    case 7:
                        Controls[indexInControls].Name = ValueName.makeInstafade.ToString();
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

        #endregion

        #region Find/search skins

        /// <summary>
        /// Called when the user types in <paramref name="searchSkinBox"/>.
        /// </summary>
        private void UserSearchSkins(object sender, EventArgs e)
        {
            FindOsuSkins(sender);
        }

        /// <summary>
        /// Searches the skin directory and adds skins to <paramref name="osuSkinsList"/> and <paramref name="osuSkinsListBox"/>.
        /// </summary>
        private void FindOsuSkins(object sender)
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
            DirectoryInfo skinFolderDI = new DirectoryInfo(Path.Combine(OsuHelper.osuPath, "skins"));
            DirectoryInfo[] osuSkins = skinFolderDI.GetDirectories();

            foreach (DirectoryInfo workingSkin in osuSkins)
            {
                AddSkin(new UserSkin(workingSkin.FullName));
            }
            osuSkinsListBox.EndUpdate();
            if (!Controls.Contains(osuSkinsListBox))
                Controls.Add(osuSkinsListBox);

            //if skin that was last selected is shown, select it
            string lastSkinName = GetValue(ValueName.selectedSkin);
            if (lastSkinName != null && osuSkinsListBox.Items.Contains(lastSkinName))
            {
                osuSkinsListBox.SetSelected(osuSkinsListBox.Items.IndexOf(lastSkinName), true);
                DebugLog("Previous selected skin name: " + lastSkinName, false);
            }

            if (sender != searchSkinBox)
                EnableAllControls(true);
        }

        #endregion

        #region Selected Skin Handling

        /// <summary>
        /// Opens the skin folder of the selected skin.
        /// </summary>
        /// <remarks>
        /// If no skin is selected, throws error.
        /// </remarks>
        private void OpenSelectedSkinFolder()
        {
            if (osuSkinsListBox.SelectedItem == null)
            {
                DebugLog("Select skin before trying to open its folder", true);
                return;
            }
            DebugLog("Attempting open skin folder: " + Path.Combine(OsuHelper.osuPath, "skins", osuSkinsListBox.SelectedItem.ToString()), false);

            if (OperatingSystem.IsWindows())
                Process.Start("explorer.exe", Path.Combine(OsuHelper.osuPath, "skins", osuSkinsListBox.SelectedItem.ToString()));
        }

        /// <summary>
        /// Deletes the selected skin.
        /// </summary>
        private void DeleteSelectedSkin()
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
            Directory.Move(currentSkinPath, Path.Combine(OsuHelper.osuPath, "skins", "Deleted Skins", osuSkinsListBox.SelectedItem.ToString()));
            DebugLog($"Moving {currentSkinPath} to {Path.Combine(OsuHelper.osuPath, "skins", "Deleted Skins", osuSkinsListBox.SelectedItem.ToString())}", false);
            osuSkinsList.RemoveAt(osuSkinsListBox.SelectedIndex);
            osuSkinsListBox.Items.RemoveAt(osuSkinsListBox.SelectedIndex);
            EnableAllControls(true);
        }

        /// <summary>
        /// Renames the selected skin
        /// </summary>
        private void RenameSelectedSkin()
        {
            if (osuSkinsListBox.SelectedItems.Count != 1)
                return;

            string renameTo = osuSkinsListBox.SelectedItem.ToString();

            if (PopUp.InputBox("Rename", "Rename:", ref renameTo) == DialogResult.Cancel)
                return;

            while (renameTo.Contains(Path.DirectorySeparatorChar))
                if (PopUp.InputBox("Cannot contain \"" + Path.DirectorySeparatorChar + "\"", "Rename:", ref renameTo) == DialogResult.Cancel)
                    return;


            renameTo = osuSkinsList[osuSkinsListBox.SelectedIndex].path.Replace(osuSkinsListBox.SelectedItem.ToString(), renameTo);
            if (osuSkinsList[osuSkinsListBox.SelectedIndex].path != renameTo)
                Directory.Move(osuSkinsList[osuSkinsListBox.SelectedIndex].path, renameTo);

            FindOsuSkins(new Object());

            osuSkinsListBox.ClearSelected();

            osuSkinsListBox.SetSelected(osuSkinsList.IndexOf(new UserSkin(renameTo)), true);
        }

        #region Change to skin

        /// <summary>
        /// Changes to the selected skin
        /// </summary>
        private void ChangeToSelectedSkin()
        {
            DebugLog("[STARTING TO CHANGE SKIN]", false);
            if (osuSkinsListBox.SelectedItem == null)
            {
                DebugLog("Please select skin before trying to change to it.", true);
                return;
            }
            EnableAllControls(false);

            helperSkin.DeleteSkinElements();
            UserSkin workingSkin;
            if (osuSkinsListBox.SelectedItems.Count == 1) //false if multiple skins are selected
                workingSkin = osuSkinsList[osuSkinsListBox.SelectedIndex];
            else
            {
                Random r = new Random();
                int randomSkinIndex = osuSkinsListBox.Items.IndexOf(osuSkinsListBox.SelectedItems[r.Next(0, osuSkinsListBox.SelectedItems.Count)]);
                workingSkin = osuSkinsList[randomSkinIndex];

                osuSkinsListBox.ClearSelected();
                osuSkinsListBox.SetSelected(randomSkinIndex, true);
                DebugLog($"Changing to random skin from selected | Selected: {osuSkinsListBox.SelectedItem.ToString()}", false);
            }

            workingSkin.ChangeToSkin();

            DebugLog("[FINISHED CHANGING TO SKIN]", false);

            if (!disableSkinChangesBox!.Checked)
            {
                DebugLog("[STARTING EDITING SKIN]", false);
                GetCurrentSkin().EditSkin(Controls);
                /* GetCurrentSkin().ShowHitCircleNumbers(showSkinNumbersBox.CheckState);
                GetCurrentSkin().ShowSliderEnds(showSliderEndsBox.CheckState);
                GetCurrentSkin().ShowCursorTrail(disableCursorTrailBox.CheckState);
                GetCurrentSkin().ShowComboBursts(showComboBurstsBox.CheckState);
                GetCurrentSkin().ShowHitLighting(showHitLightingBox.CheckState);
                GetCurrentSkin().ShowHitCircles(showHitCirclesBox.CheckState);
                GetCurrentSkin().ChangeExpandingCursor(expandingCursorBox.CheckState); */
                //MakeInstafade(makeInstafadeBox.CheckState);
                DebugLog("[FINISHED EDITING SKIN]", false);
            }
            EnableAllControls(true);
        }

        /// <summary>
        /// Selects a random skin of all the available skins.
        /// </summary>
        private void ChangeToRandomSkin()
        {
            EnableAllControls(false);
            Random r = new Random();
            osuSkinsListBox.ClearSelected();
            osuSkinsListBox.SetSelected(r.Next(0, osuSkinsListBox.Items.Count), true);
            DebugLog($"Changed to random skin. Picked \"{osuSkinsListBox.SelectedItem.ToString()}\"", false);
            ChangeToSelectedSkin();
        }

        #endregion

        #endregion

        #region On Click

        /// <summary>
        /// Handles button click events
        /// </summary>
        private void OnButtonClick(object sender, EventArgs e)
        {
            if (sender == randomSkinButton)
                ChangeToRandomSkin();
            else if (sender == useSkinButton)
                ChangeToSelectedSkin();
            else if (sender == deleteSkinButton)
                DeleteSelectedSkin();
            else if (sender == openSkinFolderButton)
                OpenSelectedSkinFolder();
            else if (sender == searchSkinBox || sender == hideSelectedSkinFilterButton || sender == showFilteredSkinsButton)
                FindOsuSkins(sender);
            else if (sender == changeOsuPathButton)
                ChangeOsuPath();
            else if (sender == searchSkinBox)
                FindOsuSkins(sender);
            else if (sender == renameSkinButton)
                RenameSelectedSkin();
            else
                DebugLog("Problem with OnButtonClick. Sender: " + ((Control)sender).Name + " | EventArgs: " + e.ToString(), true);
        }

        /// <summary>
        /// Handles click events of all of the checkboxes.
        /// </summary>
        private void OnCheckBoxClick(object sender, EventArgs e)
        {
            if (osuSkinsListBox.SelectedIndex == -1)
                return;
            else if (sender == showSkinNumbersBox)
                GetCurrentSkin().ShowHitCircleNumbers(showSkinNumbersBox.CheckState);
            else if (sender == showSliderEndsBox)
                GetCurrentSkin().ShowSliderEnds(showSliderEndsBox.CheckState);
            else if (sender == disableCursorTrailBox)
                GetCurrentSkin().ShowCursorTrail(disableCursorTrailBox.CheckState);
            else if (sender == showComboBurstsBox)
                GetCurrentSkin().ShowComboBursts(showComboBurstsBox.CheckState);
            else if (sender == showHitLightingBox)
                GetCurrentSkin().ShowHitLighting(showHitLightingBox.CheckState);
            else if (sender == showHitCirclesBox)
                GetCurrentSkin().ShowHitCircles(showHitCirclesBox.CheckState);
            else if (sender == expandingCursorBox)
                GetCurrentSkin().ChangeExpandingCursor(expandingCursorBox.CheckState);
            else if (sender == disableSkinChangesBox) { }
            /*         else if (sender == makeInstafadeBox)
                        MakeInstafade(makeInstafadeBox.CheckState); */
            else
            {
                DebugLog("Error with OnClick | " + sender.ToString(), true);
                return;
            }
        }

        #endregion

        #region Handle saved and loaded values

        /// <param name="valName">The name you want the value of</param>
        /// <returns>The value of the ValueName with the same name.</returns>
        private string GetValue(ValueName valName)
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

        /// <param name="valName">The name you want the value of</param>
        /// <returns>The value of the ValueName with the same name.</returns>
        private string GetValue(string valName)
        {
            return GetValue(ParseValueName(valName));
        }

        /// <summary>
        /// Parses <paramref name="name"/> to <paramref name="ValueName"/>.
        /// </summary>
        /// <remarks>
        /// If <paramref name="name"/> is not a <paramref name="ValueName"/>, returns ValueName.defaultValue
        /// </remarks>
        /// <param name="name">The name to parse</param>
        public static ValueName ParseValueName(string name)
        {
            object result;
            if (Enum.TryParse(typeof(ValueName), name, true, out result))
                return (ValueName)result;
            else
                return ValueName.defaultValue;
        }

        /// <summary>
        /// Returns true if the saved value is non-existent or empty.
        /// </summary>
        /// <param name="valName">The name of the value you are checking.</param>
        /// <returns>bool if the values is non-existent or empty.</returns>
        private bool IsSavedValueEmpty(ValueName valName)
        {
            if (!loadedValues.Keys.Contains(valName))
                return true;

            return String.IsNullOrWhiteSpace(loadedValues[valName]);
        }

        /// <param name="name">The name of the checkbox you want the state of.</param>
        /// <returns>The CheckState of <paramref name="name"/>.</returns>
        private CheckState GetCheckState(ValueName name)
        {
            if (!IsSavedValueEmpty(name))
                return (CheckState)Enum.Parse(typeof(CheckState), GetValue(name), true);
            else
                return CheckState.Unchecked;

        }

        /// <summary>
        /// Loads the values from "settings.txt" into <paramref name="loadedValues"/>.
        /// </summary>
        private void LoadValues()
        {
            if (!File.Exists("settings.txt"))
                return;

            DebugLog("[Starting loading values from settings.txt]", false);
            StreamReader reader = new StreamReader("settings.txt");

            string curLine;

            while ((curLine = reader.ReadLine()) != null)
            {
                if (OsuHelper.spamLogs)
                    DebugLog("Read | " + curLine, false);

                string[] curLineArr = curLine.Split(',');
                if (curLineArr[0].Equals(ValueName.skinFilters.ToString()))
                {
                    curLineArr[1] = curLine.Replace(ValueName.skinFilters.ToString() + ",", "");
                }
                loadedValues.Add(ParseValueName(curLineArr[0]), curLineArr[1]);
            }
            reader.Dispose();
            DebugLog("[Finished loading values from settings.txt]", false);
        }

        /// <summary>
        /// Saves the values of checkboxes, filters, ect. to settings.txt
        /// </summary>
        private void SaveEditedValues(object sender, EventArgs e)
        {
            //save the values of things
            if (!File.Exists("settings.txt"))
                File.Create("settings.txt");

            StreamWriter writer = new StreamWriter("settings.txt");

            foreach (Control currentObj in Controls)
            {
                if (currentObj.Name == ValueName.skinFilters.ToString()) //is the skin filters
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
                    if (currentObj.Name == ValueName.hiddenSkinFilter.ToString())
                        continue;
                    else if (currentObj is CheckBox/*  &&
                    (((CheckBox)currentObj).CheckState != CheckState.Indeterminate) */)
                        writer.WriteLine(((CheckBox)currentObj).Name + "," + ((CheckBox)currentObj).CheckState.ToString());
                    else if (currentObj.Name == ValueName.selectedSkinFilter.ToString())
                    {
                        if (currentObj.Text != "All" && !String.IsNullOrWhiteSpace(currentObj.Text))
                            writer.WriteLine(ValueName.selectedSkinFilter.ToString() + "," + currentObj.Text);
                    }
                    else if (currentObj is TextBox)
                    {
                        if (String.IsNullOrWhiteSpace(currentObj.Text))
                            continue;
                        if (currentObj.Name == ValueName.osuPath.ToString())
                            continue;

                        writer.WriteLine(currentObj.Name + "," + currentObj.Text);
                    }
                }
            }
            if (osuSkinsListBox.SelectedItems.Count == 1)
                writer.WriteLine(ValueName.selectedSkin.ToString() + "," + osuSkinsListBox.SelectedItem.ToString());

            if (OsuHelper.osuPath != Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "appdata", "Local", "osu!"))
                writer.WriteLine(ValueName.osuPath.ToString() + "," + OsuHelper.osuPath);

            writer.Dispose();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if <paramref name="ValueNames"/> contains <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The string form of a potential <paramref name="ValueNames"/> to check</param>
        /// <returns>bool true is <paramref name="ValueNames"/> contains <paramref name="name"/></returns>
        public static bool ValueNameContains(string name)
        {
            foreach (ValueName val in Enum.GetValues(typeof(ValueName)))
            {
                if (name.Equals(val.ToString(), StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Adds <paramref name="skinFilterSelector.Text"/> to available skin filters, if it is not already in there.
        /// </summary>
        private void AddToSkinFilters()
        {
            if (skinFilterSelector.Text == ",")
            {
                DebugLog("You cannot add \",\" as a prefix", true);
            }
            else if (skinFilterSelector.Text != "All" && !skinFilterSelector.Items.Contains(skinFilterSelector.Text))
            {
                skinFilterSelector.Items.Add(skinFilterSelector.Text);
            }
        }

        /// <summary>
        /// Gets the current skin that is being used.
        /// </summary>
        /// <remarks>
        /// If no skin is selected and no name is saved, throws an error.
        /// </remarks>
        /// <returns>The UserSkin object of the skin that is being used.</returns>
        private UserSkin GetCurrentSkin()
        {
            if (osuSkinsListBox.SelectedItems.Count == 1)
                return osuSkinsList[osuSkinsListBox.SelectedIndex];
            else if (String.IsNullOrWhiteSpace(GetValue(ValueName.selectedSkin)) || osuSkinsListBox.SelectedItems.Count > 1)
                DebugLog("Multiple/no skins selected. Unable to get skin path.", true);
            else
                return new UserSkin(GetValue(ValueName.selectedSkin), false);

            return null;
        }

        /// <summary>
        /// Enables or disables all controls.
        /// </summary>
        /// <param name="enable">If true, controls are enabled.</param>
        private void EnableAllControls(bool enable)
        {
            DebugLog($"EnableAllControls({enable}) called", false);
            foreach (Control obj in Controls)
            {
                if (obj != null)
                    obj.Enabled = enable;
            }
        }

        /// <summary>
        /// Adds <paramref name="skin"/> to <paramref name="osuSkinsList"/> and <paramref name="osuSkinsListBox"/>.
        /// </summary>
        /// <remarks>
        /// Checks against the filters, the search box, and the hidden filters.
        /// </remarks>
        /// <param name="skin">The skin to be added</param>
        private void AddSkin(UserSkin skin)
        {
            bool shouldAdd = false;

            if (skin.name != OsuHelper.managerFolderName && skin.name != "Deleted Skins")
            {
                if (!hiddenSkinFiltersText.Text.Contains(skin.name.First<char>().ToString())) //true if skin does not have prefix that is supposed to be hidden
                {
                    if (skinFilterSelector.Text != "All" && skin.name.IndexOf(skinFilterSelector.Text) == 0)
                        shouldAdd = true;
                    else if (skinFilterSelector.Text == "All")
                        shouldAdd = true;
                }
            }

            if (shouldAdd && skin.name.Contains(searchSkinBox.Text, StringComparison.OrdinalIgnoreCase))
            {
                osuSkinsList.Add(skin);
                osuSkinsListBox.Items.Add(skin.name);
                if (OsuHelper.spamLogs)
                    DebugLog($"Adding {skin.name} to the skin list | {skin.path}", false);
            }
        }

        #endregion

        #region Debug Stuff
        /// <summary>
        /// Writes <paramref name="log"/>.toString() to the console if <paramref name="alwaysLog"/> is true or if debug more is on.
        /// </summary>
        /// <param name="log">What will be written to the console.</param>
        /// <param name="alwaysLog">If true, will always log the message regardless of <paramref name="debugMode"/></param>
        public static void DebugLog(object log, bool alwaysLog)
        {
            if (alwaysLog || OsuHelper.debugMode)
                Console.WriteLine(log.ToString());
        }

        /// <summary>
        /// Writes "Test debug" to the console. 
        /// </summary>
        public static void DebugLog()
        {
            Console.WriteLine("Test debug");
        }
        #endregion
    }
}