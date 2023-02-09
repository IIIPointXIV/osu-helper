using System;
using System.IO;

/// <summary>
/// Class used to store the Helper skin information
/// </summary>
/// <remarks>
/// Should only have one object made.
/// </remarks>
public sealed class HelperSkin : Skin
{
    private static HelperSkin instance;

    public static HelperSkin Instance
    {
        get
        {
            if (instance == null)
                instance = new HelperSkin();
            return instance;
        }
    }

    private HelperSkin() : base(getPath()) { }

    /// <summary>
    /// Gets the path of the helper skin.
    /// </summary>
    /// <returns>The path of the helper skin.</returns>
    private static string getPath()
    {
        if (Form1.managerFolderName == null)
            throw new ArgumentNullException("managerFolderName is null.");

        return Skin.getPathFromName(Form1.managerFolderName);
    }

    /// <summary>
    /// Deletes all files in the helper skin
    /// </summary>
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