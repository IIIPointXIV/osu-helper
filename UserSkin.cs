using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

/// <summary>
/// Made of the skin folder that the user would use. Has the ability to alter itself.
/// </summary>
public class UserSkin : Skin
{
    #region Constructors
    /// <param name="path">The path that this skin will use.</param>
    public UserSkin(string path) : base(path)
    {
    }

    /// <param name="nameOrPath">The name or path that this skin will use.</param>
    /// <param name="isPath">If true, the skin given will be treated as a path.</param>
    public UserSkin(string nameOrPath, bool isPath) : base(nameOrPath, isPath)
    {
    }

    #endregion
    
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
        Form1.DebugLog($"Attempting to restore {searchFor}. In the skin.ini", false);
        StreamReader origINIReader = new StreamReader(iniPath);
        string currLine;

        while ((currLine = origINIReader.ReadLine()) != null)
        {
            if (currLine.ToLower().Contains(searchFor.ToLower()))
                break;
        }

        origINIReader.Dispose();

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
        Form1.DebugLog($"Attempting to search for {searchFor}, replace it with {replaceWith}, with a fallback of {fallBackSearch}. In the skin.ini", false);
        /* if(File.Exists(Form1.helperSkin.iniPath.Replace("skin.ini", "skin.ini.temp")))
            File.Delete(Form1.helperSkin.iniPath.Replace("skin.ini", "skin.ini.temp")); */

        File.Copy(Form1.helperSkin.iniPath, Form1.helperSkin.iniPath.Replace("skin.ini", "skin.ini.temp"));
        StreamReader reader = new StreamReader(Form1.helperSkin.iniPath.Replace("skin.ini", "skin.ini.temp"));
        StreamWriter writer = new StreamWriter(Form1.helperSkin.iniPath);
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
        reader.Dispose();
        writer.Dispose();

        if (!lineFound)
        {
            StreamWriter writerNew = new StreamWriter(Form1.helperSkin.iniPath);
            StreamReader readerNew = new StreamReader(Form1.helperSkin.iniPath.Replace("skin.ini", "skin.ini.temp"));

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
            writerNew.Dispose();
            readerNew.Dispose();
        }

        File.Delete(Form1.helperSkin.iniPath.Replace("skin.ini", "skin.ini.temp"));
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
        Form1.DebugLog($"ShowSliderEnds({showState}) called", false);

        if (showState == CheckState.Indeterminate)
            return;

        bool show = (showState == CheckState.Checked);


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

                using (Image image = (File.Exists(Path.Combine(path, fileName)) ? Image.FromFile(Path.Combine(path, fileName)) : null))
                {
                    if (File.Exists(Path.Combine(path, fileName)) && (image == null ? true : image.Size.Height < 100))
                    {
                        File.Copy(Path.Combine(path, fileName), Path.Combine(Form1.helperSkin.path, fileName), true);
                        if (Form1.spamLogs)
                            Form1.DebugLog($"Copying {Path.Combine(path, fileName)} to {Path.Combine(Form1.helperSkin.path, fileName)}", false);
                    }
                    else if (File.Exists(Path.Combine(path, fileName.Replace("end", "start"))))
                    {
                        File.Copy(Path.Combine(path, fileName.Replace("sliderendcircle", "sliderstartcircle")), Path.Combine(Form1.helperSkin.path, fileName), true);
                        if (Form1.spamLogs)
                            Form1.DebugLog($"Copying {Path.Combine(path, fileName.Replace("sliderendcircle", "sliderstartcircle"))} to {Path.Combine(Form1.helperSkin.path, fileName)}", false);
                        if (!File.Exists(Path.Combine(path, fileName.Replace(".png", "@2x.png"))))
                        {
                            skipAt2X = fileName.Replace(".png", "@2x.png");
                        }
                    }
                    else if (File.Exists(Path.Combine(path, fileName.Replace("sliderend", "hit"))))
                    {
                        File.Copy(Path.Combine(path, fileName.Replace("sliderend", "hit")), Path.Combine(Form1.helperSkin.path, fileName), true);
                        if (Form1.spamLogs)
                            Form1.DebugLog($"Copying {Path.Combine(path, fileName.Replace("sliderend", "hit"))} to {Path.Combine(Form1.helperSkin.path, fileName)}", false);
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
                emptyImage.Save(Path.Combine(Form1.helperSkin.path, fileName));
        }
        emptyImage.Dispose();
        sliderImage.Dispose();

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
        Form1.DebugLog($"ShowHitCircles({showState}) called", false);

        if (showState == CheckState.Indeterminate)
            showState = CheckState.Checked;

        bool show = (showState == CheckState.Checked);


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
                string helperSkinFilePath = Path.Combine(Form1.helperSkin.path, curName);
                emptyImage.Save(helperSkinFilePath);
                if (Form1.spamLogs)
                    Form1.DebugLog($"Copying empty image to {helperSkinFilePath}", false);
            }

