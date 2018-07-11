﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Skrypt.Engine;

namespace Skrypt
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Get skrypt test code
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"..\..\SkryptFiles\testcode.sk");
            var code = File.ReadAllText(path);

            // Creating a skrypt engine object
            var engine = new SkryptEngine();

            engine.AddClass(typeof(Vector));
            engine.Parse(code);

            var stopwatch = Stopwatch.StartNew();

            var b = 0;
            var a = "";

            while (b < 100000)
            {
                b++;
                a = a + "kek";
            }

            stopwatch.Stop();
            double execute = stopwatch.ElapsedMilliseconds;

            Console.WriteLine("Test: {0}ms", execute);

            Console.ReadKey();
        }
    }
}