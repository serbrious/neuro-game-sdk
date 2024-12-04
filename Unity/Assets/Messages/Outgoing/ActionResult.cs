#nullable enable

using JetBrains.Annotations;
using NeuroSdk.Messages.API;
using NeuroSdk.Websocket;
using Newtonsoft.Json;

namespace NeuroSdk.Messages.Outgoing
{
    [PublicAPI]
    public sealed class ActionResult : OutgoingMessageBuilder
    {
        public ActionResult(string id, ExecutionResult result)
        {
            _id = id;
            _success = result.Successful;
            _message = result.Message;
        }

        protected override string Command => "action/result";

        [JsonProperty("id", Order = 0)]
        private readonly string _id;

        [JsonProperty("success", Order = 10)]
        private readonly bool _success;

        [JsonProperty("message", Order = 20)]
        private readonly string? _message;
    }
}
