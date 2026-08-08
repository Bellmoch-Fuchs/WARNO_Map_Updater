using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;


namespace warno
{
    internal class Program
    {
        static async Task Main()
        {
            #region Setup and define endpoints
            Console.WriteLine("Starting WARNO Map Batch Updater...");
            Console.WriteLine();
            string userName = System.Environment.UserName; // System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            string WARNO_Directory = @"C:\Users\" + userName + @"\Saved Games\EugenSystems\WARNO";
            string CurrentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string LevelBuild = WARNO_Directory + @"\LevelBuild\";
            string DatasMap   = WARNO_Directory + @"\DatasMap\";
            string DecorPacks = WARNO_Directory + @"\Datapacks\DecorsSets\";
            string MapsPacks  = WARNO_Directory + @"\Datapacks\Maps\";
            string ScenarioPacks = WARNO_Directory + @"\Datapacks\Scenarios\";

            string LaunchGeneration     = "";
            string LaunchMapBaking      = "";
            string LaunchDataPacking    = "";
            string Tools_Directory      = "";


            if (File.Exists(@"H:\Spiele\Steam_Subfolder\steamapps\common\WARNO\WARNO.exe"))
            {
                // Set Tool Locations
                LaunchGeneration  = @"H:\Spiele\Steam_Subfolder\steamapps\common\WARNO\Tools\LaunchGeneration.bat";
                LaunchMapBaking   = @"H:\Spiele\Steam_Subfolder\steamapps\common\WARNO\Tools\LaunchMapBaking.bat";
                LaunchDataPacking = @"H:\Spiele\Steam_Subfolder\steamapps\common\WARNO\Tools\LaunchDataPacking.bat";
                Tools_Directory   = @"H:\Spiele\Steam_Subfolder\steamapps\common\WARNO\Tools";

            }
            else
            {
                //string CurrentDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (File.Exists(CurrentDirectory + @"/WARNO.exe"))
                {
                    LaunchGeneration  = CurrentDirectory + @"\Tools\LaunchGeneration.bat";
                    LaunchMapBaking   = CurrentDirectory + @"\Tools\LaunchMapBaking.bat";
                    LaunchDataPacking = CurrentDirectory + @"\Tools\LaunchDataPacking.bat";
                    Tools_Directory   = CurrentDirectory + @"\Tools";
                }
                else
                {
                    Console.WriteLine("WARNO installation folder could not be located. Exiting program!");
                    Environment.Exit(-10) ;
                }
            }
            #endregion


            // 1. Get all Maps and Mapfoldernames
            #region Get Map definition and folders
            string[] Maps = Directory.GetDirectories(LevelBuild, "*", System.IO.SearchOption.AllDirectories);
            if (Maps.Length > 0)
            {
                Console.WriteLine(Maps.Length.ToString() + " Maps Detected:");
                Console.WriteLine();
                for( int i = 0; i < Maps.Length; i++ )
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(Maps[i]);
                    Maps[i] = dirInfo.Name;
                    Console.WriteLine("  " + Maps[i]);
                }
            }
            else
            {
                Console.WriteLine("No maps detected. Exiting program!");
                Environment.Exit(-11);
            }
            Console.WriteLine();
            Directory.SetCurrentDirectory(Tools_Directory);
            #endregion

            // 2. Initial LaunchGeneration
            #region LaunchGeneration
            Console.WriteLine("Continue with preliminary LaunchGeneration? (y/n)");
            string controlInputA = Console.ReadLine();
            if (controlInputA == "y")
            {
                
                Console.WriteLine("Starting LaunchGeneration.bat...");
                Process process = new Process();
                // Configure the process using the StartInfo properties.
                process.StartInfo.FileName = LaunchGeneration;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardInput = true;
                process.EnableRaisingEvents = true;

                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (e.Data != null)
                        Console.WriteLine(" LaunchGeneration: " + e.Data);
                };


                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (e.Data != null)
                        Console.WriteLine(" LaunchGeneration Error: " + e.Data);
                };
                process.Start();
                process.StandardInput.Close();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();
                //process.WaitForExit();// Waits here for the process to exit.

                if (process.ExitCode == 0)
                {
                    Console.WriteLine(" LaunchGeneration successfully completed!");
                    Console.WriteLine();
                    process.Close();
                }
                else
                {
                    Console.WriteLine(" Error during LaunchGeneration - manual analysis required");
                    Console.WriteLine();
                    process.Close();
                }
            }
            #endregion

            // 3. Bake Maps
            #region Map Baking 
            Console.WriteLine("Continue with Map Baking Process? (y/n)");
            string controlInputB = Console.ReadLine();
            if(controlInputB == "y")
            {     
                // Baking the maps
                for (int i = 0; i < Maps.Length; i++)
                {
                    Console.WriteLine();
                    Console.WriteLine(" Baking: " + Maps[i]);
                    Console.WriteLine(" ...");
                    Process baking = new Process();
                    // Configure the process using the StartInfo properties.
                    baking.StartInfo.FileName = LaunchGeneration;
                    baking.StartInfo.Arguments = "/C " + Maps[i];
                    baking.StartInfo.CreateNoWindow = true;
                    baking.StartInfo.UseShellExecute = false;
                    baking.StartInfo.RedirectStandardOutput = true;
                    baking.StartInfo.RedirectStandardError = true;
                    baking.StartInfo.RedirectStandardInput = true;
                    baking.EnableRaisingEvents = true;

                    baking.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                    {
                        if (e.Data != null)
                            Console.WriteLine("Baking: " + e.Data);
                    };


                    baking.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                    {
                        if (e.Data != null)
                            Console.WriteLine("Baking Error: " + e.Data);
                    };
                    baking.Start();
                    baking.StandardInput.Close();
                    baking.BeginOutputReadLine();
                    baking.BeginErrorReadLine();
                    await baking.WaitForExitAsync();
                    //process.WaitForExit();// Waits here for the process to exit.

                    if (baking.ExitCode == 0)
                    {
                        Console.WriteLine("Baking successfull for: " + Maps[i]);
                        Console.WriteLine();
                        baking.Close();
                    }
                    else
                    {
                        Console.WriteLine("Error during baking process for: " + Maps[i]);
                        Console.WriteLine();
                        baking.Close();
                    }


                }
            }
            #endregion

        }
    }
}


