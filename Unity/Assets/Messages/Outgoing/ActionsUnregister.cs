using System.Collections.Generic;
using System.Linq;
using NeuroSdk.Actions;
using NeuroSdk.Messages.API;
using Newtonsoft.Json;

namespace NeuroSdk.Messages.Outgoing
{
    public sealed class ActionsUnregister : OutgoingMessageBuilder
    {
        public ActionsUnregister(IEnumerable<INeuroAction> actions)
        {
            Names = actions.Select(action => action.Name).ToList();
        }

        public ActionsUnregister(params INeuroAction[] actions) : this((IEnumerable<INeuroAction>) actions)
        {
        }

        protected override string Command => "actions/unregister";

        [JsonProperty("action_names")]
        public readonly List<string> Names;

        public override bool Merge(OutgoingMessageBuilder other)
        {
            if (other is ActionsUnregister actionsUnregister)
            {
                Names.RemoveAll(existingName => actionsUnregister.Names.Contains(existingName));
                Names.AddRange(actionsUnregister.Names);
                return true;
            }

            return false;
        }
    }
}
