using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class Skin
{
    public string name { get; protected set; }
    public string path { get; protected set; }
    public string iniPath { get; protected set; }

    public Skin(string path) : this(path, true) { }

    public Skin(string nameOrPath, bool isPath)
    {
        if (isPath)
        {
            this.path = nameOrPath;
            this.name = getNameFromPath(nameOrPath);
        }
        else
        {
            this.path = getPathFromName(nameOrPath);
            this.name = nameOrPath;
        }
        this.iniPath = getINIPath(nameOrPath);
    }

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

    public void RevertSkinIni(string searchFor)
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

    public void EditSkinIni(string searchFor, string replaceWith, string fallBackSearch)
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

    public void DisableCursorTrail(CheckState showState)
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

    public void CopyFilesToHelperSkin(string[] fileNames)
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
    private void RecursiveSkinFolderMove(string prevFolder)
    {
        DirectoryInfo rootFolder = new DirectoryInfo(path + prevFolder);

        foreach (DirectoryInfo folder in rootFolder.GetDirectories())
        {
            Directory.CreateDirectory(Form1.helperSkin.path + prevFolder + "\\" + folder.Name);
            DirectoryInfo subFolder = new DirectoryInfo(path + prevFolder + "\\" + folder.Name);


            foreach (FileInfo file in subFolder.GetFiles())
            {
                file.CopyTo(Form1.helperSkin.path + prevFolder + "\\" + folder.Name + "\\" + file.Name, true);
                if (Form1.spamLogs)
                    Form1.DebugLog($"Copying \"{file.FullName}\" to \"{Path.Combine(Form1.helperSkin.path + prevFolder, folder.Name, file.Name)}\"", false);
            }

            if (subFolder.GetDirectories().Length != 0)
                RecursiveSkinFolderMove(prevFolder + "\\" + folder.Name);
        }
    }

    private string getINIPath(string path) => Path.Combine(path, "skin.ini");

    public static string getNameFromPath(string path)
    {
        if (Form1.osuPath == null)
            throw new ArgumentNullException("osu path is null. Perhaps it was not instantiated?");

        return path.Replace(Path.Combine(Form1.osuPath, "skins") + "\\", "");
    }

    public static string getPathFromName(string name) => Path.Combine(Form1.osuPath, "skins") + "\\" + name;

    public override bool Equals(object obj) => obj is Skin skin && path == skin.path;

    public override int GetHashCode() => HashCode.Combine(name, path);

    public override string ToString() => path;
}