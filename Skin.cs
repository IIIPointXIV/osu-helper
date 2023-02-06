using System;
using System.IO;

public class Skin
{
    public string name { get; private set; }
    public string path { get; private set; }
    public string iniPath { get; private set; }

    public Skin(string path) : this(path, true)
    {

    }

    public Skin(string nameOrPath, bool isPath)
    {
        if (isPath)
        {
            this.path = path;
            this.name = getNameFromPath(path);
        }
        else
        {
            this.path = getPathFromName(nameOrPath);
            this.name = nameOrPath;
        }
        this.iniPath = getINIPath(nameOrPath);
    }

    private string getINIPath(string path)
    {
        return Path.Combine(path, "skin.ini");
    }

    public static string getNameFromPath(string path)
    {
        return path.Replace(Path.Combine(Form1.osuPath, "skins") + "\\", "");
    }

    public static string getPathFromName(string name)
    {
        return Path.Combine(Form1.osuPath, "skins") + "\\" + name;
    }

    public override bool Equals(object obj)
    {
        return obj is Skin skin && name == skin.name && path == skin.path;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(name, path);
    }
}