#nullable enable

using Cysharp.Threading.Tasks;
using NeuroSdk.Json;
using NeuroSdk.Websocket;

namespace NeuroSdk.Actions
{
    public abstract class BaseNeuroAction : INeuroAction
    {
        private readonly ActionWindow? _window;

        protected BaseNeuroAction(ActionWindow? window = null)
        {
            _window = window;
        }

        public abstract string Name { get; }
        protected abstract string Description { get; }
        protected abstract JsonSchema? Schema { get; }

        public virtual bool CanBeUsed() => true;

        ExecutionResult INeuroAction.Validate(ActionJData actionData, out object? parsedData)
        {
            ExecutionResult result = Validate(actionData, out parsedData);

            if (_window != null)
            {
                return _window.Result(result);
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
