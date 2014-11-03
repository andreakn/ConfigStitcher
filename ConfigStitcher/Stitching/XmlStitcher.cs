﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ConfigStitcher.Stitching;

namespace ConfigStitcher
{
   public class XmlStitcher
   {
      public RecipeResult MergeConfigFiles(RecipeInputs inputs, bool enableTrace)
      {
         var ret = new RecipeResult {Recipe = inputs.Recipe};
         var merger = new XmlConfigMerge(enableTrace, "id", "name", "key", "path", "virtualPath", "href", "namespace");
         ret.Result = merger.MergeConfigs(inputs.OrderedInputs());
         return ret;
      }

      public class XmlConfigMerge
      {
         public string DeleteKeyword = "DELETEME";
         private bool EnableTrace = false;
         public XmlConfigMerge(bool enableTrace, params string[] unique)
         {
            UniqueAttributes = unique.ToList();
            EnableTrace = enableTrace;
         }

         private List<string> _uniqueAttributes;
         public List<string> UniqueAttributes
         {
            get { return _uniqueAttributes; }
            set { _uniqueAttributes = value; }
         }

         private XElement FindByAttr(IEnumerable<XElement> elements, string attribute, string value)
         {
            return elements.FirstOrDefault(e => HasAttribute(e, attribute, value));
         }

         private bool HasAttribute(XElement elem, string attributeName)
         {
            var attr = elem.Attribute(attributeName);
            if (attr != null
                && !string.IsNullOrEmpty(attr.Value)
                )
            {
               return true;
            }
            return false;
         }

         private bool HasAttribute(XElement elem, string attributeName, string attributeValue)
         {
            if (HasAttribute(elem, attributeName))
            {
               if (0 ==
                   string.Compare(elem.Attribute(attributeName).Value, attributeValue,
                                  StringComparison.InvariantCultureIgnoreCase))
               {
                  return true;
               }
            }
            return false;
         }

         public XDocument MergeConfigs(IEnumerable<RecipeInput> inputs)
         {
            if (inputs == null || !inputs.Any()) throw new Exception("No input files to transform");
            var template = XDocument.Parse("<configuration></configuration>");
            foreach (var data in inputs)
            {
               template = MergeConfigs(template, data);
            }
            return template;
         }

         private XDocument MergeConfigs(XDocument template, RecipeInput data)
         {
            var rootNodeTemplate = template.Root;
            var rootNodeData = data.Xml.Root;
            MergeConfigs(rootNodeTemplate, rootNodeData, data.Filepath);
            return template;
         }

         private void MergeConfigs(XElement template, XElement data, string filePath)
         {
            if (template == null) { return; }
            bool shouldDeleteNode = ShouldDeleteNode(data);
            if (shouldDeleteNode)
            {
               template.Remove();  //cannot trace this for now
               return;
            }

            MergeAttributes(template, data,filePath);

            if (!data.HasElements)
            {
               if (data.Value == DeleteKeyword) template.Value = string.Empty;
               else if (!string.IsNullOrEmpty(data.Value)) template.Value = data.Value;
               return;
            }

            var templateElements = template.Elements();
            var dataElements = data.Elements();
            foreach (var dataNode in dataElements)
            {
               KeyValue uniquey = GetUniqueKey(dataNode);
               bool shouldDeleteDataNode = ShouldDeleteNode(dataNode);
               var matchingNodes = templateElements.Where(x => x.Name == dataNode.Name);
               if (matchingNodes.Count() == 0 && !shouldDeleteDataNode)
               {
                  dataNode.SetAttributeValue("TRACE", TraceText(filePath, "CREATED"));
                  template.Add(dataNode);
                  continue;
               }

               if (uniquey == null && matchingNodes.Count() > 1)
               {
                  throw new ApplicationException("Cannot merge into file containing multiple undelimited equal siblings, needs attribute with name: " + string.Join(", ", UniqueAttributes.ToArray()) + "\nThe offender is: " + data.ToString());
               }
               if (uniquey == null && matchingNodes.Count() == 1)
               {
                  var templateNode = templateElements.Where(x => x.Name == dataNode.Name).First();
                  MergeConfigs(templateNode, dataNode,filePath);
                  continue;
               }

               if (uniquey != null)
               {
                  var ambigous =
                      templateElements.Count(
                          x =>
                          x.Attribute(uniquey.Key) != null &&
                          x.Attribute(uniquey.Key).Value.ToLower() == uniquey.Value.ToLower()) > 1;
                  if (ambigous)
                  {
                     throw new ApplicationException("Cannot merge into file containing multiple equal siblings with same identifier, needs unique value for attribute with name: " + string.Join(", ", UniqueAttributes.ToArray()) + "\nThe offender is: " + data.ToString());
                  }
                  var templateNode = (from x in templateElements
                                      where
                                          x.Attribute(uniquey.Key) != null
                                          && x.Attribute(uniquey.Key).Value.ToLower() == uniquey.Value.ToLower()
                                      select x).FirstOrDefault();
                  if (templateNode == null && !shouldDeleteDataNode)
                  {
                     if (EnableTrace)
                     {
                        dataNode.SetAttributeValue("TRACE", TraceText(filePath,"CREATED"));
                     }
                     template.Add(dataNode);
                     continue;
                  }
                  else
                  {
                     MergeConfigs(templateNode, dataNode,filePath);
                     continue;
                  }
               }

            }
            return;

         }

