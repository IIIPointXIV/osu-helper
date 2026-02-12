using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.Control;

namespace osu_helper
{
    /// <summary>
    /// Made of the skin folder that the user would use. Has the ability to alter itself.
    /// </summary>
    public class UserSkin : Skin
    {
        #region Constructors
        /// <param name="path">The path that this skin will use.</param>
        public UserSkin(string path) : base(path) { }

        /// <param name="nameOrPath">The name or path that this skin will use.</param>
        /// <param name="isPath">If true, the skin given will be treated as a path.</param>
        public UserSkin(string nameOrPath, bool isPath) : base(nameOrPath, isPath) { }

        #endregion

        /// <summary>
        /// Calls all skin edit functions on skin.
        /// </summary>
        /// <param name="controls">Controls of main window</param>
        public void EditSkin(ControlCollection controls)
        {
            foreach (Control obj in controls)
            {
                if (obj is CheckBox box && MainForm.ValueNameContains(obj.Name))
                {
                    CheckState state = box.CheckState;
                    switch (MainForm.ParseValueName(obj.Name))
                    {
                        case MainForm.ValueName.disableCursorTrail:
                            ShowCursorTrail(state);
                            break;
                        case MainForm.ValueName.expandingCursor:
                            ChangeExpandingCursor(state);
                            break;
                        case MainForm.ValueName.showComboBursts:
                            ShowComboBursts(state);
                            break;
                        case MainForm.ValueName.showHitCircles:
                            ShowHitCircles(state);
                            break;
                        case MainForm.ValueName.showHitLighting:
                            ShowHitLighting(state);
                            break;
                        case MainForm.ValueName.showHitCircleNumbers:
                            ShowHitCircleNumbers(state);
                            break;
                        case MainForm.ValueName.showSliderEnds:
                            ShowSliderEnds(state);
                            break;
                        case MainForm.ValueName.disableSkinChanges:
                            break;
                        default:
                            MainForm.DebugLog("Error editing skin. Passed value was: " + obj.Name, true);
                            break;
                    }
                }
            }
        }

        #region Edit INI things

        /// <summary>
        /// Reverts the helper skin ini to what it is in the original skin where it finds the <paramref name="searchFor"/> string.
        /// </summary>
        /// <remarks>
        /// Only reverts the line where <paramref name="searchFor"/> can be found.
        /// If <paramref name="searchFor"/> cannot be found, does nothing.
        /// </remarks>
        /// <param name="searchFor">Used to search the ini for what to restore.</param>
        private void RevertSkinIni(string searchFor)
        {
            MainForm.DebugLog($"Attempting to restore {searchFor}. In the skin.ini", false);
            using StreamReader origINIReader = new(INIPath);
            string? currLine;

            while ((currLine = origINIReader.ReadLine()) != null)
            {
                if (currLine.ToLower().Contains(searchFor.ToLower()))
                    break;
            }

            if (currLine == null)
                EditSkinIni(searchFor, "", "really long word that should not be there");
            else
                EditSkinIni(searchFor, currLine, "really long word that should not be there");
        }

