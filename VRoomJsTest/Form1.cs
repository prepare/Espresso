using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace VRoomJsTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            var asm = typeof(VroomJs.Tests.TestClass).Assembly;
            var testFixtureAttr = typeof(NUnit.Framework.TestFixtureAttribute);
            var testAttr = typeof(NUnit.Framework.TestAttribute);
            var setupAttr = typeof(NUnit.Framework.SetUpAttribute);
            var tearDownAttr = typeof(NUnit.Framework.TearDownAttribute);
            var testCaseAttr = typeof(NUnit.Framework.TestCaseAttribute);

            List<TestCaseInstance> testCaseList = new List<TestCaseInstance>();

            foreach (var t in asm.GetTypes())
            {
                var founds = t.GetCustomAttributes(testFixtureAttr, false);
                if (founds != null && founds.Length > 0)
                {
                    //test 
                    var testCaseInstance = new TestCaseInstance(t);
                    //find setup/teardown method
                    foreach (var met in t.GetMethods())
                    {
                        var someSetUpAttrs = met.GetCustomAttributes(setupAttr, false);
                        if (someSetUpAttrs != null && someSetUpAttrs.Length > 0)
                        {
                            testCaseInstance.SetSetupMethod(met);
                            continue;
                        }
                        var someTeardownAttrs = met.GetCustomAttributes(tearDownAttr, false);
                        if (someTeardownAttrs != null && someTeardownAttrs.Length > 0)
                        {
                            testCaseInstance.SetTeardownMethod(met);
                            continue;
                        }
                        var someTestAttrs = met.GetCustomAttributes(testAttr, false);
                        if (someTestAttrs != null && someTestAttrs.Length > 0)
                        {

                            var testMethod = testCaseInstance.AddTestMethod(met);
                            this.listBox1.Items.Add(testMethod);
                            continue;
                        }
                        someTestAttrs = met.GetCustomAttributes(testCaseAttr, false);
                        if (someTestAttrs != null && someTestAttrs.Length > 0)
                        {
                            var testMethod = testCaseInstance.AddTestMethod(met);
                            this.listBox1.Items.Add(testMethod);
                        }
                    }

                    testCaseList.Add(testCaseInstance);
                }
            }
            //---------------------------------------------------------------------
            this.listBox1.DoubleClick += new EventHandler(listBox1_DoubleClick);
        }

        void listBox1_DoubleClick(object sender, EventArgs e)
        {
            var testCaseMethod = listBox1.SelectedItem as TestCaseMethod;
            if (testCaseMethod != null)
            {
                //run test
                testCaseMethod.RunTest();
            }
        }
    }
}
