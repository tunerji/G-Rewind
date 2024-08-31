using System;
using System.IO;

namespace G_Rewind.classes
{
    public static class Paths
    {
        public static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string ConfigDirectory = Path.Combine(BaseDirectory, "config");  // Moved above g-codes
        public static readonly string InputDirectory = Path.Combine(BaseDirectory, "g-codes", "input");
        public static readonly string OutputDirectory = Path.Combine(BaseDirectory, "g-codes", "output");
        public static readonly string ResumeDirectory = Path.Combine(BaseDirectory, "g-codes", "resume");
        public static readonly string ConfigFilePath = Path.Combine(ConfigDirectory, "config.json");

        public static void EnsureDirectoriesExist()
        {
            Directory.CreateDirectory(ConfigDirectory);
            Directory.CreateDirectory(InputDirectory);
            Directory.CreateDirectory(OutputDirectory);
            Directory.CreateDirectory(ResumeDirectory);
        }
    }
}