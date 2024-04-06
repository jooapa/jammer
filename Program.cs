using jammer;
#if WINDOWS
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Forms;
#endif
class Program
{
#if WINDOWS
    //! USE FOR WINDOWS BUILD
    private static Thread? formThread;
    public static readonly KeyboardHook hook = new();
    [STAThread]
    static void Main(string[] args)
    {
        formThread = new Thread(() =>
        {
            // Start the Windows Forms application within the thread
            ApplicationConfiguration.Initialize();

            Application.Run(new Form1());
        });

        // Start the formThread
        formThread.Start();
        
        System.Diagnostics.Debug.WriteLine("CONTINUING..");
        /*Thread.Sleep(5000);*/
    #else
    static void Main(string[] args)
    {
    #endif    
        string mutexName = "jammer";

        using (Mutex mutex = new Mutex(true, mutexName, out bool createdNew))
        {

            // If the mutex was successfully created, it means this is the first instance
            if (createdNew)
            {
                Console.WriteLine("Launching Jammer...");
                Start.Run(args);
            }
        }
    }
}

