//MIT, 2017, EngineKit
using System;
using System.IO;

namespace EasePatcher
{
    class Program
    {

        static string current_node_version = "node-v7.10.0";
        static string patch_subdir = "node_patches/node7.10_modified";
        //
        static string original_node_srcdir = "";
        static string espresso_srcdir = "";
        static string config_pars = "";
        //
        static PatcherOS currentOS;

        static void Main(string[] args)
        {
            Console.WriteLine("Espresso's EasePatcher");
            Console.WriteLine("for " + current_node_version);
            Console.WriteLine("======================");

            //------------------------------------------  
            //welcome page....
            InitCheckSourceAndTargetFolders();
            //------------------------------------------         
            //select what to do 

            while (DisplayMainMenu()) ;

        }
        static void InitCheckSourceAndTargetFolders()
        {

            //1. os
            currentOS = PatcherBase.GetPatcherOS();
            Console.WriteLine("current OS: " + currentOS);


            switch (currentOS)
            {
                case PatcherOS.Windows:

                    original_node_srcdir = @"C:\projects\node-v7.10.0";
                    espresso_srcdir = @"D:\projects\CompilerKit\Espresso";
                    config_pars = "release dll nosign nobuild";

                    break;
                case PatcherOS.Mac:
                    original_node_srcdir = "../../../node-v7.10.0";
                    espresso_srcdir = "../../../Espresso";
                    config_pars = "--dest-cpu=x64 --shared --xcode";

                    break;
                case PatcherOS.Linux:
                    original_node_srcdir = "../../../node-v7.10.0";
                    espresso_srcdir = "../../../Espresso";
                    config_pars = "x64 --shared";
                    break;
            }
            //
            Console.Write("original_node_srcdir: " + original_node_srcdir + " ->");
            if (!Directory.Exists(original_node_srcdir))
            {
                Console.WriteLine(" NOT FOUND! ");
            }
            else
            {
                Console.WriteLine("FOUND");
            }
            //
            Console.Write("espresso_srcdir: " + espresso_srcdir + " ->");
            if (!Directory.Exists(espresso_srcdir))
            {
                Console.WriteLine(" NOT FOUND! ");
            }
            else
            {
                Console.WriteLine("FOUND");
            }

            Console.WriteLine("======================");
        }

        static bool DisplayMainMenu()
        {
            Console.WriteLine("Choose What to do... (and press ENTER)");
            Console.WriteLine();
            Console.WriteLine(" 0 : Exit");
            Console.WriteLine(" 1 : Apply Patches");
            Console.WriteLine();
            //wait
            string userReadLine = Console.ReadLine();
            int selected_menuNum;

            if (!int.TryParse(userReadLine, out selected_menuNum))
            {
                //with tail call
                return true;
            }
            //
            switch (selected_menuNum)
            {
                case 0:
                    Console.WriteLine("bye!");
                    return false; //exit
                case 1:
                    ApplyPatches();
                    break;
            }
            return true;
        }
        static void ApplyPatches(SimpleAction next = null)
        {
            Console.WriteLine("ApplyPatches ....");
            switch (currentOS)
            {
                case PatcherOS.Windows:
                    {
                        var patcher = new WindowsPatcher();
                        patcher.PatchSubFolder = "node_patches/node7.10_modified";
                        patcher.Setup(@"C:\projects\node-v7.10.0", //specific target 
                          @"D:\projects\CompilerKit\Espresso",
                          "release dll nosign nobuild");

                        //we will build it manually with visual studio

                        Console.WriteLine("Building ...");
                        patcher.Configure(() =>
                        {
                            if (!patcher.ConfigHasSomeErrs)
                            {
                                patcher.DoPatch();
                                Console.WriteLine("Finish!");
                                if (next != null)
                                {
                                    next();
                                }
                            }
                        });
                    }
                    break;
                case PatcherOS.Mac:
                    {
                        var patcher = new LinuxAndMacPatcher();
                        patcher.PatchSubFolder = "node_patches/node7.10_modified";
                        patcher.Setup(@"../../../node-v7.10.0", //specific target 
                          @"../../../Espresso",
                          "--dest-cpu=x64 --shared --xcode");
                        Console.WriteLine("Building ...");

                        //patch before configure
                        patcher.PatchGyp(() =>
                        {
                            patcher.DoPatch();
                            Console.WriteLine("Finish!");
                            if (next != null)
                            {
                                next();
                            }
                        });

                    }
                    break;
                case PatcherOS.Linux:
                    {
                        var patcher = new LinuxAndMacPatcher();
                        patcher.PatchSubFolder = "node_patches/node7.10_modified";
                        patcher.Setup(@"../../../node-v7.10.0", //specific target 
                          @"../../../Espresso",
                          "x64 --shared");
                        Console.WriteLine("Building ...");
                        //
                        patcher.PatchGyp(() =>
                        {
                            patcher.DoPatch();
                            Console.WriteLine("Finish!");
                            if (next != null)
                            {
                                next();
                            }
                        });
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}