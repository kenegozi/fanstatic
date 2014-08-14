using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Fanstatic.Engine.Processors
{
    public interface IProcessorSettings
    {
    }

    public interface IGeneratorSettings
    {
        string Title { get; }
        string Root { get; set; }
    }

    public interface IProcessor
    {
        string ProcessorKey { get; }        

        void ExecuteFirstPass(JToken processorSettings, IGeneratorSettings globalSettings, IList<IProcessor> previousProcessors);

        void ExecuteSecondPass(JToken processorSettings, IGeneratorSettings globalSettings, IList<IProcessor> processors);
    }
}