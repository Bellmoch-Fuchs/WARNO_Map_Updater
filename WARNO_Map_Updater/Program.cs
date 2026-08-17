using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;


namespace warno
{
    internal class Program
    {
        static async Task Main()
        {
            #region Setup and define endpoints
            Console.WriteLine("Starting WARNO Map Batch Updater...");
            Console.WriteLine();
            bool VersionFlag = false; // Flag to check if the version number has been updated in the mapmod - only one update per run is required
            string userName = System.Environment.UserName; // System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            string WARNO_Directory = @"C:\Users\" + userName + @"\Saved Games\EugenSystems\WARNO";
            string CurrentDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            string LevelBuild = WARNO_Directory + @"\LevelBuild\";
            string DatasMap   = WARNO_Directory + @"\DatasMap\";
            string DecorPacks = WARNO_Directory + @"\Datapacks\DecorsSets\";
            string MapsPacks  = WARNO_Directory + @"\Datapacks\Maps\";
            string ScenarioPacks = WARNO_Directory + @"\Datapacks\Scenarios\";
            string ModPacks   = WARNO_Directory + @"\mod\";

            string LaunchGeneration     = "";
            string LaunchMapBaking      = "";
            string LaunchDataPacking    = "";
            string Tools_Directory      = "";
            string Mods_Directory       = "";
            string[] ModList            = new string[0];


            if (File.Exists(@"H:\Spiele\Steam_Subfolder\steamapps\common\WARNO\WARNO.exe"))
            {
                // Set Tool Locations - my personal lazyness and for testing - ment to place exe in the warno installation folder 
                LaunchGeneration  = @"H:\Spiele\Steam_Subfolder\steamapps\common\WARNO\Tools\LaunchGeneration.bat";
                LaunchMapBaking   = @"H:\Spiele\Steam_Subfolder\steamapps\common\WARNO\Tools\LaunchMapBaking.bat";
                LaunchDataPacking = @"H:\Spiele\Steam_Subfolder\steamapps\common\WARNO\Tools\LaunchDataPacking.bat";
                Tools_Directory   = @"H:\Spiele\Steam_Subfolder\steamapps\common\WARNO\Tools";
                Mods_Directory    = @"H:\Spiele\Steam_Subfolder\steamapps\common\WARNO\Mods";
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
                    Mods_Directory    = CurrentDirectory + @"\Mods";
                }
                else
                {
                    Console.WriteLine("WARNO installation folder could not be located. Exiting program!");
                    Environment.Exit(-10) ;
                }
            }
            #endregion


            // 1. Get all Maps and Mapfoldernames + Mods
            #region Get Map definition and folders
            string[] Maps = Directory.GetDirectories(LevelBuild, "*", System.IO.SearchOption.TopDirectoryOnly);
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

            // Comparison of Mod Folders - Warno Installation vs Save Folder / Check if mapmod / check if it has been updated atleast once - (no initial upload)!
            // Get all the Names of the mod folders in the Warno Installation Folder
            string[] ModsW = Directory.GetDirectories(Mods_Directory, "*", System.IO.SearchOption.TopDirectoryOnly);
            if (ModsW.Length > 0)
            {
                for (int i = 0; i < ModsW.Length; i++)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(ModsW[i]);
                    ModsW[i] = dirInfo.Name;
                }
            }
            Console.WriteLine(" Mod folders in Warno installation directory: " + ModsW.Length.ToString());

