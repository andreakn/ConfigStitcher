using System.Xml.Linq;

namespace ConfigStitcher.Stitching
{
    public class RecipeResult
    {
        public Recipe Recipe { get; set; }
        public XDocument Result { get; set; }
    }
}