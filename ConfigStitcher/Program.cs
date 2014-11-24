using System;
using ConfigStitcher.Stitching;
using ConfigStitcher.Utility;

namespace ConfigStitcher
{
    public class Program
    {
        static void Main(string[] args)
        {
            var arguments = new Arguments(args);

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
    }
}
