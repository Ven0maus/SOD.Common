using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;

namespace Builder
{
    /// <summary>
    /// Class used to help create a r2modman zip file for your mod.
    /// </summary>
    public static class BuildTool
    {
        /// <summary>
        /// Create a zip file package for r2modman based on the mod content folder.
        /// </summary>
        /// <param name="sourceFolderPath"></param>
        /// <param name="targetFolderPath"></param>
        public static bool Package(PackageBuilder builder, out string packagePath, out string packageVersion)
        {
            packagePath = null;
            packageVersion = null;
            if (!builder.DLLs.Any())
                throw new Exception($"Missing DLL path(s) in PackageBuilder for build \"{builder.ProjectName}\".");

            // Collect mod files
            var files = Directory.GetFiles(builder.FolderPath);
            var filesWeNeed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "changelog.md", "icon.png", "manifest.json", "readme.md"
            };
            var fileNames = files
                .Select(a => new { Path = a, Filename = Path.GetFileName(a) })
                .ToArray();
            var validFilePaths = fileNames
                .Where(a => filesWeNeed.Contains(a.Filename))
                .Select(a => a.Path)
                .ToArray();

            // Attempt to read version from r2modman manifest
            var manifestFile = fileNames
                .Select(a =>
                {
                    if (a.Filename.Equals("manifest.json", StringComparison.OrdinalIgnoreCase))
                        return a.Path;
                    return null;
                }).FirstOrDefault(a => a != null);
            if (manifestFile != null)
            {
                var manifestJObject = JObject.Parse(File.ReadAllText(manifestFile));
                packageVersion = manifestJObject.SelectToken("version_number")?.Value<string>();
            }

            packageVersion ??= "1.0.0";

            // Parse project name as filename for the zip archive
            string projectName = builder.ProjectName.Replace(".", "_");
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var invalidChar in invalidChars)
                projectName = projectName.Replace(invalidChar, '_');

            // Create zip archive file name
            var zipFileName = Path.Combine(builder.DestinationPath, $"{projectName}_{packageVersion}.zip");

            // Verify if there already exists a zip archive with this name
            if (File.Exists(zipFileName))
                return false;

            // Create zip archive
            using (ZipArchive archive = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
            {
                // Make sure to include the DLL paths
                foreach (var filePath in validFilePaths.Concat(builder.DLLs).Distinct())
                {
                    var entryName = Path.GetFileName(filePath);
                    var entry = archive.CreateEntry(entryName);

                    using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    using var entryStream = entry.Open();
                    // Copy the file content to the archive entry
                    fileStream.CopyTo(entryStream);
                }
            }
            packagePath = zipFileName;
            return true;
        }

        public class PackageBuilder
        {
            internal string ProjectName { get; }
            internal HashSet<string> DLLs { get; }
            internal string FolderPath { get; }
            internal string DestinationPath { get; }

            public PackageBuilder(string projectName, string sourceFolder, string destinationFolder)
            {
                if (string.IsNullOrWhiteSpace(projectName))
                    throw new ArgumentException("Invalid project name, please provide a valid name.", nameof(sourceFolder));

                if (string.IsNullOrWhiteSpace(sourceFolder))
                    throw new ArgumentException("Invalid path, please provide a valid path.", nameof(sourceFolder));
                if (string.IsNullOrWhiteSpace(destinationFolder))
                    throw new ArgumentException("Invalid path, please provide a valid path.", nameof(destinationFolder));

                if (!Directory.Exists(sourceFolder))
                    throw new ArgumentException("No folder exists at the provided path, please provide a valid path.", nameof(sourceFolder));
                if (!Directory.Exists(destinationFolder))
                    throw new ArgumentException("No folder exists at the provided path, please provide a valid path.", nameof(destinationFolder));

                ProjectName = projectName;
                FolderPath = sourceFolder;
                DestinationPath = destinationFolder;
                DLLs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Add a DLL file's path to the builder
            /// </summary>
            /// <param name="dllPath"></param>
            /// <returns></returns>
            public void AddDLL(params string[] dllPaths)
            {
                if (dllPaths == null || dllPaths.Length == 0)
                    return;
                foreach (var path in dllPaths)
                {
                    if (!File.Exists(path))
                        throw new Exception("No file exists at the given DLL path: " + path);
                    DLLs.Add(path);
                }
            }
        }
    }
}