        /// <summary>
        /// Replaces the skin ini line <paramref name="searchFor"/> with <paramref name="replaceWith"/>.
        /// </summary>
        /// <remarks>
        /// If it does not find <paramref name="searchFor"/>, it looks for <paramref name="fallBackSearch"/>, and if it finds it, adds <paramref name="replaceWith"/> after it.
        /// If it finds neither, does nothing.
        /// </remarks>
        /// <param name="searchFor">The string that will be used to search for what needs to be replaced</param>
        /// <param name="replaceWith">What will replace <paramref name="searchFor"/></param>
        /// <param name="fallBackSearch">What will be used to search if <paramref name="searchFor"/> cannot be found. Will add <paramref name="replaceWith"/> after it.</param>
        private void EditSkinIni(string searchFor, string replaceWith, string fallBackSearch)
        {
            MainForm.DebugLog($"Attempting to search for {searchFor}, replace it with {replaceWith}, with a fallback of {fallBackSearch}. In the skin.ini", false);
            /* if(File.Exists(Form1.helperSkin.iniPath.Replace("skin.ini", "skin.ini.temp")))
                File.Delete(Form1.helperSkin.iniPath.Replace("skin.ini", "skin.ini.temp")); */

            File.Copy(MainForm.HelperSkin.INIPath, MainForm.HelperSkin.TempINIPath);
            using (StreamReader reader = new(MainForm.HelperSkin.TempINIPath))
            {
                using StreamWriter writer = new(MainForm.HelperSkin.INIPath);
                string? currLine;
                bool lineFound = false;

                //Loop through the ini looking for searchFor
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

                if (!lineFound)
                {
                    using StreamWriter writerNew = new(MainForm.HelperSkin.INIPath);
                    using StreamReader readerNew = new(MainForm.HelperSkin.TempINIPath);

                    //Loop through ini looking for fallBackSearch
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
                }
            }

            File.Delete(MainForm.HelperSkin.TempINIPath);
        }

        /* private string SearchSkinINI(string searchFor)
        {
            Form1.DebugLog($"Searching skin.ini for {searchFor}", false);
            StreamReader reader = new StreamReader(iniPath);
            string currLine = null;
            while ((currLine = reader.ReadLine()) != null)
            {
                if (currLine.Contains(searchFor))
                    break;
            }
            reader.Close();
            reader.Dispose();
            return currLine;
        } */

        #endregion

        #region Edit hit objects/sliders

