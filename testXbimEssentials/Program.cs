using System;
using Xbim.Common.Collections;

namespace testXbimEssentials
{
    class Program
    {
        static void Main(string[] args)
        {
            var ifc = new MakeIfc();
            ifc.SaveIfc("test1.ifc");
            Console.WriteLine("Hello World!");
            Environment.Exit(0);
        }
    }
}