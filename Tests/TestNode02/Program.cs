//MIT, 2015-2017, EngineKit, brezza92
using System;
using System.IO;
using Espresso;
using Espresso.NodeJsApi;

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
            string libEspr = @"../../../node-v13.5.0/out/Debug/node.dll";
            //-----------------------------------
            //2. load node.dll
            //-----------------------------------  
            IntPtr intptr = LoadLibrary(libEspr);
            int errCode = GetLastError();
            int libesprVer = JsBridge.LibVersion;


            TestNodeJs_NApi(); //
            //TestNodeJs_Buffer();
            //TestNodeVM_Example();
            //TestNodeFeature_OS_Example1();
            //TestNodeFeature_OS_Example2();
            //TestNodeFeature_DNS_Example();
            //TestNodeFeature_Internationalization_Example();
            // TestNodeFeature_Url_Example();

            //TestSocketIO_ChatExample(); 
        }

        static void TestNodeJs_NApi()
        {
#if DEBUG
            JsBridge.dbugTestCallbacks();
#endif

            var test_instance = new MyNodeJsApiBridgeTestInstance();
            NodeJsEngineHelper.Run(ss =>
            {
                //for general v8
                //see more https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/DataView/getUint8

                test_instance.SetJsContext(ss.Context);
                ss.SetExternalObj("test_instance", test_instance);
                return @"                  
                   
                    var arr= test_instance.CreateArrayFromDotnetSide();
                    console.log(arr);

                    var str= test_instance.CreateString('user_a001_test');
                    console.log(str);

                    var externalMem = test_instance.CreateExternalBuffer();
                    console.log(externalMem);
                    externalMem=null;
                     
                    test_instance.TestRunScript();
                ";
            });

            string userInput = Console.ReadLine();
        }

        class MyNodeJsApiBridgeTestInstance
        {
            JsContext _context;
            NapiEnv _env;

            public void SetJsContext(JsContext context)
            {
                _context = context;
                _env = _context.GetJsNodeNapiEnv();
            }
            public NodeJsArray CreateArrayFromDotnetSide()
            {
                //return "hello!";
                //NodeJsArray arr = _env.CreateArray();
                return _env.CreateArray(2);
            }
            public NodeJsExternalBuffer CreateExternalBuffer()
            {
                //this method will be called from nodejs side
                //we alloc memory from .net side and set this unmanged mem to node js side

                //TODO: implement dispose
                //this is an example 
                MyNativeMemBuffer myNativeMemBuffer = MyNativeMemBuffer.AllocNativeMem(100);
                return _env.CreateExternalBuffer(myNativeMemBuffer);
            }
            public NodeJsString CreateString(string user_input)
            {
                return _env.CreateString("hello! " + user_input + " , from .net side");
            }
            public void TestRunScript()
            {
                NodeJsValue result = _env.RunScript("(function(){return 1+1;})()");
                _env.Typeof(result.UnmanagedPtr);

            }
        }


        static void TestNodeJs_Buffer()
        {
            //https://nodejs.org/api/buffer.html
#if DEBUG
            JsBridge.dbugTestCallbacks();
#endif
            //------------ 

            ////example2: get value from node js              
            MyBufferBridge myBuffer = new MyBufferBridge();

            //for nodejs's Buffer
            //NodeJsEngineHelper.Run(ss =>
            //{
            //    //for node js
            //    ss.SetExternalObj("myBuffer", myBuffer);
            //    return @"                     
            //        const buf1 = Buffer.alloc(20); 
            //        buf1.writeUInt8(0, 0);
            //        buf1.writeUInt8(1, 1);
            //        buf1.writeUInt8(2, 2);
            //        //-----------
            //        myBuffer.SetBuffer(buf1);
            //        console.log('before:');
            //        console.log(buf1);

            //        if(myBuffer.HaveSomeNewUpdate()){
            //            myBuffer.UpdateBufferFromDotNetSide();
            //            console.log('after:');
            //            console.log(buf1);    
            //        }else{
            //            console.log('no data');
            //        }                    
            //    ";
            //});

            //for v8
            NodeJsEngineHelper.Run(ss =>
            {
                //for general v8
                //see more https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/DataView/getUint8

                ss.SetExternalObj("myBuffer", myBuffer);
                return @"                     
                    const buf1 = new ArrayBuffer(20);
                    const view = new DataView(buf1);
                    view.setUint8(0, 0); 
                    view.setUint8(1, 1);
                    view.setUint8(2, 2);
                    //-----------
                    myBuffer.SetBuffer(buf1);
                    console.log('before:');
                    console.log(buf1);

                    if(myBuffer.HaveSomeNewUpdate()){
                        myBuffer.UpdateBufferFromDotNetSide();
                        console.log('after:');
                        console.log(buf1);    
                    }else{
                        console.log('no data');
                    }                    
                ";
            });
            int buffLen = myBuffer.Length;

            string userInput = Console.ReadLine();
        }


        class MyBufferBridge
        {
            JsObject _buffer;
            JsBuffer _nodeJsBuffer;
            int _bufferLen;
            byte[] _memBuffer;

            public MyBufferBridge()
            {
            }
            public void SetBuffer(JsObject buffer)
            {
                _buffer = buffer;
                _nodeJsBuffer = new JsBuffer(buffer);
                _bufferLen = _nodeJsBuffer.GetBufferLen();
            }
            public bool HaveSomeNewUpdate()
            {
                //TEST ONLY
                //return false;
                return true;
            }
            public void UpdateBufferFromDotNetSide()
            {
                //test write data back
                byte[] newOutputData = new byte[100];
                for (int i = 0; i < _bufferLen; ++i)
                {
                    newOutputData[i] = 100;
                }
                unsafe
                {
                    fixed (byte* ptr0 = &newOutputData[0])
                    {
                        //_nodeJsBuffer.SetBuffer((IntPtr)ptr0, 10);//write data start at   offset 0 on dest
                        _nodeJsBuffer.SetBuffer((IntPtr)ptr0, 2, 10); //write data start at 0 offset 2 on dest
                    }
                }
            }
            public void CopyBufferFromNodeJs()
            {
                unsafe
                {
                    _memBuffer = new byte[_bufferLen];
                    fixed (byte* ptr0 = &_memBuffer[0])
                    {
                        _nodeJsBuffer.CopyBuffer((IntPtr)ptr0, _bufferLen);
                    }
                }

            }
            public int Length => _bufferLen;
            public byte[] CopyMem() => _memBuffer;
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
        static extern IntPtr GetProcAddress(IntPtr libHandle, string funcName);

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern int GetLastError();


    }
}
