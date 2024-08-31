using System;
using System.IO;
using G_Rewind.classes;

namespace G_Rewind
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Ensure directories exist
                Paths.EnsureDirectoriesExist();

                // Load the configuration
                Config config = Config.LoadOrCreateConfig(Paths.ConfigFilePath);


                // Initialize the processor with the loaded config
                GRewindProcessor processor = new GRewindProcessor(config);

                // Get all G-code files in the input directory
                string[] gcodeFiles = Directory.GetFiles(Paths.InputDirectory, "*.gcode");

                if (gcodeFiles.Length == 0)
                {
                    Console.WriteLine("No G-code files found in the input directory.");
                    return;
                }

                // Process each G-code file
                foreach (var filePath in gcodeFiles)
                {
                    Console.WriteLine($"Processing file: {Path.GetFileName(filePath)}");

                    // Process the GCode file
                    processor.ProcessGCode(filePath);

                    Console.WriteLine($"Processed file saved as: reversed_{Path.GetFileName(filePath)}");
                }

                Console.WriteLine("Processing complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}