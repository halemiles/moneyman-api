using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Moneyman.Api
{
    public static class DatabaseInitializer
    {
        private static string FindScriptPath()
        {
            // Try multiple possible paths
            var possiblePaths = new[]
            {
                // Development: from bin/Debug/net9.0 back to src/Scripts
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Scripts", "seed_database.py"),
                // Alternative development path
                Path.Combine(Directory.GetCurrentDirectory(), "..", "Scripts", "seed_database.py"),
                // Running from src/Moneyman.Api directory
                Path.Combine(Directory.GetCurrentDirectory(), "..", "Scripts", "seed_database.py"),
                // Docker/published builds (if script is copied to output)
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "seed_database.py")
            };
            
            foreach (var path in possiblePaths)
            {
                var normalizedPath = Path.GetFullPath(path);
                if (File.Exists(normalizedPath))
                {
                    return normalizedPath;
                }
            }
            
            return null;
        }
        
        public static void SeedDatabase(ILogger logger = null)
        {
            try
            {
                var scriptPath = FindScriptPath();
                
                if (scriptPath == null)
                {
                    logger?.LogWarning("Seed script not found. Skipping database seeding.");
                    return;
                }
                
                logger?.LogInformation("Initializing database with seed data...");
                
                // Try python3 first (Linux/Mac), fallback to python (Windows)
                string pythonCommand = "python3";
                try
                {
                    var testProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = pythonCommand,
                        Arguments = "--version",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                    testProcess?.WaitForExit();
                    if (testProcess?.ExitCode != 0)
                    {
                        pythonCommand = "python";
                    }
                }
                catch
                {
                    pythonCommand = "python";
                }
                
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = pythonCommand,
                    Arguments = $"\"{scriptPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(scriptPath)
                };

                using (var process = Process.Start(processStartInfo))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        
                        process.WaitForExit();
                        
                        if (!string.IsNullOrEmpty(output))
                        {
                            logger?.LogInformation($"Database seeding output:\n{output}");
                        }
                        
                        if (process.ExitCode == 0)
                        {
                            logger?.LogInformation("Database seeding completed successfully");
                        }
                        else
                        {
                            logger?.LogError($"Database seeding failed with exit code {process.ExitCode}");
                            if (!string.IsNullOrEmpty(error))
                            {
                                logger?.LogError($"Error: {error}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error during database seeding");
            }
        }
    }
}
