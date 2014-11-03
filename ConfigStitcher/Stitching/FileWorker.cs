using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ConfigStitcher.Stitching;

namespace ConfigStitcher
{
   public class FileWorker
   {


      public RecipeInputs FetchInput(Recipe recipe)
      {
         var ret = new RecipeInputs {Recipe = recipe};
         ret.Inputs = new Dictionary<string, XDocument>();
         foreach (var inputFile in recipe.InputFilePaths)
         {
            if (File.Exists(inputFile))
            {
               var xmlstring = File.ReadAllText(inputFile);
               var xml = XDocument.Parse(xmlstring);
               ret.Inputs[inputFile] = xml;
            }
         }
         return ret;
      }

      public void WriteOutput(RecipeResult result)
      {
         var filePath = result.Recipe.OutputFilePath;
         
         var fileinfo = new FileInfo(filePath);
         var directoryInfo = fileinfo.Directory;
         EnsureDirectory(directoryInfo);

         if (File.Exists(filePath))
         {
            File.Delete(filePath);
         }
         File.WriteAllText(filePath,result.Result.ToString());
      }

      public void EnsureDirectory(DirectoryInfo directoryInfo)
      {
         if (directoryInfo.Parent != null)
            EnsureDirectory(directoryInfo.Parent);
         if (!directoryInfo.Exists)
         {
            directoryInfo.Create();
         }
      }

      public IEnumerable<Recipe> FetchRecipes(RecipeCreator outerCreator, string recipeLocation)
      {
         var ret = new List<Recipe>();
         var recipeFile = new FileInfo(recipeLocation);
         if (!Path.IsPathRooted(recipeLocation))
         {
            recipeFile = new FileInfo(Path.Combine(outerCreator.WorkingDir, recipeLocation));
         }
         if (!recipeFile.Exists)
            throw new StitcherException("Can't find recipefile at location " + recipeLocation);


         var creator = new RecipeCreator { 
            Trace = outerCreator.Trace,
            WorkingDir = recipeFile.Directory.FullName, 
            OutputFolder = outerCreator.OutputFolder, 
            InputFolder = outerCreator.InputFolder};

         var lines = File.ReadAllLines(recipeFile.FullName).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

         foreach (var line in lines)
         {
            if (line.StartsWith("recipe", StringComparison.InvariantCultureIgnoreCase))
            {
               try
               {
                  ret.AddRange(FetchRecipes(creator, line.Split('=')[1].Trim())); //nested recipe file definitions
               }
               catch (StitcherException ex)
               {
                  Console.WriteLine("warning: -->"+ex.Message);
               }
            }
            else
            {
               ret.Add(creator.HandleRecipeLine(line));
            }
         }

         return ret.Where(recipe => recipe != null);
      }

      public IEnumerable<Recipe> FetchRecipes(string workingDir, string recipeLocation, bool trace)
      {
         var outerCreator = new RecipeCreator {WorkingDir = workingDir,Trace = trace};
        return FetchRecipes(outerCreator, recipeLocation);
      }
   }
}