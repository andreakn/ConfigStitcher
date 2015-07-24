using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConfigStitcher.Stitching
{
    public class RecipeCreator
    {
        public string WorkingDir { get; set; }
        public string InputFolder { get; set; }
        public string OutputFolder { get; set; }
        public bool Recurse { get; set; }
        public bool Trace { get; set; }

        private List<string> _inputFilenames;
        private string _outputFileName = null;

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

            if (_outputFileName == null && line.Contains("="))
            {
                _outputFileName = line.Split('=')[0].Trim();
                line = line.Split('=')[1].Trim();
            }

            if (line.Contains("[")) _inputFilenames = new List<string>();
            line = line.Replace("[", "");

            var produceRecipe = line.Contains("]");
            line = line.Replace("]", "");
            line = line.Trim();
            if (!string.IsNullOrEmpty(line))
            {
                var inputFilenamesInLine = line.Split(',').Select(s => s.Trim());
                foreach (var s in inputFilenamesInLine)
                {
                    if (!string.IsNullOrWhiteSpace(s))
                        _inputFilenames.Add(s);
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

            if (Path.IsPathRooted(_outputFileName))
                ret.OutputFilePath = _outputFileName;
            else if (Path.IsPathRooted(OutputFolder))
                ret.OutputFilePath = Path.Combine(OutputFolder, _outputFileName);
            else
                ret.OutputFilePath = Path.Combine(WorkingDir, OutputFolder, _outputFileName);


            var inputRootDir = GetDirectory(InputFolder);
            var searchOption = Recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var inputFilename in _inputFilenames)
            {
                if (Path.IsPathRooted(inputFilename))
                {
                    if (File.Exists(inputFilename))
                    {
                        ret.AddInputFile(inputFilename);
                    }
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

            if (!ret.InputFilePaths.Any())
            {
                throw new Exception("Cannot find ANY inputs according to recipe " + (Recurse?"in subfolders of ":"in folder ") +inputRootDir+". Was looking for the following: "+string.Join(", ",_inputFilenames));
            }

            _outputFileName = null; //ensure crash rather than reuse if invalid syntax
            _inputFilenames = null; //ensure crash rather than reuse if invalid syntax
            return ret;
        }


        private DirectoryInfo GetDirectory(string folderPath)
        {
            var ret = new DirectoryInfo(folderPath);
            if (!Path.IsPathRooted(folderPath))
            {
                ret = new DirectoryInfo(Path.Combine(WorkingDir, folderPath));
            }
            return ret;
        }
    }
}
