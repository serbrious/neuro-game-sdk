#nullable enable

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

namespace NeuroSdk.Actions
{
    [PublicAPI]
    public abstract class BaseNeuroAction : INeuroAction
    {
        /// <summary>
        /// The value that was passed to the actionWindow parameter in the constructor
        /// </summary>
        protected readonly ActionWindow? ActionWindow;

        protected BaseNeuroAction(ActionWindow? actionWindow)
        {
            ActionWindow = actionWindow;
        }

        public abstract string Name { get; }
        protected abstract string Description { get; }
        protected abstract JsonSchema? Schema { get; }

        public virtual bool CanBeUsed() => true;

        ExecutionResult INeuroAction.Validate(ActionJData actionData, out object? parsedData)
        {
            ExecutionResult result = Validate(actionData, out parsedData);

            if (ActionWindow != null)
            {
                return ActionWindow.Result(result);
            }

            return result;
        }

        UniTask INeuroAction.ExecuteAsync(object? data) => ExecuteAsync(data);

        public virtual WsAction GetWsAction()
        {
            return new WsAction(Name, Description, Schema);
        }

        protected abstract ExecutionResult Validate(ActionJData actionData, out object? parsedData);
        protected abstract UniTask ExecuteAsync(object? data);
    }
}
