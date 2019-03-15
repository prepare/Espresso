//MIT, 2015-present, WinterDev, EngineKit, brezza92
using System;
using Espresso;

namespace TestEspressoCore
{
    public class Test1
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


        [Test("3", "TestColl1")]
        static void TestColl1()
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

                string[] ta = { "test" };
                ctx.SetVariableFromAny("ta", ta);
                object result = ctx.Execute("(function(){return JSON.stringify(ta);})()");

                //please parse the string with some json lib
                //(eg Json.net) to check the first elem

                if ((string)result != "[\"" + ta[0] + "\"]") throw new Exception("!");

                stwatch.Stop();
                Console.WriteLine("result " + result);
                Console.WriteLine("met3 managed reflection:" + stwatch.ElapsedMilliseconds.ToString());

            }
        }


        //------------------------------------------
        //data for test4
        public class Base { string _id { get; set; } = "id"; }
        public class A<T> where T : Base
        {
            public void Add(T a) { Console.WriteLine("Add(T a)"); }
            public void Add(Base a) { Console.WriteLine("Add(Base a)"); }
            public void Add(object a) { Console.WriteLine("Add(object a)"); }
        }
        public class B : Base { string data { get; set; } = "data"; }


        //------------------------------------------
        [Test("4", "TestOverload")]
        static void TestOverload()
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

                A<B> a = new A<B>();
                Base base1 = new Base();
                B b = new B();
                //----
                Console.WriteLine("pure .net behaviour");
                //test .net side
                a.Add(null); //Add(T a)
                a.Add(b); //Add(T a)
                a.Add(base1);  //Add(Base a)
                a.Add(new object()); // Add(object a)
                a.Add(new object[0]); // Add(object a)
                //----------
                Console.WriteLine("----------");
                Console.WriteLine("Espresso behaviour");

                ctx.SetVariableFromAny("a", a);
                ctx.SetVariableFromAny("b", b);
                ctx.SetVariableFromAny("base1", base1);


                ctx.Execute("(function(){a.Add(null);})()");//Add(T a)
                ctx.Execute("(function(){a.Add(b);})()");//Add(T a)
                ctx.Execute("(function(){a.Add(base1);})()");//Add(Base a)
                ctx.Execute("(function(){a.Add({});})()");// Add(object a)
                ctx.Execute("(function(){a.Add([]);})()");// Add(object a)

                // if we get to here, we haven't thrown the exception 
                stwatch.Stop();
                Console.WriteLine("met4 managed reflection:" + stwatch.ElapsedMilliseconds.ToString());

            }
        }

        //------------------------------------------
    }
}
