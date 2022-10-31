using System;
using System.Windows.Forms;

namespace osu_helper
{
    public class Program
    {
        public static Form1 form = new Form1();
        [STAThread]
        static void Main(string[] args)
        {
            form.FormLayout((args.Length != 0 ? bool.Parse(args[0]) : false), (args.Length == 2 ? bool.Parse(args[1]) : false));
            Application.Run(form);
        }
    }
}