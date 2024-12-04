#nullable enable

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using NeuroSdk.Websocket;

namespace NeuroSdk.Actions
{
    [PublicAPI]
    public abstract class NeuroAction : BaseNeuroAction
    {
        protected abstract ExecutionResult Validate(ActionJData actionData);
        protected abstract UniTask ExecuteAsync();

        protected sealed override ExecutionResult Validate(ActionJData actionData, out object? parsedData)
        {
            ExecutionResult result = Validate(actionData);
            parsedData = null;
            return result;
        }

        protected sealed override UniTask ExecuteAsync(object? data) => ExecuteAsync();
    }

    [PublicAPI]
    public abstract class NeuroAction<TData> : BaseNeuroAction
    {
        protected abstract ExecutionResult Validate(ActionJData actionData, out TData? parsedData);
        protected abstract UniTask ExecuteAsync(TData? parsedData);

        protected sealed override ExecutionResult Validate(ActionJData actionData, out object? parsedData)
        {
            ExecutionResult result = Validate(actionData, out TData? tParsedData);
            parsedData = tParsedData;
            return result;
        }

        protected sealed override UniTask ExecuteAsync(object? parsedData) => ExecuteAsync((TData?) parsedData);
    }

    [PublicAPI]
    public abstract class NeuroActionS<TData> : NeuroAction<TData?> where TData : struct
    {
    }
}
