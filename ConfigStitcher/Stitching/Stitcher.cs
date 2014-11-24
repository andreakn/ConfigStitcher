using System;

namespace ConfigStitcher.Stitching
{
    public class Stitcher
    {
        private readonly FileWorker _fileWorker = new FileWorker();
        private readonly XmlStitcher _xmlStitcher = new XmlStitcher();

        public void PerformConfigStitching(string workingDir, string recipeLocation, bool enableTrace)
        {
            var recipes = _fileWorker.FetchRecipes(workingDir, recipeLocation, enableTrace);
            foreach (var recipe in recipes)
            {
                var input = _fileWorker.FetchInput(recipe);
                var result = _xmlStitcher.MergeConfigFiles(input, recipe.Trace);
                _fileWorker.WriteOutput(result);
                Console.WriteLine("Writing file [{0}] based on inputs-->{1}", result.Recipe.OutputFilePath, string.Join(", ", result.Recipe.InputFilePaths));
            }
        }
    }
}