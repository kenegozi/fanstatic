using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Fanstatic.Engine.Processors
{
    public interface IProcessorSettings
    {
    }

    public interface IGeneratorSettings
    {
        string RootDirectory { get; }
    }

    public interface IProcessor
    {
        string ProcessorKey { get; }        

        void ExecuteFirstPass(JObject processorSettings, IGeneratorSettings globalSettings, IList<IProcessor> previousProcessors);

        void ExecuteSecondPass(JObject processorSettings, IGeneratorSettings globalSettings, IList<IProcessor> processors);
    }
}