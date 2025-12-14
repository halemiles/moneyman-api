using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Moneyman.Api
{
    public static class DatabaseInitializer
    {
        public static void SeedDatabase(ILogger logger = null)
        {
            try
            {
                var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Scripts", "seed_database.py");
                
                // Normalize the path
                scriptPath = Path.GetFullPath(scriptPath);
                
                if (!File.Exists(scriptPath))
                {
                    // Try alternative path for Docker/published builds
                    scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "seed_database.py");
                    if (!File.Exists(scriptPath))
                    {
                        logger?.LogWarning($"Seed script not found at {scriptPath}. Skipping database seeding.");
                        return;
                    }
                }
                
                logger?.LogInformation("Initializing database with seed data...");
                
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "python3",
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
