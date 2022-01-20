using Microsoft.VisualBasic;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Runtime.ConstrainedExecution;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
public class Form1 : Form
{
    private TextBox osuPathBox;
    private Button changeOsuPathButton;
    private Button searchOsuSkinsButton;
    private Button deleteSkinButton;
    private Button changeToSkinButton;
    private System.Windows.Forms.OpenFileDialog openFileDialog1;
    private string osuPath;
    private Font mainFont;
    private ListBox osuSkinsListBox;
    private List<string> osuSkinsPathList = new List<string>();
    private string mainSkinPath;
    public void FormLayout()
    {
        mainFont = new Font("Segoe UI", 12);
        if(Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", "osuPath", null) == null)
        {
            osuPath = Environment.GetEnvironmentVariable("USERPROFILE") + "\\appdata\\Local\\osu!";
        }
        else
        {
            osuPath = Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", "osuPath", null).ToString();
        }
        mainSkinPath = osuPath + "\\skins\\!!!osu!helper Skin";

        //general form stuff
        this.MaximizeBox = false;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.Icon = new Icon(".\\Images\\o_h_kDs_icon.ico");
        this.Name = "osu!helper";
        this.Text = "osu!helper";
        Size = new Size(800, 800);

        //osuPathBox stuff
        this.osuPathBox = new System.Windows.Forms.TextBox();
        osuPathBox.Text = osuPath;
        osuPathBox.Width = 300;
        osuPathBox.Font = mainFont;
        Controls.Add(osuPathBox);

        //changeOsuPathButton stuff
        changeOsuPathButton = new System.Windows.Forms.Button();
        changeOsuPathButton.Click += new EventHandler(changeOsuPathButton_Click);
        changeOsuPathButton.Left = 302;
        changeOsuPathButton.Top = 3;
        changeOsuPathButton.Width = 115;
        changeOsuPathButton.Font = mainFont;
        changeOsuPathButton.Text = "osu! Directory";
        Controls.Add(changeOsuPathButton);

        //searchOsuSkinsButton
        searchOsuSkinsButton = new System.Windows.Forms.Button();
        searchOsuSkinsButton.Click += new EventHandler(SearchOsuSkins);
        searchOsuSkinsButton.Left = 420;
        searchOsuSkinsButton.Top = 3;
        searchOsuSkinsButton.Width = 90;
        searchOsuSkinsButton.Font = mainFont;
        searchOsuSkinsButton.Text = "Find Skins";
        Controls.Add(searchOsuSkinsButton);

        //osuSkinsListBox
        osuSkinsListBox = new ListBox();
        osuSkinsListBox.Size = new Size(700, 700);
        osuSkinsListBox.Font = mainFont;
        osuSkinsListBox.SelectionMode = SelectionMode.One;
        osuSkinsListBox.Top = 30;

        //deleteSkinButton
        deleteSkinButton = new System.Windows.Forms.Button();
        deleteSkinButton.Click += new EventHandler(DeleteSelectedSkin);
        deleteSkinButton.Left = 3;
        deleteSkinButton.Top = 737;
        deleteSkinButton.Width = 100;
        deleteSkinButton.Font = mainFont;
        deleteSkinButton.Text = "Delete Skin";
        deleteSkinButton.BackColor = Color.Red;
        Controls.Add(deleteSkinButton);

        //changeToSkinButton
        changeToSkinButton = new System.Windows.Forms.Button();
        changeToSkinButton.Click += new EventHandler(ChangeToSelectedSkin);
        changeToSkinButton.Left = 107;
        changeToSkinButton.Top = 737;
        changeToSkinButton.Width = 140;
        changeToSkinButton.Font = mainFont;
        changeToSkinButton.Text = "Change To Skin";
        Controls.Add(changeToSkinButton);

        //fileDialog stuff
        openFileDialog1 = new System.Windows.Forms.OpenFileDialog()
        {
            InitialDirectory =  osuPath,
            Filter = "Directory|*.directory",
            FileName =  "",
            Title = "Select osu! directory"
        };

        DirectoryInfo osuPathDI = new DirectoryInfo(osuPath + "\\skins");
        if(osuPathDI.Exists)
        {
            try
            {
                osuPathDI.CreateSubdirectory("!!!osu!helper Skin");
                osuPathDI.CreateSubdirectory("Deleted Skins");
            }
            catch
            {
                Console.WriteLine("Error Creating SubDirectories");
            }
        }
    }
    
    private void changeOsuPathButton_Click(object sender, EventArgs e)
    {
        FolderBrowserDialog directorySelector = new FolderBrowserDialog();
            directorySelector.ShowNewFolderButton = false;
            directorySelector.RootFolder = Environment.SpecialFolder.MyComputer;
            directorySelector.Description = "Select an osu! root directory:";
            directorySelector.SelectedPath = osuPath;

        DialogResult givenPath = directorySelector.ShowDialog();
        if (givenPath == DialogResult.OK)
        {
            //check if osu!.exe is present
            if (!File.Exists(directorySelector.SelectedPath + "\\osu!.exe"))
            {
                MessageBox.Show("Not a valid osu! directory!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                changeOsuPathButton_Click(sender, e);
                directorySelector.Dispose();
                return;
            }
            SetRegOsuPath(directorySelector.SelectedPath);
            directorySelector.Dispose();
        }
        //DialogResult result = openFileDialog1.ShowDialog();
    }

    private void SetRegOsuPath(string path)
    {
        osuPathBox.Text = path;
        Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\osuHelper", "osuPath", path);
    }

    private void SearchOsuSkins(object sender, EventArgs e)
    {
        osuSkinsListBox.BeginUpdate();
        osuSkinsListBox.Items.Clear();
        osuSkinsPathList.Clear();
        DirectoryInfo di = new DirectoryInfo(osuPath + "\\skins");
        var osuSkins = di.GetDirectories();
        foreach(DirectoryInfo skin in osuSkins)
        {
            string skinName = skin.FullName.Replace(osuPath + "\\skins\\", "");

            if(skinName != "!!!osu!helper Skin")
            {
                osuSkinsPathList.Add(skin.FullName);
                osuSkinsListBox.Items.Add(skinName);
                //Console.WriteLine(skinName);
            }
        }
        osuSkinsListBox.EndUpdate();
        Controls.Add(osuSkinsListBox);
    }

    private void DeleteSelectedSkin(object sender, EventArgs e)
    {
        try
        {
            string skinPath = osuSkinsPathList[osuSkinsListBox.SelectedIndex];
            Directory.Move(skinPath, osuPath + "\\skins\\Deleted Skins\\" + osuSkinsListBox.SelectedItem);
        }
        catch
        {
            Console.WriteLine("Find skins first. Error occurred when trying to delete skin");
        }
        osuSkinsPathList.RemoveAt(osuSkinsListBox.SelectedIndex);
        osuSkinsListBox.Items.RemoveAt(osuSkinsListBox.SelectedIndex);
    }

    private void ChangeToSelectedSkin(object sender, EventArgs e)
    {
        DeleteSkinElementsInMainSkin();
        string skinPath = osuSkinsPathList[osuSkinsListBox.SelectedIndex];
        DirectoryInfo di = new DirectoryInfo(skinPath);
        
        foreach(FileInfo file in di.GetFiles())
        {
            file.CopyTo(mainSkinPath + "\\" + file.Name, true);
        }
    }

    private void DeleteSkinElementsInMainSkin()
    {
        DirectoryInfo di = new DirectoryInfo(mainSkinPath);
        
        foreach(FileInfo file in di.GetFiles())
        {
            try
            {
                file.Delete();
            }
            catch{}
        }
    }
}