using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

using DominikStiller.VertretungsplanUploader.UI;

namespace DominikStiller.VertretungsplanUploader
{
    static class Program
    {
        // http://sanity-free.org/143/csharp_dotnet_single_instance_application.html
        static Mutex mutex = new Mutex(true, "{51D773FB-2AA8-4D37-8764-2DD0C88808B4}");

        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Window());
                mutex.ReleaseMutex();
            }
            else
            {
                BringToFront("Vertretungsplan-Uploader");
            }
        }

        // http://stackoverflow.com/a/2636915
        public static void BringToFront(string title)
        {
            IntPtr handle = FindWindow(null, title);

            if (handle == IntPtr.Zero)
            {
                return;
            }

            ShowWindow(handle, SW_SHOW);
            ShowWindow(handle, SW_RESTORE);
            SetForegroundWindow(handle);
        }

        [DllImport("user32", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

        [DllImport("user32")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        // http://stackoverflow.com/q/9099479/1867001
        [DllImport("user32")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_SHOW = 5;
        private const int SW_RESTORE = 9;

    }
}
