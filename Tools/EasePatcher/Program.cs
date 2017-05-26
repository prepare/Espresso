//MIT, 2017, EngineKit
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
namespace EasePatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Espresso's EasePatcher");
            Console.WriteLine("Start Build....");
            //------------------------------------ 
            Patcher patcher = new Patcher();
            patcher.Setup(@"C:\projects\node-v7.10.0",
                          @"D:\projects\CompilerKit\Espresso",
                          "x86 release");
            //------------------------------------ 

            patcher.InitBuild((s, e) =>
            {
                //finish init build
                //then => do patch
                patcher.DoPatch();

            });

            string userReadLine = Console.ReadLine();

        }
    }
}