using System.Collections.Generic;
using Fanstatic.Engine.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fanstatic.Engine.Processors
{
    public abstract class Processor : IProcessor
    {
        protected readonly JsonSerializer JsonSerializer;

        protected Processor()
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new JsonInterfaceConverter());
            JsonSerializer = serializer;
        }
        public virtual void ExecuteFirstPass(JToken processorSettings, IGeneratorSettings globalSettings, IList<IProcessor> previousProcessors)
        {
            
        }

        public virtual void ExecuteSecondPass(JToken processorSettings, IGeneratorSettings globalSettings, IList<IProcessor> processors)
        {

        }

        public virtual string ProcessorKey
        {
            get
            {
                var myTypeName = GetType().Name;
                return myTypeName.Substring(0, myTypeName.IndexOf("Processor", System.StringComparison.Ordinal)).ToLower();
            }
        }
    }
}