//MIT, 2017, EngineKit
using System;
namespace EasePatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Espresso's EasePatcher");
            Console.WriteLine("Start Build....");
            //------------------------------------    
            switch (PatcherBase.GetPatcherOS())
            {
                case PatcherOS.Windows:
                    {
                        var patcher = new WindowsPatcher();
                        patcher.PatchSubFolder = "node_patches/node7.10_modified";
                        patcher.Setup(@"C:\projects\node-v7.10.0", //specific target 
                          @"D:\projects\CompilerKit\Espresso",
                          "release nosign nobuild"); //we will build it manually with visual studio

                        Console.WriteLine("Building ...");
                        patcher.Configure(() =>
                        {
                            if (!patcher.ConfigHasSomeErrs)
                            {
                                patcher.DoPatch();
                                Console.WriteLine("Finish!");
                            }
                        });
                    }
                    break;
                case PatcherOS.Mac:
                case PatcherOS.Linux:
                    {
                        var patcher = new LinuxAndMacPatcher();
                        patcher.PatchSubFolder = "node_patches/node7.10_modified";
                        patcher.Setup(@"~/Downloads/node-v7.10.0", //specific target 
                          @"~/Downloads/Espresso",
                          "release");
                        Console.WriteLine("Building ...");
                        patcher.Build(() =>
                        {
                            patcher.DoPatch();
                            Console.WriteLine("Finish!");
                        });
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }
            
            string userReadLine = Console.ReadLine();
        }
    }
}