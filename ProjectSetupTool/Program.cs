using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using Newtonsoft.Json.Linq;

namespace ProjectSetupTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("      Mikrocop Project Setup Tool");
            Console.WriteLine("==========================================");
            Console.WriteLine();

            // 1. Detect SQL Servers
            Console.WriteLine("Detecting local SQL Server instances...");
            var instances = DetectSqlInstances();

            string selectedServer = "";

            if (instances.Count > 0)
            {
                Console.WriteLine($"Found {instances.Count} instance(s):");
                for (int i = 0; i < instances.Count; i++)
                {
                    Console.WriteLine($"[{i + 1}] {instances[i]}");
                }
                Console.WriteLine($"[{instances.Count + 1}] Enter manually");

                int selection = 0;
                while (selection < 1 || selection > instances.Count + 1)
                {
                    Console.Write("\nSelect a server (enter number): ");
                    string? input = Console.ReadLine();
                    if (int.TryParse(input, out int result))
                    {
                        selection = result;
                    }
                }

                if (selection == instances.Count + 1)
                {
                    Console.Write("Enter SQL Server name (e.g., .\\SQLEXPRESS): ");
                    selectedServer = Console.ReadLine() ?? "";
                }
                else
                {
                    selectedServer = instances[selection - 1];
                }
            }
            else
            {
                Console.WriteLine("No local SQL Server instances detected.");
                Console.Write("Enter SQL Server name (e.g., .\\SQLEXPRESS): ");
                selectedServer = Console.ReadLine() ?? "";
            }

            if (string.IsNullOrWhiteSpace(selectedServer))
            {
                Console.WriteLine("No server selected. Exiting.");
                return;
            }

            Console.WriteLine($"\nSelected Server: {selectedServer}");

            // 2. Update appsettings.json
            string? appSettingsPath = FindAppSettings();
            if (appSettingsPath == null)
            {
                Console.WriteLine("\n[ERROR] Could not find UserManagementService/appsettings.json.");
                PrintTroubleshooting(selectedServer);
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Found appsettings.json at: {appSettingsPath}");
            UpdateConnectionString(appSettingsPath, selectedServer);

            // 3. Run Migrations
            Console.WriteLine("\nSetting up database...");
            RunMigrations(appSettingsPath);

            Console.WriteLine("\n==========================================");
            Console.WriteLine("      Setup Complete!");
            Console.WriteLine("==========================================");
            Console.WriteLine("You can now open UserManagementService.sln and run the project.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static List<string> DetectSqlInstances()
        {
            var instances = new List<string>();
            try
            {
                // Check Windows Services for SQL Server
                if (OperatingSystem.IsWindows())
                {
                    var services = ServiceController.GetServices();
                    foreach (var service in services)
                    {
                        if (service.ServiceName == "MSSQLSERVER")
                        {
                            instances.Add("."); // Default instance
                        }
                        else if (service.ServiceName.StartsWith("MSSQL$"))
                        {
                            string instanceName = service.ServiceName.Substring(6);
                            instances.Add($".\\{instanceName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not detect services due to error: {ex.Message}");
            }
            return instances.Distinct().ToList();
        }

        static string? FindAppSettings()
        {
            // Strategy: Look in current directory for "UserManagementService" folder.
            // If not found, look in parent (for dev mode).
            
            Console.WriteLine($"Current working directory: {Environment.CurrentDirectory}");

            // Case A: Running from SLN root (via terminal or if exe placed there)
            string pathA = Path.Combine(Environment.CurrentDirectory, "UserManagementService", "appsettings.json");
            if (File.Exists(pathA)) return pathA;

            // Case B: Running from bin/Debug/net8.0 (Development)
            DirectoryInfo? dir = new DirectoryInfo(Environment.CurrentDirectory);
            for (int i = 0; i < 6; i++)
            {
                if (dir == null) break;
                string attempt = Path.Combine(dir.FullName, "UserManagementService", "appsettings.json");
                if (File.Exists(attempt)) return attempt;
                dir = dir.Parent;
            }

            return null;
        }

        static string GetConnectionString(string server)
        {
             return $"Server={server};Database=UserManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
        }

        static void PrintTroubleshooting(string selectedServer)
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("TROUBLESHOOTING:");
            Console.WriteLine("1. Ensure 'UserManagementService' folder is next to this tool.");
            Console.WriteLine("2. OR manually edit 'appsettings.json' in the 'UserManagementService' project:");
            Console.WriteLine("   Find: \"ConnectionStrings\": { \"DefaultConnection\": \"...\" }");
            Console.WriteLine($"   Replace with: \"{GetConnectionString(selectedServer)}\"");
            Console.WriteLine("--------------------------------------------------------------");
        }

        static void PrintMigrationTroubleshooting()
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("TROUBLESHOOTING:");
            Console.WriteLine("1. Ensure 'dotnet-ef' tool is installed:");
            Console.WriteLine("   Run: dotnet tool install --global dotnet-ef");
            Console.WriteLine("2. If installed, try running migrations manually from the solution root:");
            Console.WriteLine("   Run: dotnet ef database update --project UserManagementService --startup-project UserManagementService");
            Console.WriteLine("--------------------------------------------------------------");
        }

        static void UpdateConnectionString(string path, string server)
        {
            try
            {
                string json = File.ReadAllText(path);
                JObject jsonObj = JObject.Parse(json);

                // Build connection string
                string connString = GetConnectionString(server);

                if (jsonObj["ConnectionStrings"] == null)
                {
                    jsonObj["ConnectionStrings"] = new JObject();
                }
                
                // Ensure ConnectionStrings is treated as JObject to avoid potential null reference on indexer
                var connectionStrings = jsonObj["ConnectionStrings"];
                if (connectionStrings != null)
                {
                    connectionStrings["DefaultConnection"] = connString;
                }

                File.WriteAllText(path, jsonObj.ToString());
                Console.WriteLine("Updated ConnectionStrings:DefaultConnection in appsettings.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating appsettings.json: {ex.Message}");
            }
        }

        static void RunMigrations(string appSettingsPath)
        {
            try
            {
                string? dirName = Path.GetDirectoryName(appSettingsPath);
                if (dirName == null) return;
                
                DirectoryInfo? parentDir = Directory.GetParent(dirName);
                if (parentDir == null) return;

                string solutionRoot = parentDir.FullName;
                
                Console.WriteLine($"Executing EF Migrations in: {solutionRoot}");

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "ef database update --project UserManagementService --startup-project UserManagementService",
                    WorkingDirectory = solutionRoot,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process? proc = Process.Start(psi))
                {
                    if (proc == null)
                    {
                        Console.WriteLine("Error: Could not start dotnet process.");
                        return;
                    }

                    proc.OutputDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine(e.Data); };
                    proc.ErrorDataReceived += (sender, e) => { if (e.Data != null) Console.WriteLine("ERROR: " + e.Data); };

                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    proc.WaitForExit();

                    if (proc.ExitCode == 0)
                    {
                        Console.WriteLine("Database setup completed successfully.");
                    }
                    else
                    {
                        Console.WriteLine("\n[ERROR] Database setup failed.");
                        PrintMigrationTroubleshooting();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running migrations: {ex.Message}");
            }
        }
    }
}
