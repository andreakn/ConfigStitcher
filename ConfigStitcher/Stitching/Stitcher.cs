using System;
using System.Runtime.InteropServices;

namespace ConfigStitcher
{
   public class Stitcher
   {
      private FileWorker fileWorker = new FileWorker();
      private XmlStitcher xmlStitcher = new XmlStitcher();


      public void PerformConfigStitching(string workingDir, string recipeLocation, bool enableTrace)
      {
         var recipes = fileWorker.FetchRecipes(workingDir, recipeLocation, enableTrace);
         foreach (var recipe in recipes)
         {
            var input = fileWorker.FetchInput(recipe);
            var result = xmlStitcher.MergeConfigFiles(input, recipe.Trace);
            fileWorker.WriteOutput(result);
            Console.WriteLine("Writing file [{0}] based on inputs-->{1}",result.Recipe.OutputFilePath,string.Join(", ",result.Recipe.InputFilePaths));
         }

      }
   }
}