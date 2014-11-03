using System.Xml.Linq;

namespace ConfigStitcher
{
   public class RecipeResult
   {
      public Recipe Recipe { get; set; }
      public XDocument Result { get; set; }

   }
}