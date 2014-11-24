using System.Collections.Generic;

namespace ConfigStitcher.Stitching
{
    public class Recipe
    {
        public bool Trace { get; set; }
        public string OutputFilePath { get; set; }
        private readonly List<string> _inputFilePaths;

        public IEnumerable<string> InputFilePaths { get { return _inputFilePaths.AsReadOnly(); } }
        
        public Recipe()
        {
            _inputFilePaths = new List<string>();
        }

        public void AddInputFile(string fullPath)
        {
            _inputFilePaths.Add(fullPath.ToLowerInvariant());
        }
    }
}
