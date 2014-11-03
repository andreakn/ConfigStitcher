using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CommandLine.Utility;

namespace ConfigStitcher
{
   public class Program
   {
      static void Main(string[] args)
      {
         var arguments = new Arguments(args);
         //Console.WriteLine("currentdir " + Environment.CurrentDirectory);
         //Console.WriteLine("AppDomain.CurrentDomain.BaseDirectory " + );
         //Console.WriteLine("Assembly.GetEntryAssembly().Location " + Assembly.GetEntryAssembly().Location);
         




         var workingDir = AppDomain.CurrentDomain.BaseDirectory;
         var recipeLocation = "recipe.txt";
         var enableTrace = false;

         if (arguments["workingdir"] != null)
            workingDir = arguments["workingdir"];

         if (arguments["recipe"] != null)
            recipeLocation = arguments["recipe"];

         if (arguments["trace"] != null)
            enableTrace = arguments["trace"]=="true";


         var stitcher = new Stitcher();
         stitcher.PerformConfigStitching(workingDir,recipeLocation, enableTrace);

      }
   }
}
