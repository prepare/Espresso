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

             
            //TestNodeVM_Example();
            //TestNodeFeature_OS_Example1();
            //TestNodeFeature_OS_Example2();
            //TestNodeFeature_DNS_Example();
            //TestNodeFeature_Internationalization_Example();
            TestNodeFeature_Url_Example();

            //TestSocketIO_ChatExample(); 
        }
       


        static void TestNodeVM_Example()
        {

            //https://nodejs.org/dist/latest-v11.x/docs/api/vm.html
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

            NodeJsEngineHelper.Run(() =>
            {
                return @"
                     
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
                    
                    ";
            });
            string userInput = Console.ReadLine();

        }
        static void TestNodeFeature_OS_Example1()
        {
            //https://nodejs.org/dist/latest-v11.x/docs/api/os.html
#if DEBUG
            JsBridge.dbugTestCallbacks();
#endif
            //------------ 

            //example1: just show value
            NodeJsEngineHelper.Run(() =>
            {
                return @"                     
                    const os = require('os'); 
                    console.log('arch = '+ os.arch());
                    console.log('cpus = '+ JSON.stringify(os.cpus()));
                    console.log('hostname='+ os.hostname());
                    ";
            });
            string userInput = Console.ReadLine();
        }
        static void TestNodeFeature_OS_Example2()
        {
            //https://nodejs.org/dist/latest-v11.x/docs/api/os.html
#if DEBUG
            JsBridge.dbugTestCallbacks();
#endif
            //------------ 

            ////example2: get value from node js 
            OsInfo myOsInfo = new OsInfo();
            NodeJsEngineHelper.Run(ss =>
            {
                ss.SetExternalObj("my_osInfo", myOsInfo);

                return @"                     
                    const os = require('os');                      
                    my_osInfo.Arch = os.arch();
                    my_osInfo.Hostname = os.hostname();
                    ";
            });
            Console.WriteLine("arch=" + myOsInfo.Arch);
            Console.WriteLine("hostname=" + myOsInfo.Hostname);
            string userInput = Console.ReadLine();
        }

        [JsType]
        class OsInfo
        {
            [JsProperty]
            public string Arch { get; set; }
            [JsProperty]
            public string Hostname { get; set; }
        }
        //--------------------------
        static void TestNodeFeature_DNS_Example()
        {
            //https://nodejs.org/dist/latest-v11.x/docs/api/dns.html
#if DEBUG
            JsBridge.dbugTestCallbacks();
#endif
            //------------ 

            //example1: just show value
            NodeJsEngineHelper.Run(() =>
            {
                return @"                     
                    const dns = require('dns');

                    dns.lookup('iana.org', (err, address, family) => {
                        console.log('address: %j family: IPv%s', address, family);
                    });
                    
                    ";
            });
            string userInput = Console.ReadLine();
        }

        //--------------------------
        static void TestNodeFeature_Internationalization_Example()
        {
            //https://nodejs.org/dist/latest-v11.x/docs/api/intl.html
#if DEBUG
            JsBridge.dbugTestCallbacks();
#endif
            //------------ 

            //example1: just show value
            NodeJsEngineHelper.Run(() =>
            {
                return @"                     
                    const january = new Date(9e8);
                    const english = new Intl.DateTimeFormat('en', { month: 'long' });
                    const spanish = new Intl.DateTimeFormat('es', { month: 'long' });

                    console.log(english.format(january));
                    // Prints 'January'
                    console.log(spanish.format(january));
                                    // Prints 'M01' on small-icu
                                    // Should print enero

                ";
            });
            string userInput = Console.ReadLine();
        }

        static void TestNodeFeature_Url_Example()
        {
            //https://nodejs.org/dist/latest-v11.x/docs/api/url.html
#if DEBUG
            JsBridge.dbugTestCallbacks();
#endif
            //------------ 

            //example1: just show value
            NodeJsEngineHelper.Run(() =>
            {
                return @"            

                        const url = require('url');
                        const myURL1 =
                            url.parse('https://user:pass@sub.example.com:8080/p/a/t/h?query=string#hash');

                        console.log(JSON.stringify(myURL1));

                        //
                        console.log('\r\n');
                        console.log('\r\n');
                        //
                        const myURL2 =
                            new URL('https://user:pass@sub.example.com:8080/p/a/t/h?query=string#hash');
                        console.log(myURL2.host+'\r\n');
                        console.log(myURL2.href+'\r\n');
                        console.log(myURL2.hostname +'\r\n');
                ";
            });
            string userInput = Console.ReadLine();
        }

        private static void Proc_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {

        }

        private static void Proc_ErrorDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {

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

            NodeJsEngineHelper.Run(ss => File.ReadAllText("index.js"));

            string userInput = Console.ReadLine();
        }
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern IntPtr LoadLibrary(string dllname);
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern int GetLastError();
    }
}
