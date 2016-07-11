using System;
using System.IO;
using VroomJs;
using System.Collections.Generic;

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


        static Dictionary<string, TestInfo> testDictionary;

        static void Main(string[] args)
        {

            //VroomJs.JsBridge.LoadV8(@"D:\projects\Espresso\build\Debug\libespr.dll");
            VroomJs.JsBridge.LoadV8(@"D:\projects\Espresso\Release\libespr.dll");
            Menu();
            Console.Read();
        }

        static List<TestInfo> GetTestInfoList()
        {
            var testList = new List<TestInfo>();
            Type t = typeof(Program);
            var mets = t.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            foreach (var met in mets)
            {
                var found = met.GetCustomAttributes(typeof(TestAttribute), false) as TestAttribute[];
                if (found != null && found.Length > 0)
                {
                    TestInfo testInfo = new TestInfo()
                    {
                        Choice = found[0].Choice,
                        Name = found[0].Name,
                        TestMethod = met
                    };
                    testList.Add(testInfo);
                }
            }
            return testList;
        }

        static void Menu()
        {

            //prepare test cases 
            var testList = GetTestInfoList();
            //----------------------------
            testDictionary = new Dictionary<string, TestInfo>();
            foreach (TestInfo t in testList)
            {
                testDictionary.Add(t.Choice, t);
            }

            Console.WriteLine("Select test case, and press Enter");
            Console.WriteLine("[0] Exit");
            foreach (TestInfo t in testList)
            {
                Console.WriteLine("[" + t.Choice + "] " + t.Name);
            }

            string num = Console.ReadLine();
            if (num == "0")
            {
                return;
            }

            TestInfo selectedTest;
            if (testDictionary.TryGetValue(num, out selectedTest))
            {
                //found test
                selectedTest.TestMethod.Invoke(num, new object[0]);
            }
            else
            {
                Console.WriteLine("---[not found, Please press choice (1,2 etc)]---");
            }

        }

        [Test("1", "Test1")]
        static void TestCase1()
        {
#if DEBUG
            JsBridge.dbugTestCallbacks();
#endif

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
                //test value of
                object re = ctx.Execute("(function(){function myNumberType(n) {    this.number = n;}" +
                        "myNumberType.prototype.valueOf = function() {    return this.number;};" +
                        "myObj = new myNumberType(4);" +
                        "return myObj + 3;" +
                        "})()");
                stwatch.Stop();

                Console.WriteLine("met1 template:" + stwatch.ElapsedMilliseconds.ToString());
                //Assert.That(result, Is.EqualTo(100));
            }
        }

        [Test("2", "Test2")]
        static void TestCase2()
        {
#if DEBUG
            JsBridge.dbugTestCallbacks();
#endif
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

