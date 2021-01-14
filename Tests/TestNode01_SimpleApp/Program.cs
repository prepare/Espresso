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


            string libEspr = @"../../../node-v15.5.1/out/Release/node.dll";
            //-----------------------------------
            //2. load node.dll
            //----------------------------------- 
            
            IntPtr intptr = LoadLibrary(libEspr);
            int errCode = GetLastError();
            int libesprVer = JsBridge.LibVersion;
#if DEBUG
            JsBridge.dbugTestCallbacks();
#endif
            //------------
            MyApp myApp = new MyApp();
            NodeJsEngineHelper.Run(new string[] { "--inspect", "hello.espr" },
               ss =>
               {
                   ss.SetExternalObj("myApp", myApp);

                   return @" const http2 = require('http2');
                    const fs = require('fs');
                    
                    const server = http2.createSecureServer({
                      key: fs.readFileSync('localhost-privkey.pem'),
                      cert: fs.readFileSync('localhost-cert.pem')
                    });
                    server.on('error', (err) => console.error(err));
                    server.on('socketError', (err) => console.error(err));

                    server.on('stream', (stream, headers) => {
                      // stream is a Duplex
                      //const method = headers[':method'];
                      //const path = headers[':path'];  
                       
                      //let result=  myApp.HandleRequest(method,path);
                      
                      //stream.respond({
                      //  'content-type': 'text/html',
                      //  ':status': 200
                      //});
                      stream.end('<h1>Hello World, EspressoND, node 15.5.1</h1>'+ myApp.GetMyName());
                      //stream.end(result);
                    });

                    server.listen(8443);
                    console.log('hello!');
                    ";
               });
            string userInput = Console.ReadLine();
        }

        class MyApp
        {
            public string GetMyName()
            {
                return "OKOKO1";
            }
            public byte[] HandleRequest(string method, string path)
            {
                switch (path)
                {
                    case "/version": return System.Text.Encoding.UTF8.GetBytes("1.0");
                    default: return System.Text.Encoding.UTF8.GetBytes("??");
                }
            }
        }
       

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern IntPtr LoadLibrary(string dllname);
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern int GetLastError();
    }
}
