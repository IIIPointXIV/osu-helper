using System;
using System.Drawing;
using System.IO;

namespace osu_helper
{
    /// <summary>
    /// Abstract class that has some baseline skin functionality
    /// </summary>
    public abstract class Skin
    {
        /// <summary>
        /// The name of the skin.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The path used to get to the skin.
        /// </summary>
        public string Path { get; private set; }
        public static readonly Bitmap EmptyImage = new(1, 1);
        /// <summary>
        /// The path to the ini file of the skin.
        /// </summary>
        public string INIPath
        {
            get
            {
                return System.IO.Path.Combine(Path, "skin.ini");
            }
            private set { }
        }
        /// <summary>
        /// The path that a temp copy of the INIPath is copied to
        /// </summary>
        public string TempINIPath
        {
            get
            {
                return INIPath.Replace("skin.ini", "skin.ini.temp");
            }
            private set { }
        }

        #region Constructors

        /// <param name="path">The path that this skin will use.</param>
        public Skin(string path) : this(path, true) { }

        /// <param name="nameOrPath">The name or path that this skin will use.</param>
        /// <param name="isPath">If true, the skin given will be treated as a path.</param>
        public Skin(string nameOrPath, bool isPath)
        {
            if (isPath)
            {
                Path = nameOrPath;
                Name = GetNameFromPath(nameOrPath);
            }
            else
            {
                Path = GetPathFromName(nameOrPath);
                Name = nameOrPath;
            }
        }

        ~Skin()
        {
            EmptyImage.Dispose();
        }

        #endregion

        #region Get skin values

        /// <summary>
        /// Gets the name of the skin from the supplied path.
        /// </summary>
        /// <param name="path">The path that the skin has</param>
        /// <returns>The name of the associated skin.</returns>
        public static string GetNameFromPath(string path)
        {
            if (OsuHelper.OsuFolderPath == null)
                throw new ArgumentNullException("osuPath is null. Perhaps it was not instantiated?");

            return path.Replace(System.IO.Path.Combine(OsuHelper.OsuFolderPath, "skins") + System.IO.Path.DirectorySeparatorChar, "");
        }

        /// <summary>
        /// Gets the path of the skin from the name of the skin.
        /// </summary>
        /// <param name="name">The name of the skin.</param>
        /// <returns>The path of the skin.</returns>
        public static string GetPathFromName(string name) => System.IO.Path.Combine(OsuHelper.OsuFolderPath, "skins", name);

        /// <param name="fileName">The name of the file</param>
        /// <returns>The path of <paramref name="fileName"/></returns>
        public string GetPathOfFile(string fileName) => System.IO.Path.Combine(Path, fileName);

        public bool FileOfNameExists(string fileName) => File.Exists(GetPathOfFile(fileName));

        #endregion

        public static string GetSkinFolderPathFromName(string name) => System.IO.Path.Combine(OsuHelper.OsuSkinsFolderPath, name);
        
        public override bool Equals(object obj) => obj is Skin skin && Path == skin.Path;

        public override int GetHashCode() => HashCode.Combine(Name, Path);

        public override string ToString() => Path;
    }
}

