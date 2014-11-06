using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConfigStitcher.Stitching
{
   public class RecipeCreator
   {
      public string WorkingDir { get; set; }
      public string InputFolder { get; set; }
      public string OutputFolder { get; set; }
      public bool Recurse { get; set; }
      public bool Trace { get; set; }

      private List<string> inputFilenames;
      private string outputFileName = null;

      public Recipe HandleRecipeLine(string line)
      {
         if (line.StartsWith("inputfolder", StringComparison.InvariantCultureIgnoreCase))
         {
            InputFolder = line.Split('=')[1].Trim();
            return null;
         }
         else if (line.StartsWith("outputfolder", StringComparison.InvariantCultureIgnoreCase))
         {
            OutputFolder = line.Split('=')[1].Trim();
            return null;
         }
         else if (line.StartsWith("recurse", StringComparison.InvariantCultureIgnoreCase))
         {
            Recurse = bool.Parse(line.Split('=')[1].Trim());
            return null;
         }
         else if (line.StartsWith("trace", StringComparison.InvariantCultureIgnoreCase))
         {
            Trace = bool.Parse(line.Split('=')[1].Trim());
            return null;
         }
         else if (line.StartsWith("#") || line.StartsWith("//") || line.StartsWith("--")) //comment
         {
            return null;
         }

         if (outputFileName==null&&line.Contains("="))
         {
            outputFileName = line.Split('=')[0].Trim();
            line = line.Split('=')[1].Trim();
         }

         if(line.Contains("["))inputFilenames = new List<string>();
         line = line.Replace("[", "");

         var produceRecipe = line.Contains("]");
         line = line.Replace("]", "");
         line = line.Trim();
         if (!string.IsNullOrEmpty(line))
         {
            var inputFilenamesInLine = line.Split(',').Select(s=>s.Trim());
            foreach (var s in inputFilenamesInLine)
            {
               if(!string.IsNullOrWhiteSpace(s))
                  inputFilenames.Add(s);
            }            
         }
         if (produceRecipe)
            return ProduceRecipe();

         return null;
      }

      private Recipe ProduceRecipe()
      {
         var ret = new Recipe();

         ret.Trace = Trace;

         if (Path.IsPathRooted(outputFileName))
            ret.OutputFilePath = outputFileName;
         else if (Path.IsPathRooted(OutputFolder))
            ret.OutputFilePath = Path.Combine(OutputFolder, outputFileName);
         else
            ret.OutputFilePath = Path.Combine(WorkingDir, OutputFolder, outputFileName);


         var inputRootDir = GetDirectory(InputFolder);
         var searchOption = Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
         foreach (var inputFilename in inputFilenames)
         {
            if (Path.IsPathRooted(inputFilename))
            {
               ret.AddInputFile(inputFilename);
            }
            else
            {
               var fileInfos = inputRootDir.GetFiles(inputFilename, searchOption).ToList();
               if (fileInfos.Any())
               {
                  ret.AddInputFile(fileInfos.First().FullName);
               }
            }
         }


         outputFileName = null; //ensure crash rather than reuse if invalid syntax
         inputFilenames = null; //ensure crash rather than reuse if invalid syntax
         return ret;
      }


      private DirectoryInfo GetDirectory(string folderPath)
      {
         var ret = new DirectoryInfo(folderPath);
         if (!Path.IsPathRooted(folderPath))
         {
            ret = new DirectoryInfo(Path.Combine(WorkingDir,folderPath));
         }
         return ret;
      }
   }
}
