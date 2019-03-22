//MIT, 2015-2017, EngineKit, brezza92
using System;
using System.IO;
using Espresso;

namespace TestNode01
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //-----------------------------------
            //1.
            //after we build nodejs in dll version
            //we will get node.dll
            //then just copy it to another name 'libespr'   
            string currentdir = System.IO.Directory.GetCurrentDirectory();

            string libEspr = @"../../../node-v11.12.0/Release/libespr.dll"; //previous version 8.4.0
            if (File.Exists(libEspr))
            {
                //delete the old one
                File.Delete(libEspr);
            }
            File.Copy(
               @"../../../node-v11.12.0/Release/node.dll", // //previous version 8.4.0
               libEspr);

            IntPtr intptr = LoadLibrary(libEspr);
            int errCode = GetLastError();
            int libesprVer = JsBridge.LibVersion;


            TestNodeVM_Example();

            //TestSocketIO_ChatExample(); 
        }
        static void TestSocketIO_ChatExample()
        {
            //change working dir to target app and run 
            //test with socket.io's chat sample
            System.IO.Directory.SetCurrentDirectory(@"../../../socket.io/examples/chat");

#if DEBUG
            JsBridge.dbugTestCallbacks();
#endif
            //------------ 
            NodeJsEngine.Run((eng, ctx) =>
            {
                //-------------
                //this LibEspressoClass object is need,
                //so node can talk with us,
                //-------------
                JsTypeDefinition jstypedef = new JsTypeDefinition("LibEspressoClass");
                jstypedef.AddMember(new JsMethodDefinition("LoadMainSrcFile", args =>
                 {
                     //since this is sample socket io app
                     string filedata = File.ReadAllText("index.js");
                     args.SetResult(filedata);
                 }));
                jstypedef.AddMember(new JsMethodDefinition("C", args =>
                 {

                     args.SetResult(true);
                 }));
                jstypedef.AddMember(new JsMethodDefinition("E", args =>
                 {
                     args.SetResult(true);
                 }));
                if (!jstypedef.IsRegisterd)
                {
                    ctx.RegisterTypeDefinition(jstypedef);
                }
                //----------
                //then register this as x***       
                //this object is just an instance for reference        
                ctx.SetVariableFromAny("LibEspresso",
                   ctx.CreateWrapper(new object(), jstypedef));
            });

            string userInput = Console.ReadLine();
        }


        static void TestNodeVM_Example()
        {

            //from https://nodejs.org/dist/latest-v10.x/docs/api/vm.html
            //const vm = require('vm');

            //const x = 1;

            //const sandbox = { x: 2 };
            //vm.createContext(sandbox); // Contextify the sandbox.

            //const code = 'x += 40; var y = 17;';
            //// x and y are global variables in the sandboxed environment.
            //// Initially, x has the value 2 because that is the value of sandbox.x.
            //vm.runInContext(code, sandbox);

            //console.log(sandbox.x); // 42
            //console.log(sandbox.y); // 17

            //console.log(x); // 1; y is not defined.


            //-----------

#if DEBUG
            JsBridge.dbugTestCallbacks();
#endif
            //------------ 


            NodeJsEngine.Run((eng, ctx) =>
            {
                //-------------
                //this LibEspressoClass object is need,
                //so node can talk with us,
                //-------------
                JsTypeDefinition jstypedef = new JsTypeDefinition("LibEspressoClass");
                jstypedef.AddMember(new JsMethodDefinition("LoadMainSrcFile", args =>
                {
                    //since this is sample socket io app
                    string filedata = @"
                    (function(){
                    const vm = require('vm');

                    const x = 1;

                    const sandbox = { x: 2 };
                    vm.createContext(sandbox); // Contextify the sandbox.

                    const code = 'x += 40; var y = 17;';
                    // x and y are global variables in the sandboxed environment.
                    // Initially, x has the value 2 because that is the value of sandbox.x.
                    vm.runInContext(code, sandbox);

                    console.log(sandbox.x); // 42
                    console.log(sandbox.y); // 17

                    console.log(x); // 1; y is not defined.
                    })();
                    ";
                    args.SetResult(filedata);
                }));

                if (!jstypedef.IsRegisterd)
                {
                    ctx.RegisterTypeDefinition(jstypedef);
                }

                //----------
                //then register this as x***       
                //this object is just an instance for reference        
                ctx.SetVariableFromAny("LibEspresso", ctx.CreateWrapper(new object(), jstypedef));
            });

            string userInput = Console.ReadLine();

        }
        private static void Proc_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {

        }

        private static void Proc_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {

        }

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern IntPtr LoadLibrary(string dllname);
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern int GetLastError();
    }
}
