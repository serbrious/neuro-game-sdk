using System;

namespace NeuroSdk.Json
{
    [Obsolete("", true)]
    public interface ISchemaModifier
    {
        void Apply(JsonSchema schema);
    }
}
