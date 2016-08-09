using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace RegistrySearch
{
    static class Program
    {
        static internal Form1 mainForm;

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            mainForm = new Form1();
            Application.Run(mainForm);
        }
    }
}