            emptyImage.Dispose();
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
        Form1.DebugLog($"ShowHitCircleNumbers({showState}) called", false);

        if (showState == CheckState.Indeterminate)
            return;

        bool show = (showState == CheckState.Checked);


        if (show) //show skin numbers
        {
            File.Copy(Path.Combine(path, "skin.ini"), Path.Combine(Form1.helperSkin.path, "skin.ini"), true);
            Form1.DebugLog($"Copying {Path.Combine(path, "skin.ini")} to {Path.Combine(Form1.helperSkin.path, "skin.ini")}", false);
        }
        else //hide skin numbers
            EditSkinIni("HitCirclePrefix:", "HitCirclePrefix: 727", "[Fonts]");

    }

    #endregion

    #region Edit cursor

    /// <summary>
    /// Changes if the cursor expands or not
    /// </summary>
    /// <param name="showState">Checked: Cursor does expand
    /// Unchecked: Cursor does not expand
    /// Indeterminate: Reverts skin ini to original</param>
    public void ChangeExpandingCursor(CheckState expand)
    {
        Form1.DebugLog($"ChangeExpandingCursor({expand.ToString()}) called", false);

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
        Form1.DebugLog($"DisableCursorTrail({showState}) called", false);

        if (showState == CheckState.Indeterminate)
            return;

        bool show = (showState == CheckState.Checked);


        List<string> names = new List<string>()
        {
            "cursortrail@2x.png",
            "cursortrail.png",
        };

        if (show)
        {
            foreach (string name in names)
                if (File.Exists(Path.Combine(path, name)))
                {
                    File.Copy(Path.Combine(path, name), Path.Combine(Form1.helperSkin.path, name), true);
                    if (Form1.spamLogs)
                        Form1.DebugLog($"Copying {Path.Combine(path, name)} to {Path.Combine(Form1.helperSkin.path, name)}", false);
                }

        }
        else
        {
            Bitmap emptyImage = new Bitmap(1, 1);
            foreach (string name in names)
            {
                emptyImage.Save(Path.Combine(Form1.helperSkin.path, name));
                if (Form1.spamLogs)
                    Form1.DebugLog($"Saving empty image to {Path.Combine(Form1.helperSkin.path, name)}", false);
            }
            emptyImage.Dispose();
        }

    }

    #endregion

    #region Edit other things

    /// <summary>
    /// Shows or hides hit-lighting
    /// </summary>
    /// <param name="showState">Checked: Shows hit lighting
    /// Unchecked: Hides hit lighting
    /// Indeterminate: Reverts skin hit lighting to original</param>
    public void ShowHitLighting(CheckState showState)
    {
        Form1.DebugLog($"ShowHitLighting({showState}) called", false);


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
                currentSkinFilePath = Path.Combine(path, curFileName);
                helperSkinFilePath = Path.Combine(Form1.helperSkin.path, curFileName);

                if (File.Exists(currentSkinFilePath))
                {
                    Image thisImg = Image.FromFile(currentSkinFilePath);
                    if (thisImg.Height > 100)
                    {
                        File.Copy(currentSkinFilePath, helperSkinFilePath, true);
                        thisImg.Dispose();
                        if (Form1.spamLogs)
                            Form1.DebugLog($"Copying {currentSkinFilePath} to {helperSkinFilePath}", false);

                        continue;
                    }
                    thisImg.Dispose();
                    File.Delete(helperSkinFilePath);
                    if (Form1.spamLogs)
                        Form1.DebugLog($"Deleting {helperSkinFilePath}", false);
                }
                else if (File.Exists(helperSkinFilePath))
                {
                    File.Delete(helperSkinFilePath);
                    if (Form1.spamLogs)
                        Form1.DebugLog($"Deleting {helperSkinFilePath}", false);
                }
            }
        }
        else
        {
            Image emptyImage = new Bitmap(1, 1);
            string helperSkinFilePath;
            foreach (string name in fileNames)
            {
                helperSkinFilePath = Path.Combine(Form1.helperSkin.path, name);

                emptyImage.Save(helperSkinFilePath);
                if (Form1.spamLogs)
                    Form1.DebugLog($"Saving empty image to {helperSkinFilePath}", false);
            }
            emptyImage.Dispose();
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
        Form1.DebugLog($"ShowComboBursts({showState}) called", false);

        if (showState == CheckState.Indeterminate)
            showState = CheckState.Unchecked;

        bool show = (showState == CheckState.Checked);


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
            if (!File.Exists(Path.Combine(Form1.helperSkin.path, name + ".png")))
            {
                emptyImage.Save(Path.Combine(Form1.helperSkin.path, name + ".png"));
                if (Form1.spamLogs)
                    Form1.DebugLog($"Saving empty image to {Path.Combine(Form1.helperSkin.path, name + ".png")}", false);
            }
        }

        //incase there are multiple combobursts
        DirectoryInfo di = new DirectoryInfo(path);
        foreach (FileInfo file in di.GetFiles())
        {
            foreach (string name in fileNames)
                if (file.Name.Contains(name))
                {
                    if (show)
                        File.Copy(file.FullName, Path.Combine(Form1.helperSkin.path, file.Name), true);
                    else
                        emptyImage.Save(Path.Combine(Form1.helperSkin.path, file.Name));

                    if (Form1.spamLogs)
                        Form1.DebugLog($"Saving empty image to {Path.Combine(Form1.helperSkin.path, file.Name)}", false);
                }
        }
        emptyImage.Dispose();

    }

    #endregion

    #region Copy to helper skin

    /// <summary>
    /// Copies all files in <paramref name="fileNames"/> to the helper skin.
    /// </summary>
    /// <param name="fileNames">The files to be copied</param>
    private void CopyFilesToHelperSkin(string[] fileNames)
    {
        foreach (string curName in fileNames)
        {
            string currentSkinFilePath = Path.Combine(path, curName);
            string helperSkinFilePath = Path.Combine(Form1.helperSkin.path, curName);
            if (File.Exists(currentSkinFilePath))
            {
                File.Copy(currentSkinFilePath, helperSkinFilePath, true);
                if (Form1.spamLogs)
                    Form1.DebugLog($"Copying {currentSkinFilePath} to {helperSkinFilePath}", false);
            }
        }
    }

    /// <summary>
    /// Changes the helper skin to this skin.
    /// </summary>
    public void ChangeToSkin()
    {
        DirectoryInfo workingSkinPathDi = new DirectoryInfo(path);

        foreach (FileInfo currentFile in workingSkinPathDi.GetFiles())
        {
            currentFile.CopyTo(Path.Combine(Form1.helperSkin.path, currentFile.Name), true);
            if (Form1.spamLogs)
                Form1.DebugLog($"Copying \"{currentFile.FullName}\" to \"{Path.Combine(Form1.helperSkin.path, currentFile.Name)}\"", false);
        }
        RecursiveSkinFolderMove("\\");
    }

    /// <summary>
    /// Recursively copies the entire folder tree to the helper skin.
    /// </summary>
    /// <param name="prevFolder">The path of the previous folders</param>
    private void RecursiveSkinFolderMove(string prevFolder)
    {
        DirectoryInfo rootFolder = new DirectoryInfo(path + prevFolder);

        foreach (DirectoryInfo folder in rootFolder.GetDirectories())
        {
            Directory.CreateDirectory(Form1.helperSkin.path + prevFolder + Path.DirectorySeparatorChar + folder.Name);
            DirectoryInfo subFolder = new DirectoryInfo(path + prevFolder + Path.DirectorySeparatorChar + folder.Name);


            foreach (FileInfo file in subFolder.GetFiles())
            {
                file.CopyTo(Form1.helperSkin.path + prevFolder + Path.DirectorySeparatorChar + folder.Name + Path.DirectorySeparatorChar + file.Name, true);
                if (Form1.spamLogs)
                    Form1.DebugLog($"Copying \"{file.FullName}\" to \"{Path.Combine(Form1.helperSkin.path + prevFolder, folder.Name, file.Name)}\"", false);
            }

            if (subFolder.GetDirectories().Length != 0)
                RecursiveSkinFolderMove(prevFolder + Path.DirectorySeparatorChar + folder.Name);
        }
    }

    #endregion
}