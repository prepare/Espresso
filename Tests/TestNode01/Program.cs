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
            string libEspr = @"../../../node-v10.15.1/Release/libespr.dll"; //previous version 8.4.0
            if (File.Exists(libEspr))
            {
                //delete the old one
                File.Delete(libEspr);
            }
            File.Copy(
               @"../../../node-v10.15.1/Release/node.dll", // //previous version 8.4.0
               libEspr);
            //-----------------------------------
            //2. load libespr.dll (node.dll)
            //----------------------------------- 


            IntPtr intptr = LoadLibrary(libEspr);
            int errCode = GetLastError();
            int libesprVer = JsBridge.LibVersion;
#if DEBUG
            JsBridge.dbugTestCallbacks();
#endif
            //------------ 
            JsEngine.RunJsEngine(
                new string[] { "--inspect", "hello.espr" },
                (IntPtr nativeEngine, IntPtr nativeContext) =>
            {

                JsEngine eng = new JsEngine(nativeEngine);
                JsContext ctx = eng.CreateContext(nativeContext);
                //-------------
                //this LibEspressoClass object is needed,
                //so node can talk with us,
                //-------------

                JsTypeDefinition jstypedef = new JsTypeDefinition("LibEspressoClass");
                jstypedef.AddMember(new JsMethodDefinition("LoadMainSrcFile", args =>
                {


                    //handle main src loading here 
                    //example 2:
                    //test http2 --server side
                    //(from https://nodejs.org/api/http2.html#http2_server_side_example)
                    string filedata =
                    @" const http2 = require('http2');
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
                      stream.end('<h1>Hello World, EspressoND, node 10.15.1</h1>');
                    });

                    server.listen(8443);
                    ";
                    args.SetResult(filedata);
                    //-------------------------

                    //
                    //example 1: simple http server
                    //
                    ////handle main src loading here
                    //string filedata = @"   var http = require('http');
                    //                            (function(){
                    //                             console.log('hello from Espresso-ND');
                    //                             var server = http.createServer(function(req, res) {
                    //                                res.writeHead(200);
                    //                                res.end('Hello! from Espresso-ND');
                    //                                });
                    //                                server.listen(8080,'localhost');
                    //                            })();";
                    //args.SetResult(filedata);
                }));
                ctx.RegisterTypeDefinition(jstypedef);
                //----------
                //then register this as x***       
                //this object is just an instance for reference        
                ctx.SetVariableFromAny("LibEspresso",
                      ctx.CreateWrapper(new object(), jstypedef));
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
