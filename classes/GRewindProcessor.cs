using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace G_Rewind.classes
{
    internal class GRewindProcessor
    {
        private Config _config;

        // Constructor that accepts a Config object
        public GRewindProcessor(Config config)
        {
            _config = config;
        }


        // Method to process a single G-code file
        //public void ProcessGCode(string inputFilePath)
        //{
        //    try
        //    {
        //        // Read all lines from the input G-code file
        //        List<string> lines = File.ReadAllLines(inputFilePath).ToList();

        //        // Remove Cura initialization blocks if present
        //        lines = RemoveCuraInitBlocks(lines);

        //        // Identify and process the start, motion, and end blocks
        //        var (startBlock, motionBlock, endBlock) = SeparateBlocks(lines);

        //        // Remove extrusion commands from the motion block
        //        motionBlock = RemoveExtrusionCommands(motionBlock);

        //        // Tag and trim the motion block
        //        motionBlock = TagMotionBlock(motionBlock);
        //        motionBlock = TrimMotionBlocks(motionBlock);



        //        // Remove redundant F and Z commands before reversing
        //        motionBlock = RemoveRedundantFeedRateCommands(motionBlock);

        //        // Remove redundant Z Coordinates before reversing
        //         motionBlock = RemoveRedundantZCoordinates(motionBlock);



        //        // Clean up any lines containing ";End G-code" in the motion block
        //        motionBlock.RemoveAll(line => line.Contains(";End G-code"));

        //        // Reverse the motion block after trimming and removing redundancies
        //        motionBlock.Reverse();

        //        // Combine the start block and reversed motion block (end block is not included in the final output)
        //        List<string> finalGCodeLines = new List<string>();
        //        finalGCodeLines.AddRange(startBlock);
        //        finalGCodeLines.AddRange(motionBlock);

        //        // Generate the output file path
        //        string outputFileName = $"reversed_{Path.GetFileName(inputFilePath)}";
        //        string outputFilePath = Path.Combine(Paths.OutputDirectory, outputFileName);

        //        // Ensure the output directory exists
        //        if (!Directory.Exists(Paths.OutputDirectory))
        //        {
        //            Directory.CreateDirectory(Paths.OutputDirectory);
        //        }

        //        // Save the processed G-code to the output file
        //        File.WriteAllLines(outputFilePath, finalGCodeLines);
        //        Console.WriteLine($"Processed file saved as: {outputFilePath}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"An error occurred while processing the file: {ex.Message}");
        //    }
        //}

        public void ProcessGCode(string inputFilePath)
        {
            try
            {
                // Read all lines from the input G-code file
                List<string> lines = File.ReadAllLines(inputFilePath).ToList();

                // Remove Cura initialization blocks if present
                lines = RemoveCuraInitBlocks(lines);

                // Identify and process the start, motion, and end blocks
                var (startBlock, motionBlock, endBlock) = SeparateBlocks(lines);

                // Remove extrusion commands from the motion block
                motionBlock = RemoveExtrusionCommands(motionBlock);

                // Tag the motion block
                motionBlock = TagMotionBlock(motionBlock);

                // Trim the motion block based on user-defined Z limits
                motionBlock = TrimMotionBlocks(motionBlock);

                // Insert the safe Z move before reversing the motion block
                InsertSafeZMove(startBlock, motionBlock);

                // Remove redundant F and Z commands
                motionBlock = RemoveRedundantFeedRateCommands(motionBlock);
                motionBlock = RemoveRedundantZCoordinates(motionBlock);

                // Clean up any lines containing ";End G-code" in the motion block
                motionBlock.RemoveAll(line => line.Contains(";End G-code"));

                // Reverse the motion block after trimming and removing redundancies
                motionBlock.Reverse();

                // Combine the start block and reversed motion block (end block is not included in the final output)
                List<string> finalGCodeLines = new List<string>();
                finalGCodeLines.AddRange(startBlock);
                finalGCodeLines.AddRange(motionBlock);

                // Generate the output file path
                string outputFileName = $"reversed_{Path.GetFileName(inputFilePath)}";
                string outputFilePath = Path.Combine(Paths.OutputDirectory, outputFileName);

                // Ensure the output directory exists
                if (!Directory.Exists(Paths.OutputDirectory))
                {
                    Directory.CreateDirectory(Paths.OutputDirectory);
                }

                // Save the processed G-code to the output file
                File.WriteAllLines(outputFilePath, finalGCodeLines);
                Console.WriteLine($"Processed file saved as: {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while processing the file: {ex.Message}");
            }
        }


        private void InsertSafeZMove(List<string> startBlock, List<string> motionBlock)
        {
            // Find the maximum Z value in the motion block
            decimal maxZInGCode = _config.UserDefinedBottomZ; // Start with the user-defined bottom Z as the minimum
            foreach (var line in motionBlock)
            {
                decimal zValue = ExtractCoordinate(line, "Z", _config.UserDefinedBottomZ);
                if (zValue > maxZInGCode)
                {
                    maxZInGCode = zValue;
                }
            }

            // Calculate the safe Z height by adding the configured SafeZOffset to the maximum Z value in the G-code
            decimal safeZ = maxZInGCode + _config.SafeZOffset;

            // Create the G-code command to move to the safe Z height
            string safeZMoveCommand = $"G1 Z{safeZ.ToString(CultureInfo.InvariantCulture)} F3000 ; Raise Z to safe height";

            // Insert the safe Z move command right before the first motion command in the reversed motion block
            startBlock.Add(safeZMoveCommand);
        }



        // Method to separate the start, motion, and end blocks
        private (List<string> startBlock, List<string> motionBlock, List<string> endBlock) SeparateBlocks(List<string> lines)
        {
            List<string> startBlock = new List<string>();
            List<string> motionBlock = new List<string>();
            List<string> endBlock = new List<string>();

            int firstMotionIndex = -1;
            int lastMotionIndex = -1;

            // Analyze the entire G-code to find motion blocks
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                if (ContainsMotionAndCoordinates(line))
                {
                    if (firstMotionIndex == -1)
                    {
                        firstMotionIndex = i; // Mark the beginning of the motion block
                    }
                    lastMotionIndex = i; // Update the end of the motion block as we move forward
                }
            }

            // If no motion block found, return all lines in the startBlock and empty motion/end blocks
            if (firstMotionIndex == -1 || lastMotionIndex == -1)
            {
                startBlock = lines;
                return (startBlock, motionBlock, endBlock);
            }

            // Separate the blocks based on identified indices
            startBlock = lines.Take(firstMotionIndex).ToList();
            motionBlock = lines.Skip(firstMotionIndex).Take(lastMotionIndex - firstMotionIndex + 1).ToList();
            endBlock = lines.Skip(lastMotionIndex + 1).ToList();

            // Handle special case for Layer 0 comment
            for (int i = startBlock.Count - 1; i >= 0; i--)
            {
                if (startBlock[i].StartsWith(";LAYER:0"))
                {
                    // Move the LAYER:0 comment to the start of the motion block
                    motionBlock.Insert(0, startBlock[i]);
                    startBlock.RemoveAt(i);
                    break; // We only need to move the first occurrence
                }
            }

            return (startBlock, motionBlock, endBlock);
        }

        private bool ContainsMotionAndCoordinates(string line)
        {
            bool containsMotion = _config.MotionCommands.Any(cmd => line.StartsWith(cmd));
            bool containsCoordinate = _config.CoordinateCommands.Any(coord => line.Contains(coord));

            return containsMotion && containsCoordinate;
        }


        // Method to check if a line is part of the end G-code block
        private bool IsEndGCode(string line)
        {
            return line.StartsWith("M104") || line.StartsWith("M140") || line.StartsWith("G28") || line.StartsWith("M84");
        }

        // Method to tag motion block lines with Z and F values
        private List<string> TagMotionBlock(List<string> motionBlock)
        {
            decimal currentZ = 0.0M;
            decimal currentF = 0.0M;

            for (int i = 0; i < motionBlock.Count; i++)
            {
                var line = motionBlock[i];

                // Extract the current Z and F values from the line if they exist
                currentZ = ExtractCoordinate(line, "Z", currentZ);
                currentF = ExtractCoordinate(line, "F", currentF);

                // Split the line into the G-code part and the comment part
                var parts = line.Split(';');
                var commandPart = parts[0].Trim();
                var commentPart = parts.Length > 1 ? ";" + parts[1].Trim() : "";

                // Reorder the line to have G command first, F second, and then the coordinates
                if (_config.MotionCommands.Any(cmd => commandPart.StartsWith(cmd)))
                {
                    var reorderedCommand = "";

                    // Extract the G command
                    var gCommandMatch = Regex.Match(commandPart, @"(G\d+)");
                    if (gCommandMatch.Success)
                    {
                        reorderedCommand = gCommandMatch.Value;
                    }

                    // Add the F value if present
                    if (commandPart.Contains("F"))
                    {
                        var fValueMatch = Regex.Match(commandPart, @"F[\d.]+");
                        if (fValueMatch.Success)
                        {
                            reorderedCommand += " " + fValueMatch.Value;
                        }
                    }
                    else
                    {
                        reorderedCommand += $" F{currentF.ToString(CultureInfo.InvariantCulture)}";
                    }

                    // Add the remaining coordinates (X, Y, Z, E)
                    var remainingCoordinates = Regex.Replace(commandPart, @"(G\d+|F[\d.]+)", "").Trim();
                    reorderedCommand += " " + remainingCoordinates.Trim();

                    // Ensure Z is added if it was missing
                    if (!reorderedCommand.Contains("Z"))
                    {
                        reorderedCommand += $" Z{currentZ.ToString(CultureInfo.InvariantCulture)}";
                    }

                    // Reconstruct the line with the updated command and the original comment
                    motionBlock[i] = reorderedCommand + " " + commentPart;
                }
            }

            return motionBlock;
        }

        private List<string> RemoveRedundantFeedRateCommands(List<string> motionBlock)
        {
            string? lastFeedRateCommand = null;
            string? lastGCommand = null; //  G0, G1..

            for (int i = motionBlock.Count - 1; i >= 0; i--)
            {
                var line = motionBlock[i];

                // Skip the line if it starts with a comment
                if (line.TrimStart().StartsWith(";"))
                {
                    continue;
                }

                string? currentGCommand = ExtractGCommand(line);
                string? currentFeedRateCommand = ExtractFCommand(line);

                if (currentGCommand is ("G0" or "G1"))
                {
                    if (currentGCommand == lastGCommand && currentFeedRateCommand != null && currentFeedRateCommand == lastFeedRateCommand)
                    {
                        // Remove the redundant feed rate command from the line
                        line = Regex.Replace(line, @"\s*F-?\d+(\.\d+)?", "").Trim();
                        motionBlock[i] = line; // Update the line in the list after removing the redundant F command
                    }
                    else
                    {
                        // Update the last known feed rate and G command
                        lastFeedRateCommand = currentFeedRateCommand;
                        lastGCommand = currentGCommand;
                    }
                }
                else
                {
                    // Reset tracking if the command is not G0 or G1
                    lastGCommand = null;
                    lastFeedRateCommand = null;
                }
            }

            return motionBlock;
        }
        private List<string> RemoveRedundantZCoordinates(List<string> motionBlock)
        {
            //string? lastFeedRateCommand = null;
            string? lastZCoordinate = null; //  G0, G1..

            for (int i = motionBlock.Count - 1; i >= 0; i--)
            {
                var line = motionBlock[i];

                // Skip the line if it starts with a comment
                if (line.TrimStart().StartsWith(";") || !line.Contains("Z"))
                {
                    continue;
                }

                string? currentZCoordinate = ExtractZCommand(line);

                if (currentZCoordinate != null)
                {
                    if (currentZCoordinate == lastZCoordinate)
                    {
                        // Remove the redundant feed rate command from the line
                        line = Regex.Replace(line, @"\s*Z-?\d+(\.\d+)?", "").Trim();
                        motionBlock[i] = line; // Update the line in the list after removing the redundant F command
                    }
                    else
                    {

                        lastZCoordinate = currentZCoordinate;
                    }
                }
                else
                {
                    // Reset tracking if the command is not G0 or G1
                    lastZCoordinate = null;
                }
            }

            return motionBlock;
        }
        private string ExtractGCommand(string line)
        {
            var match = Regex.Match(line, @"^G\d+");
            return match.Success ? match.Value : null;
        }
        private string ExtractFCommand(string line)
        {
            var match = Regex.Match(line, @"F-?\d+(\.\d+)?");
            return match.Success ? match.Value : null;
        }

        private string ExtractZCommand(string line)
        {
            var match = Regex.Match(line, @"Z-?\d+(\.\d+)?");
            return match.Success ? match.Value : null;
        }


        private List<string> RemoveCuraInitBlocks(List<string> lines)
        {
            bool insideInitBlock = false;
            bool initBlockRemoved = false; // Flag to indicate if we removed an init block
            List<string> cleanedLines = new List<string>();

            foreach (var line in lines)
            {
                if (line.Contains(";Initilization Start"))
                {
                    insideInitBlock = true;
                    initBlockRemoved = true; // Set flag to true since we're removing the block
                    continue;  // Skip this line and everything until the end of the init block
                }

                if (line.Contains(";Initilization End"))
                {
                    insideInitBlock = false;
                    continue;  // Skip this line as well
                }

                if (!insideInitBlock)
                {
                    cleanedLines.Add(line);
                }
            }

            // If we removed an initialization block, add a comment about it
            if (initBlockRemoved)
            {
                cleanedLines.Insert(0, "; Removed Cura initialization block");
            }

            return cleanedLines;
        }
        private List<string> RemoveExtrusionCommands(List<string> motionBlock)
        {
            List<string> cleanedMotionBlock = new List<string>();

            foreach (var line in motionBlock)
            {
                // Remove any E commands
                var cleanedLine = Regex.Replace(line, @"\s?E-?\d+(\.\d+)?", string.Empty);

                // Add the cleaned line if it's not empty after removing the E command
                if (!string.IsNullOrWhiteSpace(cleanedLine))
                {
                    cleanedMotionBlock.Add(cleanedLine);
                }
            }

            return cleanedMotionBlock;
        }

        private decimal ExtractCoordinate(string line, string axis, decimal defaultValue)
        {
            try
            {
                // Adjust regex to potentially handle very large numbers and optional signs
                var match = Regex.Match(line, $@"\b{axis}(-?\d+(\.\d+)?)\b");
                if (match.Success)
                {
                    return decimal.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting {axis} value from line '{line}': {ex.Message}");
            }
            return defaultValue;
        }

        // Method to trim motion blocks based on user-defined Z limits
        private List<string> TrimMotionBlocks(List<string> motionBlock)
        {
            decimal LastZ = decimal.MinValue;
            bool keepRemoving = true;

            // Forward pass: Remove lines with Z < UserDefinedBottomZ
            for (int i = 0; i < motionBlock.Count; i++)
            {
                decimal zValue = ExtractCoordinate(motionBlock[i], "Z", LastZ); // Use LastZ as default for lines without Z
                if (zValue != LastZ)
                {
                    LastZ = zValue; // Update LastZ if a new Z coordinate is found
                }

                if (keepRemoving && LastZ < _config.UserDefinedBottomZ)
                {
                    motionBlock.RemoveAt(i);
                    i--; // Adjust index after removal
                }
                else
                {
                    keepRemoving = false; // Stop removing once we hit the first Z within the bottom limit
                }
            }

            // Reverse pass: Remove lines with Z > UserDefinedTopZ
            LastZ = decimal.MaxValue;
            keepRemoving = true;

            for (int i = motionBlock.Count - 1; i >= 0; i--)
            {
                decimal zValue = ExtractCoordinate(motionBlock[i], "Z", LastZ); // Use LastZ as default for lines without Z
                if (zValue != LastZ)
                {
                    LastZ = zValue; // Update LastZ if a new Z coordinate is found
                }

                if (keepRemoving && LastZ > _config.UserDefinedTopZ)
                {
                    motionBlock.RemoveAt(i);
                }
                else
                {
                    keepRemoving = false; // Stop removing once we hit the first Z within the top limit
                }
            }

            return motionBlock;
        }


    }
}
