using System.Runtime.ConstrainedExecution;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
public class Form1 : Form
{
    private TextBox textBox1;
    private Button button1;
    private OpenFileDialog openFileDialog1;
    private string osuPath;
    private Font mainFont;
    public void FormLayout()
    {
        mainFont = new Font("Segoe UI", 12);
        osuPath = Environment.GetEnvironmentVariable("USERPROFILE") + "\\appdata\\Local\\osu!";

        //general form stuff
        this.MaximizeBox = false;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.Icon = new Icon(".\\Images\\o_h_kDs_icon.ico");
        this.Name = "osu!helper";
        this.Text = "osu!helper";
        Size = new Size(800, 800);


        //textbox1 stuff
        this.textBox1 = new System.Windows.Forms.TextBox();
        textBox1.Text = osuPath;
        textBox1.Width = 300;
        textBox1.Font = mainFont;
        Controls.Add(textBox1);

        //button1 stuff
        button1 = new System.Windows.Forms.Button();
        button1.Click += new EventHandler(button1_Click);
        button1.Left = 302;
        button1.Top = 3;
        button1.Width = 115;
        button1.Font = mainFont;
        button1.Text = "osu! directory";
        Controls.Add(button1);

        //fileDialog stuff
        openFileDialog1 = new OpenFileDialog()
        {
            InitialDirectory =  osuPath,
            Filter = "Directory|*.directory",
            FileName =  "",
            Title = "Select osu! directory"
        };
    }
    
    private void button1_Click(object sender, EventArgs e)
    {
        FolderBrowserDialog folder = new FolderBrowserDialog();
            folder.ShowNewFolderButton = false;
            folder.RootFolder = Environment.SpecialFolder.MyComputer;
            folder.Description = "Select an osu! root directory:";
            folder.SelectedPath = osuPath;
        DialogResult path = folder.ShowDialog();
        if (path == DialogResult.OK)
        {
            //check if osu!.exe is present
            if (!File.Exists(folder.SelectedPath + "\\osu!.exe"))
            {
                MessageBox.Show("Not a valid osu! directory!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                button1_Click(sender, e);
                return;
            }
        }
        //DialogResult result = openFileDialog1.ShowDialog();
    }
}