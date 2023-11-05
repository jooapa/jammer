using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace jammer
{
    public class CheckJammerFolder
    {
        static public void CheckJammerFolderExists()
        {
            string jammerPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/jammer";
            if (!System.IO.Directory.Exists(jammerPath))
            {
                System.IO.Directory.CreateDirectory(jammerPath);
            }
        }
    }
}