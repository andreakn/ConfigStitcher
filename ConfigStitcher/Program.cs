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
      public static void Main(string[] args)
      {
         var arguments = new Arguments(args);
         //Console.WriteLine("currentdir " + Environment.CurrentDirectory);
         //Console.WriteLine("AppDomain.CurrentDomain.BaseDirectory " + );
         //Console.WriteLine("Assembly.GetEntryAssembly().Location " + Assembly.GetEntryAssembly().Location);

         if (arguments["help"] == "true"|| arguments["?"] == "true")
         {
            PrintHelp();
            return;
         }




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

      public static void PrintHelp()
      {
         Console.WriteLine(@"Usage:
ConfigStitcher [-workingdir ""<path>""] [-recipe ""<path>""] [-trace <true|false>] 

Arguments:
- recipe: relative or absolute path to recipe file
- workingdir: root path for resolving the recipe if relative path is used for recipe
- trace: whether to write diagnostic info into the output file(s)  (NB: this makes the files unusable by .net, but can be handy to verify whence a setting came from)

Defaults: 
- recipe: ""recipe.txt""
- workingdir: directory path of ConfigStitcher.exe 
- trace: false

");

      }
   }
}
