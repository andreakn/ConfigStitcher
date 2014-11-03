using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConfigStitcher
{
   public class Recipe
   {
      public bool Trace { get; set; }
      public string OutputFilePath { get; set; }
      private List<string> inputFilePaths { get; set; }

      public IEnumerable<string> InputFilePaths { get { return inputFilePaths.AsReadOnly(); } } 
      public Recipe()
      {
         inputFilePaths = new List<string>();
      }

      public void AddInputFile(string fullPath)
      {
         inputFilePaths.Add(fullPath.ToLowerInvariant());
      }
   }
}
