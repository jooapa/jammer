using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq;

namespace Jammer
{
    public static class Log
    {
        private static readonly List<string> log = new List<string>();
        private static readonly int maxLogEntries = 1000; // Prevent unbounded growth
        private static readonly object logLock = new object(); // Thread safety
        
        private static void New(string txt, bool isErr = false)
        {
            var time = DateTime.Now.ToString("HH:mm:ss"); // case sensitive

            var curPlaylist = Playlists.GetJammerPlaylistVisualPath(Utils.CurrentPlaylist);
            if (curPlaylist == "")
            {
                curPlaylist = "No playlist";
            }

            string logEntry;
            if (isErr)
            {
                logEntry = "[red]" + time + "[/]" + ";ERROR;[cyan]" + Start.Sanitize(curPlaylist) + "[/]: " + Start.Sanitize(txt);
            }
            else
            {
                logEntry = "[green3_1]" + time + "[/]" + ";INFO;[cyan]" + Start.Sanitize(curPlaylist) + "[/]: " + Start.Sanitize(txt);
            }
            
            lock (logLock)
            {
                log.Add(logEntry);
                
                // Implement log rotation: remove oldest entries when exceeding limit
                if (log.Count > maxLogEntries)
                {
                    log.RemoveAt(0); // Remove oldest entry
                }
            }
        }

        public static void Info(string txt)
        {
            New(txt);
        }

        public static void Error(string txt)
        {
            New(txt, true);
        }

        public static string GetLog()
        {
            lock (logLock)
            {
                return string.Join("\n", log);
            }
        }
    }

    public static class PerformanceMonitor
    {
        private static readonly List<string> performanceLog = new List<string>();
        private static readonly int maxPerformanceEntries = 500; // Limit performance log size
        private static readonly object perfLock = new object();
        private static readonly Process currentProcess = Process.GetCurrentProcess();
        private static DateTime lastLogTime = DateTime.MinValue;
        private static readonly int logIntervalSeconds = 10; // Log every 10 seconds
        
        // Performance counters
        private static long totalLoopIterations = 0;
        private static long totalKeyboardChecks = 0;
        private static DateTime startTime = DateTime.Now;
        private static readonly Stopwatch loopTimer = new Stopwatch();
        private static readonly List<double> loopTimings = new List<double>();
        
        public static void IncrementLoopIterations()
        {
            Interlocked.Increment(ref totalLoopIterations);
        }
        
        public static void IncrementKeyboardChecks()
        {
            Interlocked.Increment(ref totalKeyboardChecks);
        }
        
        public static void StartLoopTiming()
        {
            loopTimer.Restart();
        }
        
        public static void EndLoopTiming()
        {
            if (loopTimer.IsRunning)
            {
                loopTimer.Stop();
                lock (perfLock)
                {
                    loopTimings.Add(loopTimer.Elapsed.TotalMilliseconds);
                    // Keep only recent timings
                    if (loopTimings.Count > 100)
                    {
                        loopTimings.RemoveAt(0);
                    }
                }
            }
        }
        
        public static void LogPerformanceMetrics()
        {
            var now = DateTime.Now;
            if ((now - lastLogTime).TotalSeconds < logIntervalSeconds)
            {
                return; // Don't log too frequently
            }
            
            try
            {
                // Refresh process info
                currentProcess.Refresh();
                
                // Calculate metrics
                var uptimeSeconds = (now - startTime).TotalSeconds;
                var loopsPerSecond = uptimeSeconds > 0 ? totalLoopIterations / uptimeSeconds : 0;
                var keyboardChecksPerSecond = uptimeSeconds > 0 ? totalKeyboardChecks / uptimeSeconds : 0;
                
                // Memory metrics
                var workingSetMB = currentProcess.WorkingSet64 / (1024.0 * 1024.0);
                var privateMemoryMB = currentProcess.PrivateMemorySize64 / (1024.0 * 1024.0);
                var gcGen0 = GC.CollectionCount(0);
                var gcGen1 = GC.CollectionCount(1);
                var gcGen2 = GC.CollectionCount(2);
                
                // Loop timing stats
                double avgLoopTime = 0, maxLoopTime = 0, minLoopTime = 0;
                lock (perfLock)
                {
                    if (loopTimings.Count > 0)
                    {
                        avgLoopTime = loopTimings.Average();
                        maxLoopTime = loopTimings.Max();
                        minLoopTime = loopTimings.Min();
                    }
                }
                
                var perfEntry = $"[PERF] {now:HH:mm:ss} | " +
                              $"Loops/sec: {loopsPerSecond:F1} | " +
                              $"KB/sec: {keyboardChecksPerSecond:F1} | " +
                              $"Memory: {workingSetMB:F1}MB/{privateMemoryMB:F1}MB | " +
                              $"GC: {gcGen0}/{gcGen1}/{gcGen2} | " +
                              $"Loop: {avgLoopTime:F2}ms avg, {minLoopTime:F2}-{maxLoopTime:F2}ms | " +
                              $"Uptime: {uptimeSeconds:F0}s";
                
                lock (perfLock)
                {
                    performanceLog.Add(perfEntry);
                    
                    // Rotate log
                    if (performanceLog.Count > maxPerformanceEntries)
                    {
                        performanceLog.RemoveAt(0);
                    }
                }
                
                // Write directly to file for continuous monitoring only in debug mode
                if (Utils.IsDebug)
                {
                    try
                    {
                        var logPath = Path.Combine(Directory.GetCurrentDirectory(), "jammer_performance.log");
                        File.AppendAllText(logPath, perfEntry + "\n");
                    }
                    catch
                    {
                        // Ignore file write errors to prevent disrupting the main application
                    }
                }
                
                lastLogTime = now;
            }
            catch (Exception ex)
            {
                // Don't let performance monitoring crash the app
                Log.Error($"Performance monitoring error: {ex.Message}");
            }
        }
        
        public static string GetPerformanceLog()
        {
            lock (perfLock)
            {
                return string.Join("\n", performanceLog);
            }
        }
        
        public static void WritePerformanceLogToFile()
        {
            if (!Utils.IsDebug)
            {
                Log.Info("Performance logging is only available in debug mode. Use the -D flag to enable debug mode.");
                return;
            }
            
            try
            {
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "jammer_performance.log");
                var perfData = GetPerformanceLog();
                
                // Append to file with timestamp
                File.AppendAllText(logPath, $"\n=== Performance Log Session {DateTime.Now} ===\n{perfData}\n");
                
                Log.Info($"Performance log written to: {logPath}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to write performance log to file: {ex.Message}");
            }
        }
    }
}