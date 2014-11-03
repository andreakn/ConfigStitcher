using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace ConfigStitcher.Stitching
{
   public class RecipeInput
   {
      public Recipe Recipe { get; set; }
      public string Filepath { get; set; }
      public XDocument Xml { get; set; }
   }
}
