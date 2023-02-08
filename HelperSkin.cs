
using System;
using System.IO;

public class HelperSkin : Skin
{
    public HelperSkin() : base(getPathForConstructor()) { }

    private static string getPathForConstructor()
    {
        if(Form1.managerFolderName == null)
            throw new ArgumentNullException("managerFolderName is null.");

        return Skin.getPathFromName(Form1.managerFolderName);
    }

    public void DeleteSkinElements()
    {
        DirectoryInfo rootFolder = new DirectoryInfo(path);

        foreach (FileInfo file in rootFolder.GetFiles())
        {
            if (Form1.spamLogs)
                Form1.DebugLog($"Deleting {file.FullName}", false);
            file.Delete();
        }


        foreach (DirectoryInfo folder in rootFolder.GetDirectories())
        {
            if (Form1.spamLogs)
                Form1.DebugLog($"Deleting {folder.FullName} (directory)", false);
            Directory.Delete(folder.FullName, true);
        }
    }
}