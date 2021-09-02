//MIT, 2015-2017, EngineKit, brezza92
using System;
using System.IO;
using Espresso;

namespace TestNode01
{
    static class Program
    {

        //  [DllImport(JsBridge.LIB_NAME, CallingConvention = CallingConvention.Cdecl)]
        //        static extern int RunJsEngine(int argc, string[] args, NativeEngineSetupCallback engineSetupCb, NativeEngineClosingCallback engineClosingCb);

        delegate int RunJsEngineDel(int argc,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPArray)] string[] args,
            IntPtr engineSetupCb,
            IntPtr engineClosingCb);

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


            string libEspr = @"../../../node-v16.8.0/out/Release/node.dll";
            //-----------------------------------
            //2. load node.dll
            //----------------------------------- 
            //string libEspr = "libespr.dll";
            IntPtr hModule = LoadLibrary(libEspr);
            int errCode = GetLastError();
            int libesprVer = JsBridge.LibVersion;

#if DEBUG
            JsBridge.dbugTestCallbacks();
            IntPtr proc = GetProcAddress(hModule, "RunJsEngine");
            //RunJsEngineDel del1 = (RunJsEngineDel)System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer(proc,
            //   typeof(RunJsEngineDel));
            //del1(3, new string[] { "node", "--inspect", "hello.espr" }, IntPtr.Zero, IntPtr.Zero);
#endif
            //------------
            NodeJsEngineHelper.Run(new string[] { "--inspect", "hello.espr" },
                ss => @"
                    const http2 = require('http2');
                    const fs = require('fs');

                    const server = http2.createSecureServer({
                      key: fs.readFileSync('localhost-privkey.pem'),
                      cert: fs.readFileSync('localhost-cert.pem')
                    });
                    server.on('error', (err) => console.error(err));
                    server.on('socketError', (err) => console.error(err));

                    server.on('stream', (stream, headers) => {
                      // stream is a Duplex
                      stream.respond({
                        'content-type': 'text/html',
                        ':status': 200
                      });
                      stream.end('<h1>Hello World, EspressoND, node 16.8.0</h1>');
                    });

                    server.listen(8443);
                    ");
            string userInput = Console.ReadLine();
        }


        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern IntPtr LoadLibrary(string dllname);
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern int GetLastError();
    }
}
