using System;
using System.IO;

/// <summary>
/// Abstract class that has some baseline skin functionality
/// </summary>
public abstract class Skin
{
    /// <summary>
    /// The name of the skin.
    /// </summary>
    public string name { get; private set; }
    /// <summary>
    /// The path used to get to the skin.
    /// </summary>
    public string path { get; private set; }
    /// <summary>
    /// The path to the ini file of the skin.
    /// </summary>
    public string iniPath { get; private set; }

    #region Constructors

    /// <param name="path">The path that this skin will use.</param>
    public Skin(string path) : this(path, true) { }

    /// <param name="nameOrPath">The name or path that this skin will use.</param>
    /// <param name="isPath">If true, the skin given will be treated as a path.</param>
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

    #endregion

    #region Get skin values
    
    /// <summary>
    /// Gets the name of the skin from the supplied path.
    /// </summary>
    /// <param name="path">The path that the skin has</param>
    /// <returns>The name of the associated skin.</returns>
    public static string getNameFromPath(string path)
    {
        if (Form1.osuPath == null)
            throw new ArgumentNullException("osu path is null. Perhaps it was not instantiated?");

        return path.Replace(Path.Combine(Form1.osuPath, "skins") + Path.DirectorySeparatorChar, "");
    }

    /// <summary>
    /// Gets the path of the skin from the name of the skin.
    /// </summary>
    /// <param name="name">The name of the skin.</param>
    /// <returns>The path of the skin.</returns>
    public static string getPathFromName(string name) => Path.Combine(Form1.osuPath, "skins", name);

    /// <summary>
    /// Gets the path of the ini of the skin.
    /// </summary>
    /// <param name="path">The path of the skin.</param>
    /// <returns>The path of the ini.</returns>
    private string getINIPath(string path) => Path.Combine(path, "skin.ini");

    #endregion

    public override bool Equals(object obj) => obj is Skin skin && path == skin.path;

    public override int GetHashCode() => HashCode.Combine(name, path);

    public override string ToString() => path;
}