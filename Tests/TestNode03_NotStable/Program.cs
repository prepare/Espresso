//
//WARNING! , experiment, from @dpwhittaker,  see https://github.com/prepare/Espresso/issues/40
//
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Espresso;
using System.Diagnostics;

namespace Test03_NotStable
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("test only!");
            ScriptRuntime3 scriptRuntime3 = new ScriptRuntime3();
            //----------
            ////test1: execute from  main thread
            //for (int i = 0; i < 100; ++i)
            //{
            //    Dictionary<string, object> scriptPars = new Dictionary<string, object>();
            //    scriptRuntime3.Execute("LibEspresso.Log('ok" + i + "');", scriptPars, (result) =>
            //        {
            //            //when finish or error this will be called
            //            var exception = result as Exception;
            //            if (exception != null)
            //            {
            //                //this is error
            //            }
            //        });
            //}
            //----------
            ////test2: execute from thread pool queue
            //for (int i = 0; i < 100; ++i)
            //{
            //    int snapNum = i;
            //    ThreadPool.QueueUserWorkItem(o =>
            //    {
            //        Dictionary<string, object> scriptPars = new Dictionary<string, object>();
            //        scriptRuntime3.Execute("LibEspresso.Log('ok" + snapNum + "');", scriptPars, (result) =>
            //        {
            //            //when finish or error this will be called
            //            var exception = result as Exception;
            //            if (exception != null)
            //            {
            //                //this is error
            //            }
            //        });
            //    });
            //}
            //----------
            //test3: async- await
            RunJsTaskWithAsyncAwait(scriptRuntime3);
            Console.ReadLine();
        }
        static async void RunJsTaskWithAsyncAwait(ScriptRuntime3 scriptRuntime3)
        {
            {
                //
                //await 
                //
                object result1 = await scriptRuntime3.ExecuteAsync("(function(){LibEspresso.Log('ok" + 1 + "');return 1;})()", new Dictionary<string, object>());
                Console.WriteLine("await result:" + result1);
                //
                object result2 = await scriptRuntime3.ExecuteAsync("(function(){LibEspresso.Log('ok" + 2 + "');return 2;})()", new Dictionary<string, object>());
                Console.WriteLine("await result:" + result2);
                //
                object result3 = await scriptRuntime3.ExecuteAsync("(function(){LibEspresso.Log('ok" + 3 + "');return 3;})()", new Dictionary<string, object>());
                Console.WriteLine("await result:" + result3);
                //
                object result4 = await scriptRuntime3.ExecuteAsync("(function(){LibEspresso.Log('ok" + 4 + "');return 4;})()", new Dictionary<string, object>());
                Console.WriteLine("await result:" + result4);
            }
            //----------
            {
                //async ...
                scriptRuntime3.ExecuteAsync("(function(){LibEspresso.Log('ok" + 1 + "');return 1;})()", new Dictionary<string, object>());
                //
                scriptRuntime3.ExecuteAsync("(function(){LibEspresso.Log('ok" + 2 + "');return 2;})()", new Dictionary<string, object>());
                //
                scriptRuntime3.ExecuteAsync("(function(){LibEspresso.Log('ok" + 3 + "');return 3;})()", new Dictionary<string, object>());

                //
                scriptRuntime3.ExecuteAsync("(function(){LibEspresso.Log('ok" + 4 + "');return 4;})()", new Dictionary<string, object>());

            }
        }
    }

    //-----------------


    public class ScriptRuntime3
    {
        private JsEngine _engine;
        private JsContext _context;
        private readonly Thread _jsThread;
        private readonly ConcurrentQueue<System.Action> workQueue = new ConcurrentQueue<System.Action>();
        public ScriptRuntime3()
        {
            _jsThread = new Thread(ScriptThread);
            _jsThread.Start();
        }
        private void ScriptThread(object obj)
        {
            workQueue.Enqueue(InitializeJsGlobals);
            NodeJsEngine.Run(Debugger.IsAttached ? new string[] { "--inspect", "hello.espr" } : new string[] { "hello.espr" },
            (eng, ctx) =>
            {
                _engine = eng;
                _context = ctx;

                JsTypeDefinition jstypedef = new JsTypeDefinition("LibEspressoClass");
                jstypedef.AddMember(new JsMethodDefinition("LoadMainSrcFile", args =>
                {
                    args.SetResult(@"
function MainLoop() {
    LibEspresso.Next();
    setImmediate(MainLoop);
}
MainLoop();");
                }));

                jstypedef.AddMember(new JsMethodDefinition("Log", args =>
                {
                    Console.WriteLine(args.GetArgAsObject(0));
                }));

                jstypedef.AddMember(new JsMethodDefinition("Next", args =>
                {
                    //call from js server
                    System.Action work;
                    if (workQueue.TryDequeue(out work))
                    {
                        work();
                    }
                }));
                _context.RegisterTypeDefinition(jstypedef);
                _context.SetVariableFromAny("LibEspresso", _context.CreateWrapper(new object(), jstypedef));
            });
        }
        public void Execute(string script, Dictionary<string, object> processData, Action<object> doneWithResult)
        {
            workQueue.Enqueue(() =>
            {
                foreach (var kp in processData)
                    _context.SetVariableFromAny(kp.Key, kp.Value);
                //----------------          
                object result = null;
                try
                {
                    result = _context.Execute(script);
                }
                catch (JsException ex)
                {
                    //set result as exception
                    result = ex;
                }
                //--------
                //notify result back
                doneWithResult(result);
            });
        }
        public Task<object> ExecuteAsync(string script, Dictionary<string, object> processData)
        {
            var tcs = new TaskCompletionSource<object>();
            Execute(script, processData, result =>
            {
                tcs.SetResult(result);
            });
            return tcs.Task;
        }

        void InitializeJsGlobals()
        {
            //-----------------------------------
            //1.
            //after we build nodejs in dll version
            //we will get node.dll
            //then just copy it to another name 'libespr'   
            string currentdir = System.IO.Directory.GetCurrentDirectory();
            string libEspr = @"../../../node-v8.4.0/Release/libespr.dll";
            //if (File.Exists(libEspr))
            //{
            //    //delete the old one
            //    File.Delete(libEspr);
            //}
            //File.Copy(
            //   @"../../../node-v8.4.0/Release/node.dll", //from
            //   libEspr);
            //-----------------------------------
            //2. load libespr.dll (node.dll)
            //-----------------------------------  
            IntPtr intptr = LoadLibrary(libEspr);
            int errCode = GetLastError();
            int libesprVer = JsBridge.LibVersion;
#if DEBUG
            JsBridge.dbugTestCallbacks();
#endif
        }

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern IntPtr LoadLibrary(string dllname);
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        static extern int GetLastError();
    }









}
