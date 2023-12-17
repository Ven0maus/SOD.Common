using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Builder
{
    internal class EntryPoint
    {
        static void Main()
        {
            Console.Write("Clear all previous builds?(Y/N): ");
            var reply = Console.ReadLine();
            bool clearBuilds = reply.Equals("Y", StringComparison.OrdinalIgnoreCase);
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;
            var originalColor = Console.ForegroundColor;

            Console.WriteLine("Creating new builds, please wait.");
            Console.WriteLine();

            var sw = Stopwatch.StartNew();
            var info = JsonConvert.DeserializeObject<BuildInfo>(File.ReadAllText("manifest.json"));
            foreach (var build in info.Builds)
            {
                if (!build.Package)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"Skipped [Package: false]: {build.ProjectName}");
                    Console.ForegroundColor = originalColor;
                    continue;
                }

                var solutionDirectory = TryGetSolutionDirectoryInfo() ??
                    throw new Exception("Unable to find solution directory.");

                // Get paths based on solution folder and project name
                string folderPath;
                if (!string.IsNullOrWhiteSpace(build.ProjectParentPath))
                    folderPath = Path.Combine(solutionDirectory.FullName, build.ProjectParentPath, build.ProjectName, build.ModFolderName);
                else
                    folderPath = Path.Combine(solutionDirectory.FullName, build.ProjectName, build.ModFolderName);
                if (!Directory.Exists(folderPath))
                    throw new Exception("Unable to find mod folder at path: " + folderPath);

                // Create new destination directory if it doesn't exist yet within the mod folder
                var destinationPath = Path.Combine(folderPath, build.DestinationFolderName);
                if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);

                if (clearBuilds)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("Clearing previous build(s).");
                    Console.ForegroundColor = originalColor;
                    foreach (var file in Directory.EnumerateFiles(destinationPath))
                        File.Delete(file);
                }

                var packageBuilder = new BuildTool.PackageBuilder(build.ProjectName, folderPath, destinationPath);
                foreach (var dllPath in build.DLLPaths)
                {
                    string dllFullPath;
                    if (!string.IsNullOrWhiteSpace(build.ProjectParentPath))
                        dllFullPath = Path.Combine(solutionDirectory.FullName, build.ProjectParentPath, build.ProjectName, dllPath);
                    else
                        dllFullPath = Path.Combine(solutionDirectory.FullName, build.ProjectName, dllPath);
                    packageBuilder.AddDLL(dllFullPath);
                }

                Console.WriteLine($"Attempting to build package for project: {build.ProjectName}");
                if (!BuildTool.Package(packageBuilder, out string packagePath, out string packageVersion))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Packaging cancelled, a package for the version {packageVersion} already exists.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Package version \"{packageVersion}\" created, zip archive location: " + packagePath);
                }
                Console.ForegroundColor = originalColor;
                Console.WriteLine();
            }
            sw.Stop();

            Console.WriteLine($"Build process completed. Time Taken: {sw.Elapsed}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static DirectoryInfo TryGetSolutionDirectoryInfo(string currentPath = null)
        {
            var directory = new DirectoryInfo(currentPath ?? Directory.GetCurrentDirectory());
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return directory;
        }
    }
}