//MIT, 2015-2017, WinterDev, EngineKit, brezza92
using System;
namespace TestEspressoCore
{
    class TestAttribute : Attribute
    {
        public TestAttribute() { }
        public TestAttribute(string choice, string name)
        {
            this.Choice = choice;
            this.Name = name;
        }
        public string Choice { get; set; }
        public string Name { get; set; }
    }
}