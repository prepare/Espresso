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
            Patcher patcher = new Patcher();
            patcher.Setup(@"C:\projects\node-v7.10.0", //specific target 
                          @"D:\projects\CompilerKit\Espresso",
                          "x64 release");
            //------------------------------------ 
            patcher.DoPatch();
            patcher.InitBuild((s, e) =>
            {
                //finish init build
                //then => do patch 

            });
            Console.WriteLine("Building ...");
            string userReadLine = Console.ReadLine();

        }
    }
}