        /// <summary>
        /// Shows or hides the skin slider ends.
        /// </summary>
        /// <remarks>
        /// If CheckState.Indeterminate, does nothing
        /// </remarks>
        /// <param name="showState">Checked: Shows slider ends
        /// Unchecked: Hides slider ends
        /// Indeterminate: Does nothing</param>
        public void ShowSliderEnds(CheckState showState)
        {
            MainForm.DebugLog($"{nameof(ShowSliderEnds)}({showState}) called", false);

            if (showState == CheckState.Indeterminate)
                return;

            string[] sliderEndFileNames =
            {
                "sliderendcircle.png",
                "sliderendcircle@2x.png",
                "sliderendcircleoverlay.png",
                "sliderendcircleoverlay@2x.png"
            };

            //using Image sliderImage = new Bitmap(1, 1);
            string skipAt2X = "";

            if (showState == CheckState.Checked) //Showing ends
            {
                foreach (string fileName in sliderEndFileNames)
                {
                    if (skipAt2X == fileName)
                    {
                        skipAt2X = "";
                        continue;
                    }

                    using Image? image = FileOfNameExists(fileName) ? Image.FromFile(GetPathOfFile(fileName)) : null;

                    if (FileOfNameExists(fileName) && (image == null || image.Size.Height < 100))
                    {
                        File.Copy(GetPathOfFile(fileName), MainForm.HelperSkin.GetPathOfFile(fileName), true);

                        if (OsuHelper.SpamLogs)
                            MainForm.DebugLog($"Copying {GetPathOfFile(fileName)} to {MainForm.HelperSkin.GetPathOfFile(fileName)}", false);
                    }
                    else if (FileOfNameExists(fileName.Replace("end", "start")))
                    {
                        File.Copy(GetPathOfFile(fileName.Replace("sliderendcircle", "sliderstartcircle")), MainForm.HelperSkin.GetPathOfFile(fileName), true);

                        if (OsuHelper.SpamLogs)
                            MainForm.DebugLog($"Copying {GetPathOfFile(fileName.Replace("sliderendcircle", "sliderstartcircle"))} to {MainForm.HelperSkin.GetPathOfFile(fileName)}", false);

                        if (!File.Exists(GetPathOfFile(fileName.Replace(".png", "@2x.png"))))
                            skipAt2X = fileName.Replace(".png", "@2x.png");
                    }
                    else if (FileOfNameExists(fileName.Replace("sliderend", "hit")))
                    {
                        File.Copy(GetPathOfFile(fileName.Replace("sliderend", "hit")), MainForm.HelperSkin.GetPathOfFile(fileName), true);

                        if (OsuHelper.SpamLogs)
                            MainForm.DebugLog($"Copying {GetPathOfFile(fileName.Replace("sliderend", "hit"))} to {MainForm.HelperSkin.GetPathOfFile(fileName)}", false);
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
            else//hiding ends
            {
                foreach (string fileName in sliderEndFileNames)
                    EmptyImage.Save(MainForm.HelperSkin.GetPathOfFile(fileName));
            }
        }

        /// <summary>
        /// Shows or hides the skin hit circles.
        /// </summary>
        /// <remarks>
        /// If CheckState.Indeterminate, changes to CheckState.Checked
        /// </remarks>
        /// <param name="showState">Checked: Shows hit circles
        /// Unchecked: Hides hit circles
        /// Indeterminate: Shows hit circles</param>
        public void ShowHitCircles(CheckState showState)
        {
            MainForm.DebugLog($"{nameof(ShowHitCircles)}({showState}) called", false);

            if (showState == CheckState.Indeterminate)
                showState = CheckState.Checked;

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

            if (showState == CheckState.Checked)
                CopyFilesToHelperSkin(fileNames);
            else
            {
                foreach (string curFileName in fileNames)
                {
                    EmptyImage.Save(MainForm.HelperSkin.GetPathOfFile(curFileName));
                    if (OsuHelper.SpamLogs)
                        MainForm.DebugLog($"Copying empty image to {MainForm.HelperSkin.GetPathOfFile(curFileName)}", false);
                }
            }

        }

        /// <summary>
        /// Shows or hides the skin hit circle numbers.
        /// </summary>
        /// <remarks>
        /// If CheckState.Indeterminate, does nothing
        /// </remarks>
        /// <param name="showState">Checked: Shows hit circle numbers.
        /// Unchecked: Hides hit circle numbers.
        /// Indeterminate: Does nothing</param>
        public void ShowHitCircleNumbers(CheckState showState)
        {
            MainForm.DebugLog($"{nameof(ShowHitCircleNumbers)}({showState}) called", false);

            if (showState == CheckState.Indeterminate)
                return;

            if (showState == CheckState.Checked) //show skin numbers
            {
                File.Copy(INIPath, MainForm.HelperSkin.INIPath, true);
                MainForm.DebugLog($"Copying {INIPath} to {MainForm.HelperSkin.INIPath}", false);
            }
            else //hide skin numbers
                EditSkinIni("HitCirclePrefix:", "HitCirclePrefix: 727", "[Fonts]");

        }

        #endregion

        #region Edit cursor

        /// <summary>
        /// Changes if the cursor expands or not
        /// </summary>
        /// <param name="expand">Checked: Cursor does expand /
        /// Unchecked: Cursor does not expand /
        /// Indeterminate: Reverts skin ini to original</param>
        public void ChangeExpandingCursor(CheckState expand)
        {
            MainForm.DebugLog($"{nameof(ChangeExpandingCursor)}({expand}) called", false);

            if (expand == CheckState.Indeterminate)
            {
                RevertSkinIni("CursorExpand:");
                return;
            }

            if (expand == CheckState.Checked)
                EditSkinIni("CursorExpand:", "CursorExpand: 1", "[General]");
            else
                EditSkinIni("CursorExpand:", "CursorExpand: 0", "[General]");
        }

        /// <summary>
        /// Shows or hides the cursor trail.
        /// </summary>
        /// <remarks>
        /// Does nothing on CheckState.Indeterminate
        /// </remarks>
        /// <param name="showState">Checked: Trail is shown
        /// Unchecked: Trail is hidden
        /// Indeterminate: Does nothing</param>
        public void ShowCursorTrail(CheckState showState)
        {
            MainForm.DebugLog($"{nameof(ShowCursorTrail)}{showState}) called", false);

            if (showState == CheckState.Indeterminate)
                return;

            string[] cursorFileNames =
            {
                "cursortrail@2x.png",
                "cursortrail.png",
            };

            if (showState == CheckState.Checked)
            {
                foreach (string fileName in cursorFileNames)
                    if (FileOfNameExists(fileName))
                    {
                        File.Copy(GetPathOfFile(fileName), MainForm.HelperSkin.GetPathOfFile(fileName), true);

                        if (OsuHelper.SpamLogs)
                            MainForm.DebugLog($"Copying {GetPathOfFile(fileName)} to {MainForm.HelperSkin.GetPathOfFile(fileName)}", false);
                    }
            }
            else
            {
                foreach (string name in cursorFileNames)
                {
                    EmptyImage.Save(MainForm.HelperSkin.GetPathOfFile(name));

                    if (OsuHelper.SpamLogs)
                        MainForm.DebugLog($"Saving empty image to {MainForm.HelperSkin.GetPathOfFile(name)}", false);
                }
            }

        }

        #endregion

        #region Edit other things

        /// <summary>
        /// Shows or hides hit-lighting
        /// </summary>
        /// <param name="showState">Checked: Shows hit lighting /
        /// Unchecked: Hides hit lighting /
        /// Indeterminate: Reverts skin hit lighting to original</param>
        public void ShowHitLighting(CheckState showState)
        {
            MainForm.DebugLog($"{nameof(ShowHitLighting)}({showState}) called", false);

            string[] fileNames =
            {
                "lighting.png",
                "lighting@2x.png",
            };

            if (showState == CheckState.Indeterminate)
                CopyFilesToHelperSkin(fileNames);
            else if (showState == CheckState.Checked)
            {
                string currentSkinFilePath;
                string helperSkinFilePath;
                foreach (string curFileName in fileNames)
                {
                    currentSkinFilePath = GetPathOfFile(curFileName);
                    helperSkinFilePath = MainForm.HelperSkin.GetPathOfFile(curFileName);

                    if (File.Exists(currentSkinFilePath))
                    {
                        using Image thisImg = Image.FromFile(currentSkinFilePath);
                        if (thisImg.Height > 100)
                        {
                            File.Copy(currentSkinFilePath, helperSkinFilePath, true);

                            if (OsuHelper.SpamLogs)
                                MainForm.DebugLog($"Copying {currentSkinFilePath} to {helperSkinFilePath}", false);

                            continue;
                        }
                        File.Delete(helperSkinFilePath);

                        if (OsuHelper.SpamLogs)
                            MainForm.DebugLog($"Deleting {helperSkinFilePath}", false);
                    }
                    else if (File.Exists(helperSkinFilePath))
                    {
                        File.Delete(helperSkinFilePath);

                        if (OsuHelper.SpamLogs)
                            MainForm.DebugLog($"Deleting {helperSkinFilePath}", false);
                    }
                }
            }
            else
            {
                string helperSkinFilePath;
                foreach (string name in fileNames)
                {
                    helperSkinFilePath = MainForm.HelperSkin.GetPathOfFile(name);

                    EmptyImage.Save(helperSkinFilePath);

                    if (OsuHelper.SpamLogs)
                        MainForm.DebugLog($"Saving empty image to {helperSkinFilePath}", false);
                }
            }

        }

        /// <summary>
        /// Shows or hides combo bursts
        /// </summary>
        /// <param name="showState">Checked: Shows combo bursts
        /// Unchecked: Hides combo bursts
        /// Indeterminate: Hides combo bursts</param>
        public void ShowComboBursts(CheckState showState)
        {
            MainForm.DebugLog($"{nameof(ShowComboBursts)}({showState}) called", false);

            if (showState == CheckState.Indeterminate)
                showState = CheckState.Unchecked;

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
                if (!MainForm.HelperSkin.FileOfNameExists(name + ".png"))
                {
                    EmptyImage.Save(MainForm.HelperSkin.GetPathOfFile(name + ".png"));
                    if (OsuHelper.SpamLogs)
                        MainForm.DebugLog($"Saving empty image to {MainForm.HelperSkin.GetPathOfFile(name + ".png")}", false);
                }
            }

            //incase there are multiple combobursts
            DirectoryInfo di = new(Path);
            foreach (FileInfo file in di.GetFiles())
            {
                foreach (string name in fileNames)
                    if (file.Name.Contains(name))
                    {
                        if (showState == CheckState.Checked)
                            File.Copy(file.FullName, MainForm.HelperSkin.GetPathOfFile(file.Name), true);
                        else
                            EmptyImage.Save(MainForm.HelperSkin.GetPathOfFile(file.Name));

                        if (OsuHelper.SpamLogs)
                            MainForm.DebugLog($"Saving empty image to {MainForm.HelperSkin.GetPathOfFile(file.Name)}", false);
                    }
            }
        }

        #endregion

        #region Copy to helper skin

        /// <summary>
        /// Copies all files in <paramref name="fileNames"/> to the helper skin.
        /// </summary>
        /// <param name="fileNames">The files to be copied</param>
        private void CopyFilesToHelperSkin(string[] fileNames)
        {
            string currentSkinFilePath;
            string helperSkinFilePath;
            foreach (string curName in fileNames)
            {
                currentSkinFilePath = GetPathOfFile(curName);
                helperSkinFilePath = MainForm.HelperSkin.GetPathOfFile(curName);
                if (File.Exists(currentSkinFilePath))
                {
                    File.Copy(currentSkinFilePath, helperSkinFilePath, true);

                    if (OsuHelper.SpamLogs)
                        MainForm.DebugLog($"Copying {currentSkinFilePath} to {helperSkinFilePath}", false);
                }
            }
        }

        /// <summary>
        /// Changes the helper skin to this skin.
        /// </summary>
        public void ChangeToSkin()
        {
            DirectoryInfo workingSkinPathDi = new(Path);

            foreach (FileInfo currentFile in workingSkinPathDi.GetFiles())
            {
                currentFile.CopyTo(MainForm.HelperSkin.GetPathOfFile(currentFile.Name), true);
                if (OsuHelper.SpamLogs)
                    MainForm.DebugLog($"Copying \"{currentFile.FullName}\" to \"{MainForm.HelperSkin.GetPathOfFile(currentFile.Name)}\"", false);
            }
            RecursiveSkinFolderMove("\\");
        }

        /// <summary>
        /// Recursively copies the entire folder tree to the helper skin.
        /// </summary>
        /// <param name="prevFolder">The path of the previous folders</param>
        private void RecursiveSkinFolderMove(string prevFolder)
        {
            DirectoryInfo rootFolder = new(Path + prevFolder);

            foreach (DirectoryInfo folder in rootFolder.GetDirectories())
            {
                Directory.CreateDirectory(MainForm.HelperSkin.Path + prevFolder + System.IO.Path.DirectorySeparatorChar + folder.Name);
                DirectoryInfo subFolder = new(Path + prevFolder + System.IO.Path.DirectorySeparatorChar + folder.Name);


                foreach (FileInfo file in subFolder.GetFiles())
                {
                    file.CopyTo(System.IO.Path.Combine(MainForm.HelperSkin.Path + prevFolder, folder.Name, file.Name), true);

                    if (OsuHelper.SpamLogs)
                        MainForm.DebugLog($"Copying \"{file.FullName}\" to \"{System.IO.Path.Combine(MainForm.HelperSkin.Path + prevFolder, folder.Name, file.Name)}\"", false);
                }

                if (subFolder.GetDirectories().Length != 0)
                    RecursiveSkinFolderMove(System.IO.Path.Combine(prevFolder, folder.Name));
            }
        }
    }
    #endregion
}