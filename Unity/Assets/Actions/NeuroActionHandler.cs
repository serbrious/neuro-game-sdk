#nullable enable

using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NeuroSdk.Messages.Outgoing;
using NeuroSdk.Websocket;
using UnityEngine;

namespace NeuroSdk.Actions
{
    public sealed class NeuroActionHandler : MonoBehaviour
    {
        private static List<INeuroAction> _currentlyRegisteredActions = new();
        private static readonly List<INeuroAction> _dyingActions = new();

        public static INeuroAction? GetRegistered(string name) => _currentlyRegisteredActions.FirstOrDefault(a => a.Name == name);
        public static bool IsRecentlyUnregistered(string name) => _dyingActions.Any(a => a.Name == name);

        private void OnApplicationQuit()
        {
            WebsocketConnection.SendImmediate(new ActionsUnregister(_currentlyRegisteredActions));
            _currentlyRegisteredActions = null!;
        }

        public static void RegisterActions(bool sendToNeuro, IReadOnlyCollection<INeuroAction> newActions)
        {
            _currentlyRegisteredActions.RemoveAll(oldAction => newActions.Any(newAction => oldAction.Name == newAction.Name));
            _dyingActions.RemoveAll(oldAction => newActions.Any(newAction => oldAction.Name == newAction.Name));
            _currentlyRegisteredActions.AddRange(newActions);
            if (sendToNeuro) WebsocketConnection.TrySend(new ActionsRegister(newActions));
        }

        public static void RegisterActions(bool sendToNeuro, params INeuroAction[] newActions)
            => RegisterActions(sendToNeuro, (IReadOnlyCollection<INeuroAction>) newActions);

        public static void UnregisterActions(bool sendToNeuro, IReadOnlyCollection<INeuroAction> removeActionsList)
        {
            INeuroAction[] actionsToRemove = _currentlyRegisteredActions.Where(oldAction => removeActionsList.Any(removeAction => oldAction.Name == removeAction.Name)).ToArray();

            _currentlyRegisteredActions.RemoveAll(actionsToRemove.Contains);
            _dyingActions.AddRange(actionsToRemove);
            removeActions().Forget();

            if (sendToNeuro) WebsocketConnection.TrySend(new ActionsUnregister(removeActionsList));

            return;

            async UniTask removeActions()
            {
                await UniTask.Delay(10000);
                _dyingActions.RemoveAll(actionsToRemove.Contains);
            }
        }

        public static void UnregisterActions(bool sendToNeuro, params INeuroAction[] removeActionsList)
            => UnregisterActions(sendToNeuro, (IReadOnlyCollection<INeuroAction>) removeActionsList);

        public static void ResendRegisteredActions()
        {
            WebsocketConnection.Instance!.Send(new ActionsRegister(_currentlyRegisteredActions));
        }
    }
}
