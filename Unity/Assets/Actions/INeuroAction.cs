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

        bool CanBeUsed();

        ExecutionResult Validate(ActionJData actionData, out object? data);
        UniTask ExecuteAsync(object? data);

        WsAction GetWsAction();
    }
}
