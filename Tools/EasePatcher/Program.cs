//MIT, 2017, EngineKit
using System;
using System.IO;

namespace EasePatcher
{
    class Program
    {

        static string current_node_version = "node-v8.4.0";
        static string patch_subdir = "node_patches/node8.4.0_modified";
        //
        static string original_node_srcdir = @"../../../node-v8.4.0";
        static string espresso_srcdir = "../../../Espresso";
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
            Console.WriteLine("current_dir: " + Directory.GetCurrentDirectory());

            switch (currentOS)
            {
                case PatcherOS.Windows:
                    config_pars = "x64 release nosign nobuild"; //default build 
                    break;
                case PatcherOS.Mac:
                    config_pars = "--dest-cpu=x64 --shared --xcode";
                    break;
                case PatcherOS.Linux:
                    config_pars = "--dest-cpu=x64 --shared";
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
            Console.WriteLine("Choose ... (and press ENTER)");
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
                    Console.WriteLine("Bye!");
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
                        patcher.PatchSubFolder = patch_subdir;
                        patcher.Setup(original_node_srcdir,
                                      espresso_srcdir,
                                      config_pars);
                        //
                        Console.WriteLine("Patch and Configure ...");
                        patcher.Configure(() =>
                        {
                            if (!patcher.ConfigHasSomeErrs)
                            {
                                patcher.DoPatch();
                                Console.WriteLine("Finish!");
                                Console.WriteLine("please build with Visual Studio");
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
                        patcher.PatchSubFolder = patch_subdir;
                        patcher.Setup(original_node_srcdir,
                                      espresso_srcdir,
                                      config_pars);
                        //
                        Console.WriteLine("Patch and Configure ...");
                        //patch before configure
                        patcher.PatchGyp(() =>
                        {
                            patcher.DoPatch();
                            Console.WriteLine("Finish!");
                            Console.WriteLine("please build with Xcode");
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
                        patcher.PatchSubFolder = patch_subdir;
                        patcher.Setup(original_node_srcdir,
                                      espresso_srcdir,
                                      config_pars);
                        //
                        Console.WriteLine("Patch and Configure ...");
                        //
                        patcher.PatchGyp(() =>
                        {
                            patcher.DoPatch();
                            Console.WriteLine("Finish!");
                            Console.WriteLine("please build with make");
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