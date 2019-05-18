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

            string libEspr = "libespr.dll";

            ////string libEspr = @"../../../node-v11.12.0/Release/libespr.dll"; //previous version 8.4.0
            //if (File.Exists(libEspr))
            //{
            //    //delete the old one
            //    File.Delete(libEspr);
            //}
            //File.Copy(
            //   @"../../../node-v11.12.0/Release/node.dll", // //previous version 8.4.0
            //   libEspr);

            //-----------------------------------
            //2. load libespr.dll (node.dll)
            //----------------------------------- 
            //string libEspr = "libespr.dll";
            IntPtr intptr = LoadLibrary(libEspr);
            int errCode = GetLastError();
            int libesprVer = JsBridge.LibVersion;
#if DEBUG
            JsBridge.dbugTestCallbacks();
#endif  
            //------------
            //http2 client
            NodeJsEngineHelper.Run(
                ss => @"const http2 = require('http2');
                        const fs = require('fs');
                        const client = http2.connect('https://localhost:8443', {
                          ca: fs.readFileSync('localhost-cert.pem')
                        });
                        client.on('error', (err) => console.error(err));

                        const req = client.request({ ':path': '/' });

                        req.on('response', (headers, flags) => {
                          for (const name in headers) {
                            console.log(`${name}: ${headers[name]}`);
                          }
                        });

                        req.setEncoding('utf8');
                        let data = '';
                        req.on('data', (chunk) => { data += chunk; });
                        req.on('end', () => {
                          console.log(`\n${data}`);
                          client.close();
                        });
                        req.end();
                    ");
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
