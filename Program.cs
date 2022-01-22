using System;
using System.Windows.Forms;
using System.Drawing;

namespace osu_helper
{
    public class Program
    {
        public static Form1 form = new Form1();
        [STAThread]
        static void Main()
        {
            form.FormLayout();
            Application.Run(form);
        }
    }
}