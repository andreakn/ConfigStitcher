using System;
using ConfigStitcher.Stitching;
using ConfigStitcher.Utility;

namespace ConfigStitcher
{

    public class Program
    {
        private static void Main(string[] args)
        {
            var arguments = new Arguments(args);


            if (arguments["help"] == "true" || arguments["?"] == "true")
            {
                PrintHelp();
                return;
            }

            var workingDir = GetWorkingDir(arguments);
            var recipeLocation = arguments["recipe"] ?? "recipe.txt";
            var enableTrace = arguments["trace"] == "true";

            var stitcher = new Stitcher();
            stitcher.PerformConfigStitching(workingDir, recipeLocation, enableTrace);
        }

        private static string GetWorkingDir(Arguments arguments)
        {
            if (arguments["workingdir"] != null)
            {
                return arguments["workingdir"];
            }
            return arguments["relativeToExe"] == "true"
                ? AppDomain.CurrentDomain.BaseDirectory
                : Environment.CurrentDirectory;
        }

        public static void PrintHelp()
        {
            Console.WriteLine(@"Usage:
ConfigStitcher [-workingdir ""<absolute path>""] [-relativeToExe <true|false>] [-recipe ""<path>""] [-trace <true|false>] 

Arguments:
- recipe: relative or absolute path to recipe file
- workingdir: root path for resolving the recipe if relative path is used for recipe
- relativeToExe: if set to true this sets working dir to directory in which exe is located (has no effect if working dir is set explicitly)
- trace: whether to write diagnostic info into the output file(s)  (NB: this makes the files unusable by .net, but can be handy to verify whence a setting came from)

Defaults: 
- recipe: ""recipe.txt""
- workingdir: Current directory ( $PWD )
- relativeToExe: false  
- trace: false
");
        }

    }
}
