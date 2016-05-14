using System;
using System.IO;
using VroomJs;

namespace VRoomJsConsoleTest
{
    class TestInfo
    {

        public TestInfo()
        {

        }
        public string Name { get; set; }
        public string Choice { get; set; }
        public System.Reflection.MethodInfo TestMethod { get; set; }
    }
}