         private void MergeAttributes(XElement template, XElement data,string filePath)
         {
            foreach (var attrib in data.Attributes())
            {
               var value = attrib.Value;
               if (value == DeleteKeyword)
               {
                  value = null;
               }
               else if(!UniqueAttributes.Contains(attrib.Name.LocalName))
               {
                  value = value + TraceText(filePath,"CHANGED");
               }
               template.SetAttributeValue(attrib.Name, value);
            }
         }

         private string TraceText(string filePath, string operation)
         {
            if (EnableTrace)
               return string.Format("[{0} BY {1}]",operation, filePath);
            return null;
         }

         private KeyValue GetUniqueKey(XElement element)
         {
            foreach (var key in UniqueAttributes)
            {
               var elementsUnique = element.Attributes(key);
               if (elementsUnique.Count() == 1)
               {
                  return new KeyValue { Key = key, Value = elementsUnique.First().Value };
               }
            }
            return null;
         }

         private bool ShouldDeleteNode(XElement data)
         {
            var deleteattrib = data.Attributes(DeleteKeyword);
            if (deleteattrib.Count() == 1 && bool.Parse(deleteattrib.First().Value))
            {
               return true;
            }
            return false;
         }
      }

      public class KeyValue
      {
         public string Key;
         public string Value;
      }

      // CodegenManager class records the various blocks so it can split them up
      //class CodegenManager
      //{
      //   private class Block
      //   {
      //      public String Name;
      //      public int Start, Length;
      //   }

      //   private Block currentBlock;
      //   private List<Block> files = new List<Block>();
      //   private Block footer = new Block();
      //   private Block header = new Block();
      //   private StringBuilder template;
      //   protected List<String> generatedFileNames = new List<String>();

      //   public static CodegenManager Create(ITextTemplatingEngineHost host, StringBuilder template)
      //   {
      //      return (host is IServiceProvider) ? new VSCodegenManager(host, template) : new CodegenManager(host, template);
      //   }

      //   public void StartNewFile(String name)
      //   {
      //      if (name == null)
      //         throw new ArgumentNullException("name");
      //      CurrentBlock = new Block { Name = name };
      //   }

      //   public void StartFooter()
      //   {
      //      CurrentBlock = footer;
      //   }

      //   public void StartHeader()
      //   {
      //      CurrentBlock = header;
      //   }

      //   public void EndBlock()
      //   {
      //      if (CurrentBlock == null)
      //         return;
      //      CurrentBlock.Length = template.Length - CurrentBlock.Start;
      //      if (CurrentBlock != header && CurrentBlock != footer)
      //         files.Add(CurrentBlock);
      //      currentBlock = null;
      //   }

      //   public virtual void Process(bool split)
      //   {
      //      if (split)
      //      {
      //         EndBlock();
      //         String headerText = template.ToString(header.Start, header.Length);
      //         String footerText = template.ToString(footer.Start, footer.Length);
      //         String outputPath = Path.GetDirectoryName(host.TemplateFile);
      //         files.Reverse();
      //         foreach (Block block in files)
      //         {
      //            String fileName = Path.Combine(outputPath, block.Name);
      //            String content = headerText + template.ToString(block.Start, block.Length) + footerText;
      //            generatedFileNames.Add(fileName);
      //            CreateFile(fileName, content);
      //            template.Remove(block.Start, block.Length);
      //         }
      //      }
      //   }

      //   protected virtual void CreateFile(String fileName, String content)
      //   {
      //      if (IsFileContentDifferent(fileName, content))
      //         File.WriteAllText(fileName, content);
      //   }

