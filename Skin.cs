using System;
using System.IO;

public abstract class Skin
{
    public string name { get; private set; }
    public string path { get; private set; }
    public string iniPath { get; private set; }

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

    public static string getNameFromPath(string path)
    {
        if (Form1.osuPath == null)
            throw new ArgumentNullException("osu path is null. Perhaps it was not instantiated?");

        return path.Replace(Path.Combine(Form1.osuPath, "skins") + "\\", "");
    }

    public static string getPathFromName(string name) => Path.Combine(Form1.osuPath, "skins") + "\\" + name;
    private string getINIPath(string path) => Path.Combine(path, "skin.ini");

    public override bool Equals(object obj) => obj is Skin skin && path == skin.path;

    public override int GetHashCode() => HashCode.Combine(name, path);

    public override string ToString() => path;
}