// using jammer;
// #if WINDOWS && USE_FORMS
// using Spectre.Console;
// using System.Diagnostics;
// using System;
// using System.Collections.Generic;
// using System.Drawing;
// using System.Threading;
// using System.Threading.Tasks;
// using System.Linq;
// using System.Windows.Forms;
// #endif

// class Program
// {
// #if WINDOWS && USE_FORMS
//     //! USE FOR WINDOWS BUILD
//     private static Task? formTask = null;
//     private static CancellationTokenSource formCancellationTokenSource = new();
//     public static readonly KeyboardHook hook = new();
//     static void OnProcessExit(object? sender, EventArgs e) {
//         Start.state = MainStates.pause;
//         System.Diagnostics.Debug.WriteLine("EXITING..");
//         Console.WriteLine("Exiting Jammer..."); // TODO ADD LOCALE

//         Console.WriteLine("Unhooking keyboard..."); // TODO ADD LOCALE
//         hook.UnhookKeyboard(); // Unhook the keyboard

//         Console.WriteLine("Canceling thread token..."); // TODO ADD LOCALE
//         formCancellationTokenSource.Cancel();

//         //Environment.FailFast("Threads closed..."); // TODO ADD LOCALE
//         System.Diagnostics.Debug.WriteLine("Closing threads...");
//         System.Diagnostics.Debug.WriteLine("Killing process...");
//         Process.GetCurrentProcess().Kill();
//         /*
//         Console.WriteLine("Waiting for thread to complete..."); // TODO ADD LOCALE
//         formTask?.Wait();

//         Console.WriteLine("Disposing thread..."); // TODO ADD LOCALE
//         formTask?.Dispose();
//         */
//     }

//     [STAThread]
//     static void Main(string[] args){
//         // Register the event handler for process exit
//         AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

//         // Start the form task as a background task
//         formTask = Task.Run(async () =>
//         {
//             try {
//                 ApplicationConfiguration.Initialize();
//                 await Task.Run(() => Application.Run(new Form1()), formCancellationTokenSource.Token);
//             } finally {
//                 formCancellationTokenSource.Cancel();
//             }
//         }, formCancellationTokenSource.Token);

//         System.Diagnostics.Debug.WriteLine("CONTINUING..");

//     #else
//     static void Main(string[] args){
//     #endif
//         string mutexName = "jammer";

//         using Mutex mutex = new Mutex(true, mutexName, out bool createdNew);
//         // If the mutex was successfully created, it means this is the first instance
//         if (createdNew)
//         {
//             Console.WriteLine("Launching Jammer...");
//             Start.Run(args);
//             // The program run after this point to the end, continuing inside with a new thread
//         }

// #if WINDOWS && USE_FORMS
//         // Wait for the form task to exit gracefully
//         formTask?.Wait();
// #endif
//     }
    
// }

using Avalonia;
using Avalonia.ReactiveUI;
using System;

namespace GetStartedApp;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}