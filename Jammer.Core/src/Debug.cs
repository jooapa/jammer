using System.IO;

namespace Jammer {
    public class Debug {
        public static void dprint(string txt) {
            if (Utils.isDebug) {
                using (StreamWriter writer = new("debug.log", true))
                {
                    var _method = new System.Diagnostics.StackTrace().GetFrame(1)?.GetMethod()?.Name;
                    var _class = new System.Diagnostics.StackTrace().GetFrame(1)?.GetMethod()?.DeclaringType?.Name;
                    var time = DateTime.Now.ToString("HH:mm:ss"); // case sensitive
                    if (Start.CLI) {
                    writer.WriteLine(time + ";" + _class + ";" + _method + ": " + txt.ToString());
                    } else {
                    
                        // TODO AVALONIA_UI
                    }
                }
            }
        }
    }
}
