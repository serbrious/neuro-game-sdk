#nullable enable

using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Actions;
using NeuroSdk.Messages.API;
using Newtonsoft.Json;

namespace NeuroSdk.Messages.Outgoing
{
    public sealed class ActionsForce : OutgoingMessageBuilder
    {
        public ActionsForce(string query, string? state, IEnumerable<INeuroAction> actions)
        {
            _query = query;
            _state = state;
            _actionNames = actions.Select(a => a.Name).ToArray();
        }

        public ActionsForce(string query, string? state, params INeuroAction[] actions)
            : this(query, state, (IEnumerable<INeuroAction>)actions)
        {
        }

        protected override string Command => "actions/force";

        [JsonProperty("state", Order = 0)]
        private readonly string? _state;

        [JsonProperty("query", Order = 10)]
        private readonly string _query;

        [JsonProperty("action_names", Order = 20)]
        private readonly string[] _actionNames;
    }
}
