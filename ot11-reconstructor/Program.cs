using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileReconstructor
{
    class Program
    {
        private static readonly Dictionary<string, string> FileSignatures = new Dictionary<string, string>
        {
            { "4D-5A", ".exe" },
            { "4D-5A", ".dll" },
            { "50-4B-03-04", ".zip" },
            { "50-4B-03-04-14-00-08-00", ".docx" },
            { "89-50-4E-47", ".png" },
            { "FF-D8-FF", ".jpg" },
            { "42-4D", ".bmp" },
            { "47-49-46-38", ".gif" },
            { "25-50-44-46", ".pdf" },
            { "49-44-33", ".mp3" },
            { "23-21-2F", ".sh" },
            { "7B-22", ".json" },
            { "4F-67-67-53", ".ogg" },
            { "66-4C-61-43", ".flac" },
            { "00-00-01-BA", ".mpg" },
            { "00-00-01-B3", ".mpg" },
            { "1F-8B-08", ".gz" },
            { "52-49-46-46", ".avi" },
            { "30-26-B2-75", ".wmv" },
            { "52-61-72-21", ".rar" },
            { "D0-CF-11-E0", ".doc" },
            { "46-4F-52-4D", ".iff" },
            { "4D-54-68-64", ".mid" },
            { "00-01-00-00", ".ico" },
            { "49-49-2A-00", ".tiff" },
            { "EF-BB-BF", ".txt" },
            { "3C-21-44-4F", ".html" },
            { "1A-45-DF-A3", ".mkv" },
            { "1A-45-DF-A3", ".webm" },
            { "66-74-79-70", ".mp4" },
            { "47-4C-54-32", ".dat" },
            { "76-65-72-73", ".version" },
            { "75-65-70-72", ".ueproj" },
            { "7B-5C-72-74", ".rtf" },
            { "4D-44-4D-50", ".pdb" },
            { "55-74-66-38", ".txt" },
            { "2E-70-61-6B", ".pak" }
        };

        static void Main(string[] args)
        {
            string selfFileName = Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            Console.WriteLine("Enter the output folder path for reconstructed files:");
            string outputFolder = Console.ReadLine();

            string currentDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine($"Scanning directory: {currentDirectory}");

            var files = Directory.GetFiles(currentDirectory)
                .Where(f => Path.GetFileName(f) != selfFileName)
                .OrderBy(f => f)
                .ToList();

            Dictionary<string, List<string>> fileGroups = new Dictionary<string, List<string>>();

            foreach (var file in files)
            {
                try
                {
                    byte[] headerBytes = new byte[8];
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        fs.Read(headerBytes, 0, headerBytes.Length);
                    }

                    string headerString = BitConverter.ToString(headerBytes);

                    if (!fileGroups.ContainsKey(headerString))
                    {
                        fileGroups[headerString] = new List<string>();
                    }
                    fileGroups[headerString].Add(file);

                    Console.WriteLine($"File: {file} grouped under header: {headerString}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file {file}: {ex.Message}");
                }
            }

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            int fileCounter = 1;
            foreach (var group in fileGroups)
            {
                string extension = DetectFileExtension(group.Value.First());

                string outputFilePath = Path.Combine(outputFolder, $"reconstructed_file_{fileCounter}{extension}");

                try
                {
                    using (FileStream outputFs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                    {
                        foreach (var chunkFile in group.Value.OrderBy(f => f))
                        {
                            byte[] chunkBytes = File.ReadAllBytes(chunkFile);
                            outputFs.Write(chunkBytes, 0, chunkBytes.Length);
                            Console.WriteLine($"Appending chunk {chunkFile} to {outputFilePath}");
                        }
                    }

                    Console.WriteLine($"Reconstructed file saved as: {outputFilePath}");
                    fileCounter++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reconstructing files for group {group.Key}: {ex.Message}");
                }
            }

            Console.WriteLine("File reconstruction completed. Press any key to exit.");
            Console.ReadKey();
        }

        private static string DetectFileExtension(string filePath)
        {
            try
            {
                byte[] headerBytes = new byte[8];
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fs.Read(headerBytes, 0, headerBytes.Length);
                }

                string headerString = BitConverter.ToString(headerBytes.Take(4).ToArray());

                foreach (var signature in FileSignatures)
                {
                    if (headerString.StartsWith(signature.Key))
                    {
                        return signature.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting file extension for {filePath}: {ex.Message}");
            }

            return ".bin";
        }
    }
}