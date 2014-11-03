using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ConfigStitcher.Stitching;

namespace ConfigStitcher
{
   public class RecipeInputs
   {
      public Recipe Recipe { get; set; }
      public Dictionary<string,XDocument> Inputs { get; set; }


      public IEnumerable<RecipeInput> OrderedInputs()
      {
         return Recipe.InputFilePaths.Select(filepath => new RecipeInput
         {
            Filepath = filepath,
            Xml = Inputs[filepath],
            Recipe = Recipe
         });
      }
   }
}