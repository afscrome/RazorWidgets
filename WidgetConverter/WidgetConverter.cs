using NVelocity;
using NVelocity.Context;
using NVelocity.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace WidgetConverter
{
    public class WidgetConverter
    {
        private static RuntimeInstance _runtimeService;

        static WidgetConverter()
        {
            _runtimeService = new RuntimeInstance();
            //Need to register #registerEndOfPageHtml directive to avoid null ref errors
            _runtimeService.AddProperty("userdirective", new ArrayList { typeof(RegisterEndOfPageHtmlDirective).AssemblyQualifiedName });
            _runtimeService.Init();

        }
        private readonly string _outputDir;
        public WidgetConverter(string outputDir)
        {
            _outputDir = outputDir;
        }

        public void ConvertWidget(XElement widget)
        {
            var id = widget.Attribute("instanceIdentifier").Value;

            Console.WriteLine(id);
            var directoryPath = Path.Combine(_outputDir, id); ;
            Directory.CreateDirectory(directoryPath);
            var config = ConvertWidgetConfig(widget);
            config.Save(Path.Combine(directoryPath, "_about.config"));

            var specialScriptFiles = new Dictionary<string, XElement>{
                {"_widget", widget.Element("contentScript")},
                {"_header", widget.Element("headerScript")},
                {"_css", widget.Element("additionalCssScript")},
            };

            foreach (var specialScripts in specialScriptFiles)
            {
                if (specialScripts.Value != null)
                    OutputWidgetRazorPartial(directoryPath, specialScripts.Value.Value, specialScripts.Key);
            }

            foreach (var file in widget.Descendants("file"))
            {
                var fileName = file.Attribute("name").Value;

                using (var stream = new MemoryStream(Convert.FromBase64String(file.Value)))
                {
                    if (!fileName.EndsWith(".vm", StringComparison.OrdinalIgnoreCase))
                    {
                        using (var writeStream = File.OpenWrite(Path.Combine(directoryPath, fileName)))
                        {
                            stream.CopyTo(writeStream);
                        }
                    }
                    else
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var fileScript = reader.ReadToEnd();
                            fileName = fileName.Substring(0, fileName.Length - 3);
                            OutputWidgetRazorPartial(directoryPath, fileScript, fileName);
                        }
                    }
                }
            }
        }

        public XDocument ConvertWidgetConfig(XElement widget)
        {
            var root = new XElement("razorWidget");
            root.ReplaceAttributes(widget.Attributes());
            //Todo: unescape config xml

            var config = widget.Element("configuration");
            if (config != null)
            {
                var newConfig = String.Concat("<configuration>", config.Value, "</configuration>");
                root.Add(XElement.Parse(newConfig));
            }
            root.Add(widget.Element("languageResources"));
            root.Add(widget.Element("requiredContext"));
            return new XDocument(root);
        }

        public void OutputWidgetRazorPartial(string directory, string velocity, string fileName)
        {
            var razor = VelocityToRazor(velocity);
            //Normalise line endings to stop visual studio complaining
            razor = razor.Replace("\r\n", "\n").Replace("\n", "\r\n");

            File.WriteAllText(Path.Combine(directory, fileName + ".cshtml"), razor);
        }

        public string VelocityToRazor(string velocityScript)
        {
            using (var reader = new StringReader(velocityScript))
            {
                //var charStream = new VelocityCharStream(reader, 0, 0);
                var parser = _runtimeService.CreateNewParser();

                var syntaxTree = parser.Parse(reader, "todo");
                var context = new InternalContextAdapterImpl(new VelocityContext());
                syntaxTree.Init(context, _runtimeService);
                var visitor = new VelocityRazorConverterVisitor(_runtimeService, context);
                syntaxTree.Accept(visitor, null);
                return visitor.RazorCode();
            }
        }
    }
}
