using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using Castle.Components.DictionaryAdapter;
using Castle.DynamicProxy;
using Fanstatic.Engine.Configuration;
using Fanstatic.Engine.Processors;
using Fanstatic.Engine.Processors.Posts;
using Fanstatic.Engine.Processors.Tags;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = System.Xml.Formatting;

namespace fanstatic
{
    class Program
    {
        static void Main(string[] args)
        {
            var root = @"C:\w\oss\fanstatic\src\fanstatic";

            foreach (var template in Directory.EnumerateFiles(root, "*.cshtml", SearchOption.AllDirectories))
            {
                var templateId = template.Substring(root.Length).Replace('\\', '/').Trim('/');
                if (templateId.EndsWith(".cshtml"))
                {
                    templateId = templateId.Substring(0, templateId.Length - ".cshtml".Length);
                }
                RazorEngine.Razor.Compile(File.ReadAllText(template), templateId);
            }

            var procs = new IProcessor[]
            {
                new PostsProcessor(),
                new TagsProcessor(),
            };

            var i = 0;
            var globalSettings = new DictionaryAdapterFactory().GetAdapter<IGeneratorSettings>(new Hashtable
            {
                {
                    "RootDirectory", @"C:\w\oss\fanstatic\src\fanstatic"
                }
            });
            foreach (var processor in procs)
            {
                processor.ExecuteFirstPass(JObject.Parse("{}") , globalSettings,procs.Take(i++).ToArray());
            }

            foreach (var processor in procs)
            {
                processor.ExecuteSecondPass(JObject.Parse("{}"), globalSettings, procs);
            }

        }
    }
}