      //   public virtual String GetCustomToolNamespace(String fileName)
      //   {
      //      return null;
      //   }

      //   public virtual String DefaultProjectNamespace
      //   {
      //      get { return null; }
      //   }

      //   protected bool IsFileContentDifferent(String fileName, String newContent)
      //   {
      //      return !(File.Exists(fileName) && File.ReadAllText(fileName) == newContent);
      //   }

      //   private CodegenManager(ITextTemplatingEngineHost host, StringBuilder template)
      //   {
      //      this.host = host;
      //      this.template = template;
      //   }

      //   private Block CurrentBlock
      //   {
      //      get { return currentBlock; }
      //      set
      //      {
      //         if (CurrentBlock != null)
      //            EndBlock();
      //         if (value != null)
      //            value.Start = template.Length;
      //         currentBlock = value;
      //      }
      //   }

      //   private class VSCodegenManager : CodegenManager
      //   {
      //      private EnvDTE.ProjectItem templateProjectItem;
      //      private EnvDTE.DTE dte;
      //      private Action<String> checkOutAction;
      //      private Action<IEnumerable<String>> projectSyncAction;

      //      public override String DefaultProjectNamespace
      //      {
      //         get
      //         {
      //            return templateProjectItem.ContainingProject.Properties.Item("DefaultNamespace").Value.ToString();
      //         }
      //      }

      //      public override String GetCustomToolNamespace(string fileName)
      //      {
      //         return dte.Solution.FindProjectItem(fileName).Properties.Item("CustomToolNamespace").Value.ToString();
      //      }

      //      public override void Process(bool split)
      //      {
      //         if (templateProjectItem.ProjectItems == null)
      //            return;
      //         base.Process(split);
      //         projectSyncAction.EndInvoke(projectSyncAction.BeginInvoke(generatedFileNames, null, null));
      //      }

      //      protected override void CreateFile(String fileName, String content)
      //      {
      //         if (IsFileContentDifferent(fileName, content))
      //         {
      //            CheckoutFileIfRequired(fileName);
      //            File.WriteAllText(fileName, content);
      //         }
      //      }

      //      internal VSCodegenManager(ITextTemplatingEngineHost host, StringBuilder template)
      //         : base(host, template)
      //      {
      //         var hostServiceProvider = (IServiceProvider)host;
      //         if (hostServiceProvider == null)
      //            throw new ArgumentNullException("Could not obtain IServiceProvider");
      //         dte = (EnvDTE.DTE)hostServiceProvider.GetService(typeof(EnvDTE.DTE));
      //         if (dte == null)
      //            throw new ArgumentNullException("Could not obtain DTE from host");
      //         templateProjectItem = dte.Solution.FindProjectItem(host.TemplateFile);
      //         checkOutAction = (String fileName) => dte.SourceControl.CheckOutItem(fileName);
      //         projectSyncAction = (IEnumerable<String> keepFileNames) => ProjectSync(templateProjectItem, keepFileNames);
      //      }

      //      private static void ProjectSync(EnvDTE.ProjectItem templateProjectItem, IEnumerable<String> keepFileNames)
      //      {
      //         var keepFileNameSet = new HashSet<String>(keepFileNames);
      //         var projectFiles = new Dictionary<String, EnvDTE.ProjectItem>();
      //         var originalFilePrefix = Path.GetFileNameWithoutExtension(templateProjectItem.get_FileNames(0)) + ".";
      //         foreach (EnvDTE.ProjectItem projectItem in templateProjectItem.ProjectItems)
      //            projectFiles[projectItem.get_FileNames(0)] = projectItem;

      //         // Remove unused items from the project
      //         foreach (var pair in projectFiles)
      //            if (!keepFileNames.Contains(pair.Key) && !(Path.GetFileNameWithoutExtension(pair.Key) + ".").StartsWith(originalFilePrefix))
      //               //pair.Value.Delete();

      //               // Add missing files to the project
      //               foreach (String fileName in keepFileNameSet)
      //                  if (!projectFiles.ContainsKey(fileName))
      //                     templateProjectItem.ProjectItems.AddFromFile(fileName);
      //      }

      //      private void CheckoutFileIfRequired(String fileName)
      //      {
      //         var sc = dte.SourceControl;
      //         if (sc != null && sc.IsItemUnderSCC(fileName) && !sc.IsItemCheckedOut(fileName))
      //            checkOutAction.EndInvoke(checkOutAction.BeginInvoke(fileName, null, null));
      //      }
      //   }
      //}




   }
}