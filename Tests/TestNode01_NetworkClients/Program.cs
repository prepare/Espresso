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
            //http2 client

            //http2 client
            HttpResp httpResp = new HttpResp();

            NodeJsEngineHelper.Run(
                ss =>
                {
                    ss.SetExternalObj("my_resp", httpResp);

                    return @"const http2 = require('http2');
                        const fs = require('fs');
                        const client = http2.connect('https://localhost:8443', {
                          ca: fs.readFileSync('localhost-cert.pem')
                        });
                        client.on('error', (err) => console.error(err));

                        //const req = client.request({ ':path': '/' });
                        const req = client.request({ ':path': '/' ,'body':'123456789'});

                        req.on('response', (headers, flags) => {
                          for (const name in headers) {
                            console.log(`${name}: ${headers[name]}`);
                          }
                        });

                        req.setEncoding('utf8');
                        let data = '';
                        req.on('data', (chunk) => { data += chunk; });
                        req.on('end', () => {
                          //console.log(`\n${data}`);
                          my_resp.Data=data;
                          client.close();
                        });
                        req.end();
                    ";
               });

            string userInput = Console.ReadLine();
            if (httpResp.Data != null)
            {
                Console.WriteLine(httpResp.Data);
            }
        }
        class HttpResp
        {
            public string Data { get; set; }
        }


        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern IntPtr LoadLibrary(string dllname);
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern int GetLastError();
    }
}
