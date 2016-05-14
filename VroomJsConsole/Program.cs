using System;
using System.IO;
using VroomJs;

namespace VRoomJsConsoleTest
{
    class Program
    {
        class TestMe1
        {
            public int B()
            {
                return 100;
            }
            public bool C()
            {
                return true;
            }
        }

        static void Main(string[] args)
        {
            VroomJs.JsBridge.V8Init();
            Menu();
            Console.Read();
        }

        static void Menu()
        {
            int num = 0;
            Console.WriteLine("Select test case");
            Console.WriteLine("[1] Test1");
            Console.WriteLine("[2] Test2");
            //Console.WriteLine("[3] Exit");
            num = Console.Read();
            switch (num)
            {
                case 49:
                    TestCase1();//ascii 49= '1'
                    break;
                case 50:
                    TestCase2();
                    break;
                default:
                    Console.WriteLine("---[Please press 1-2]---");
                    break;
            }
        }

        static void TestCase1()
        {
            JsBridge.dbugTestCallbacks();

            JsTypeDefinition jstypedef = new JsTypeDefinition("AA");
            jstypedef.AddMember(new JsMethodDefinition("B", args =>
            {
                args.SetResult(100);
            }));
            jstypedef.AddMember(new JsMethodDefinition("C", args =>
            {
                args.SetResult(true);
            }));
            //===============================================================
            //create js engine and context
            using (JsEngine engine = new JsEngine())
            using (JsContext ctx = engine.CreateContext())
            {

                if (!jstypedef.IsRegisterd)
                {
                    ctx.RegisterTypeDefinition(jstypedef);
                }
                GC.Collect();
                System.Diagnostics.Stopwatch stwatch = new System.Diagnostics.Stopwatch();
                stwatch.Start();

                TestMe1 t1 = new TestMe1();
                var proxy = ctx.CreateWrapper(t1, jstypedef);

                for (int i = 2000; i >= 0; --i)
                {
                    ctx.SetVariableFromAny("x", proxy);
                    object result = ctx.Execute("(function(){if(x.C()){return  x.B();}else{return 0;}})()");
                }
                stwatch.Stop();

                Console.WriteLine("met1 template:" + stwatch.ElapsedMilliseconds.ToString());
                //Assert.That(result, Is.EqualTo(100));
            }
        }

        static void TestCase2()
        {
            JsBridge.dbugTestCallbacks();

            //create js engine and context

            using (JsEngine engine = new JsEngine())
            using (JsContext ctx = engine.CreateContext())
            {
                GC.Collect();
                System.Diagnostics.Stopwatch stwatch = new System.Diagnostics.Stopwatch();
                stwatch.Start();

                TestMe1 t1 = new TestMe1();

                for (int i = 2000; i >= 0; --i)
                {
                    ctx.SetVariableFromAny("x", t1);
                    object result = ctx.Execute("(function(){if(x.C()){return  x.B();}else{return 0;}})()");
                }
                stwatch.Stop();
                Console.WriteLine("met2 managed reflection:" + stwatch.ElapsedMilliseconds.ToString());
                //Assert.That(result, Is.EqualTo(100)); 
            }
        }
    }
}

