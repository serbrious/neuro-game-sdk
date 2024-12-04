#nullable enable

using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using NeuroSdk.Websocket;

namespace NeuroSdk.Actions
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public interface INeuroAction
    {
        string Name { get; }

        /// <summary>
        /// This is ONLY checked when the action is added to an ActionWindow, if it returns false the action won't be added.
        /// </summary>
        bool CanBeUsed();

        ExecutionResult Validate(ActionJData actionData, out object? data);
        UniTask ExecuteAsync(object? data);

        WsAction GetWsAction();
    }
}
