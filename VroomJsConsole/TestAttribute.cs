using System;
using System.IO;
using VroomJs;

namespace VRoomJsConsoleTest
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