            // Get all the Names of the mod folders in the Save Folder
            string[] ModsS = Directory.GetDirectories(ModPacks, "*", System.IO.SearchOption.TopDirectoryOnly);
            if (ModsS.Length > 0)
            {
                for (int i = 0; i < ModsS.Length; i++)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(ModsS[i]);
                    ModsS[i] = dirInfo.Name;
                }
            }
            Console.WriteLine(" Mod folders in savegame directory: " + ModsS.Length.ToString());

            //Compare the two lists and create a list of mods that are present in both folders
            for (int i = 0; i < ModsS.Length; i++)
            {
                if (Array.IndexOf(ModsW,ModsS[i]) > -1)
                {
                    // Check if its actually a mapmod
                    if (Directory.Exists(ModPacks + ModsS[i] + @"\DatasMap\"))
                    {
                        if (Directory.GetFiles(ModPacks + ModsS[i] + @"\DatasMap", "*", SearchOption.TopDirectoryOnly).Length > 0)
                        {
                            Array.Resize(ref ModList, ModList.Length + 1);
                            ModList[ModList.Length - 1] = ModsS[i];
                        }
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine(" " + ModList.Length.ToString() + " Mods containing maps detected");
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

            // 4. Data Packing
            #region Data Packing
            Console.WriteLine("Continue with packing the map data? (y/n)");
            string controlInputC = Console.ReadLine();
            if (controlInputC == "y")
            {

                Console.WriteLine("Starting LaunchDatapacking.bat...");
                Process process = new Process();
                // Configure the process using the StartInfo properties.
                process.StartInfo.FileName = LaunchDataPacking;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardInput = true;
                process.EnableRaisingEvents = true;

                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (e.Data != null)
                        Console.WriteLine(" LaunchDatapacking.bat: " + e.Data);
                };


                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                {
                    if (e.Data != null)
                        Console.WriteLine(" LaunchDatapacking.bat Error: " + e.Data);
                };
                process.Start();
                process.StandardInput.Close();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();
                //process.WaitForExit();// Waits here for the process to exit.

                if (process.ExitCode == 0)
                {
                    Console.WriteLine(" Packing the map data successfully completed!");
                    Console.WriteLine();
                    process.Close();
                }
                else
                {
                    Console.WriteLine(" Error during Packing process - manual analysis required");
                    Console.WriteLine();
                    process.Close();
                }
            }
            #endregion

            // 5. Increment Version Number
            #region Version Number Update
            Console.WriteLine("Continue with incrementing the version number? (y/n)");
            string controlInputD = Console.ReadLine();
            if (controlInputD == "y")
            {
                for (int i = 0; i < ModList.Length; i++)
                {
                    string configFilePath = ModPacks + ModList[i] + @"\Config.ini";
                    if (File.Exists(configFilePath))
                    {
                        string[] configString = File.ReadAllLines(configFilePath);
                        int versionIndex = Array.FindIndex(configString, s => s.StartsWith("Version ="));
                        if (versionIndex > -1)
                        {
                            string versionString = configString[versionIndex].Split('=')[1].Trim();
                            versionString = versionString.Split(';')[0].Trim();
                            int versionNumber = int.Parse(versionString);
                            versionNumber++;
                            configString[versionIndex] = "Version = " + versionNumber.ToString() + " ; Value to increment when an update in this mod is incompatible with the current version";
                            File.WriteAllLines(configFilePath, configString);
                            Console.WriteLine("Version number for " + ModList[i] + " updated to " + versionNumber.ToString());
                        }
                        else
                        {
                            Console.WriteLine("Version number not found in Config.ini for " + ModList[i] + " - Skipping.");
                            Console.WriteLine();
                        }
                    }
                    else
                    {
                        Console.WriteLine("Config file not found for " + ModList[i] + " - Skipping.");
                        Console.WriteLine();
                    }
                }

            }
            #endregion

            // 6. Update map mods with new packed data 
            #region Version Number Update
            Console.WriteLine("Continue with updating mod folder files? (y/n)");
            string controlInputE = Console.ReadLine();

            if (controlInputE == "y")
            {
                // Each folder is updated individually to ease algorithm complexity and my lazyness :)
                for (int i = 0; i < ModList.Length; i++)
                {
                    // DecorsSets
                    string[] decorFiles = Directory.GetFiles(ModPacks + ModList[i] + @"\DecorsSets", "*", SearchOption.TopDirectoryOnly);
                    if(decorFiles.Length > 0)
                    {
                        
                        for (int j = 0; j < decorFiles.Length; j++)
                        {
                            FileInfo filInfo = new FileInfo(decorFiles[j]);
                            string fileNameBuffer = DecorPacks+filInfo.Name;
                            if (File.Exists(fileNameBuffer))
                            {
                                File.Copy(fileNameBuffer, decorFiles[j], true);
                                //Console.WriteLine(" Copy file from " + fileNameBuffer + " to " + decorFiles[j]);
                            }
                        }
                    }

                    // DatasMap
                    string[] dataMapFiles = Directory.GetFiles(ModPacks + ModList[i] + @"\DatasMap", "*", SearchOption.TopDirectoryOnly);
                    if (dataMapFiles.Length > 0)
                    {

                        for (int j = 0; j < dataMapFiles.Length; j++)
                        {
                            FileInfo filInfo = new FileInfo(dataMapFiles[j]);
                            string fileNameBuffer = DatasMap + filInfo.Name;
                            if (File.Exists(fileNameBuffer))
                            {
                                File.Copy(fileNameBuffer, dataMapFiles[j], true);
                            }
                        }
                    }


                    // Maps
                    string[] MappackFiles = Directory.GetFiles(ModPacks + ModList[i] + @"\Maps", "*", SearchOption.TopDirectoryOnly);
                    if (MappackFiles.Length > 0)
                    {

                        for (int j = 0; j < MappackFiles.Length; j++)
                        {
                            FileInfo filInfo = new FileInfo(MappackFiles[j]);
                            string fileNameBuffer = MapsPacks + filInfo.Name;
                            if (File.Exists(fileNameBuffer))
                            {
                                File.Copy(fileNameBuffer, MappackFiles[j], true);
                            }
                        }
                    }

                    // Scenarios
                    string[] PackScenarioFiles = Directory.GetFiles(ModPacks + ModList[i] + @"\Scenarios", "*", SearchOption.TopDirectoryOnly);
                    if (PackScenarioFiles.Length > 0)
                    {

                        for (int j = 0; j < PackScenarioFiles.Length; j++)
                        {
                            FileInfo filInfo = new FileInfo(PackScenarioFiles[j]);
                            string fileNameBuffer = ScenarioPacks + filInfo.Name;
                            if (File.Exists(fileNameBuffer))
                            {
                                File.Copy(fileNameBuffer, PackScenarioFiles[j], true);
                            }
                        }
                    }
                }


            }
            #endregion

            // 7. Update map mods base structure 
            #region Update Mod Base File
            Console.WriteLine("Continue with updating map mod gen files? (y/n)");
            string controlInputF = Console.ReadLine();

            if (controlInputF == "y")
            {
                for (int i = 0; i < ModList.Length; i++)
                {
                    Console.WriteLine();
                    Console.WriteLine(" Updating: " + ModList[i]);
                    Console.WriteLine(" ...");
                    Process baking = new Process();
                    // Configure the process using the StartInfo properties.
                    baking.StartInfo.FileName = Mods_Directory + @"\" + ModList[i] + @"\UpdateMod.bat";
                    Directory.SetCurrentDirectory(Mods_Directory + @"\" + ModList[i]);
                    baking.StartInfo.Arguments = "/C ";
                    baking.StartInfo.CreateNoWindow = true;
                    baking.StartInfo.UseShellExecute = false;
                    baking.StartInfo.RedirectStandardOutput = true;
                    baking.StartInfo.RedirectStandardError = true;
                    baking.StartInfo.RedirectStandardInput = true;
                    baking.EnableRaisingEvents = true;

                    baking.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                    {
                        if (e.Data != null)
                            Console.WriteLine("Updating Gen Files... " + e.Data);
                    };


                    baking.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                    {
                        if (e.Data != null)
                            Console.WriteLine("Error during Update: " + e.Data);
                    };
                    baking.Start();
                    baking.StandardInput.Close();
                    baking.BeginOutputReadLine();
                    baking.BeginErrorReadLine();
                    await baking.WaitForExitAsync();
                    //process.WaitForExit();// Waits here for the process to exit.

                    if (baking.ExitCode == 0)
                    {
                        Console.WriteLine("Gen File Update successfull for: " + ModList[i]);
                        Console.WriteLine();
                        baking.Close();
                    }
                    else
                    {
                        Console.WriteLine("Error during Gen File Update for: " + ModList[i]);
                        Console.WriteLine();
                        baking.Close();
                    }


                }

            }
            #endregion

            // 8. Update map mods base structure 
            #region Generate Mod Base File
            Console.WriteLine("Continue with generating map mod gen files? (y/n)");
            string controlInputG = Console.ReadLine();

            if (controlInputG == "y")
            {
                for (int i = 0; i < ModList.Length; i++)
                {
                    Console.WriteLine();
                    Console.WriteLine(" Generating files for: " + ModList[i]);
                    Console.WriteLine(" ...");
                    Directory.SetCurrentDirectory(Mods_Directory + @"\" + ModList[i]);
                    Process baking = new Process();
                    // Configure the process using the StartInfo properties.
                    baking.StartInfo.FileName = Mods_Directory + @"\" +ModList[i] + @"\GenerateMod.bat";
                    baking.StartInfo.Arguments = "/C ";
                    baking.StartInfo.CreateNoWindow = true;
                    baking.StartInfo.UseShellExecute = false;
                    baking.StartInfo.RedirectStandardOutput = true;
                    baking.StartInfo.RedirectStandardError = true;
                    baking.StartInfo.RedirectStandardInput = true;
                    baking.EnableRaisingEvents = true;

                    baking.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                    {
                        if (e.Data != null)
                            Console.WriteLine("Generating Files... " + e.Data);
                    };


                    baking.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                    {
                        if (e.Data != null)
                            Console.WriteLine("Error during File Generation: " + e.Data);
                    };
                    baking.Start();
                    baking.StandardInput.Close();
                    baking.BeginOutputReadLine();
                    baking.BeginErrorReadLine();
                    await baking.WaitForExitAsync();
                    //process.WaitForExit();// Waits here for the process to exit.

                    if (baking.ExitCode == 0)
                    {
                        Console.WriteLine("Generating files successfull for: " + ModList[i]);
                        Console.WriteLine();
                        baking.Close();
                    }
                    else
                    {
                        Console.WriteLine("Error during file Generation for: " + ModList[i]);
                        Console.WriteLine();
                        baking.Close();
                    }


                }

            }
            #endregion

            // 9. Upload map mods  
            #region Upload Map Mod Base
            Console.WriteLine("Continue with uploading map mods? (y/n)");
            string controlInputH = Console.ReadLine();

            if (controlInputH == "y")
            {
                for (int i = 0; i < ModList.Length; i++)
                {
                    Console.WriteLine();
                    Console.WriteLine(" Uploading: " + ModList[i]);
                    Console.WriteLine(" ...");
                    Process baking = new Process();
                    Directory.SetCurrentDirectory(Mods_Directory + @"\" + ModList[i]);
                    // Configure the process using the StartInfo properties.
                    baking.StartInfo.FileName = Mods_Directory + @"\" + ModList[i] + @"\UploadMod.bat";
                    baking.StartInfo.Arguments = "/C ";
                    baking.StartInfo.CreateNoWindow = true;
                    baking.StartInfo.UseShellExecute = false;
                    baking.StartInfo.RedirectStandardOutput = true;
                    baking.StartInfo.RedirectStandardError = true;
                    baking.StartInfo.RedirectStandardInput = true;
                    baking.EnableRaisingEvents = true;

                    baking.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
                    {
                        if (e.Data != null)
                            Console.WriteLine("Uploading Mod... " + e.Data);
                    };


                    baking.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
                    {
                        if (e.Data != null)
                            Console.WriteLine("Error during Upload: " + e.Data);
                    };
                    baking.Start();
                    baking.StandardInput.Close();
                    baking.BeginOutputReadLine();
                    baking.BeginErrorReadLine();
                    await baking.WaitForExitAsync();
                    //process.WaitForExit();// Waits here for the process to exit.

                    if (baking.ExitCode == 0)
                    {
                        Console.WriteLine("Upload successfull for: " + ModList[i]);
                        Console.WriteLine();
                        baking.Close();
                    }
                    else
                    {
                        Console.WriteLine("Error during Upload for: " + ModList[i]);
                        Console.WriteLine();
                        baking.Close();
                    }


                }

            }
            #endregion
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("----------------- WARNO Map Batch Update Finished! -----------------");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

        }
    }
}


