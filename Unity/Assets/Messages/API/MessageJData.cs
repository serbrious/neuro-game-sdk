#nullable enable

using Newtonsoft.Json.Linq;

namespace NeuroSdk.Messages.API
{
    public readonly struct MessageJData
    {
        public readonly JToken? Data;

        public MessageJData(JToken? data)
        {
            Data = data;
        }
    };
}
