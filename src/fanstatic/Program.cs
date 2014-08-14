using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Castle.Components.DictionaryAdapter;
using Fanstatic.Engine.Configuration;
using Fanstatic.Engine.Processors;
using Fanstatic.Engine.Processors.Posts;
using Fanstatic.Engine.Processors.Tags;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace fanstatic
{
    class Program
    {
        static void Main(string[] args)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JsonInterfaceConverter());

            var root = @"C:\w\oss\fanstatic\src\fanstatic";
            var configFilePath = Path.Combine(root, "site.config.json");
            var configFile = File.ReadAllText(configFilePath);
            var configObject = JObject.Parse(configFile);
            var config = configObject.ToObject<IGeneratorSettings>(serializer);
            config.Root = root;
            var processorsNode = (JObject) configObject["processors"];

            var procTypes = (from a in AppDomain.CurrentDomain.GetAssemblies()
                             where !a.IsDynamic
                from t in a.GetExportedTypes()
                where !t.IsInterface && !t.IsAbstract && typeof (IProcessor).IsAssignableFrom(t)
                select t).ToArray();

            var procs = new Dictionary<IProcessor,JToken>();

            foreach (var prop in processorsNode.Properties())
            {
                var processorName = prop.Name;
                var propConfigObject = prop.Value;
                var procType =
                    procTypes.SingleOrDefault(
                        t =>
                            t.Name.ToLowerInvariant()
                                .Replace("processor", "")
                                .Equals(processorName, StringComparison.InvariantCultureIgnoreCase));
                if (procType == null)
                {
                    throw new InvalidOperationException("Unknown processor " + processorName);
                }
                procs.Add((IProcessor)Activator.CreateInstance(procType), propConfigObject);
            }
            foreach (var template in Directory.EnumerateFiles(root, "*.cshtml", SearchOption.AllDirectories))
            {
                var templateId = template.Substring(root.Length).Replace('\\', '/').Trim('/');
                if (templateId.EndsWith(".cshtml"))
                {
                    templateId = templateId.Substring(0, templateId.Length - ".cshtml".Length);
                }
                RazorEngine.Razor.Compile(File.ReadAllText(template), templateId);
            }

            foreach (var processor in procs)
            {
                processor.Key.ExecuteFirstPass(processor.Value, config, procs.Keys.ToList());
            }

            foreach (var processor in procs)
            {
                processor.Key.ExecuteSecondPass(processor.Value, config, procs.Keys.ToList());
            }
        }
    }
}
