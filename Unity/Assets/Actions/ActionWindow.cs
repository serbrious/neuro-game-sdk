#nullable enable

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NeuroSdk.Messages.Outgoing;
using NeuroSdk.Websocket;
using UnityEngine;

namespace NeuroSdk.Actions
{
    [PublicAPI]
    public sealed class ActionWindow : MonoBehaviour
    {
        #region Creation

        private static bool _isCreatedCorrectly = false;

        public static ActionWindow Create(Transform parent)
        {
            try
            {
                _isCreatedCorrectly = true;
                GameObject obj = new("ActionWindow");
                obj.transform.SetParent(parent);
                return obj.AddComponent<ActionWindow>();
            }
            finally
            {
                _isCreatedCorrectly = false;
            }
        }

        private void Awake()
        {
            if (!_isCreatedCorrectly)
            {
                Debug.LogError("ActionWindow should be created using Create method. This ActionWindow was either created with AddComponent or with Instantiate.");
                Destroy(this);
            }
        }

        #endregion

        #region State

        private enum State
        {
            Building,
            Registered,
            Forced,
            Ended
        }

        private State _state = State.Building;

        private bool ValidateFrozen()
        {
            if (_state != State.Building)
            {
                Debug.LogError("Cannot mutate ActionWindow after it has been registered.");
                return false;
            }

            return true;
        }

        public void Register()
        {
            if (_state != State.Building)
            {
                Debug.LogError("Cannot register an ActionWindow multiple times.");
                return;
            }

            if (!string.IsNullOrEmpty(_contextMessage))
                Context.Send(_contextMessage, _contextSilent!.Value);
            NeuroActionHandler.RegisterActions(true, _actions);

            _state = State.Registered;
        }

        #endregion

        #region Context

        private string? _contextMessage;
        private bool? _contextSilent;

        public void SetContext(string message, bool silent = false)
        {
            if (!ValidateFrozen()) return;

            _contextMessage = message;
            _contextSilent = silent;
        }

        #endregion

        #region Actions

        private readonly List<INeuroAction> _actions = new();

        public void AddAction(INeuroAction action)
        {
            if (!ValidateFrozen()) return;

            if (action.CanBeUsed()) _actions.Add(action);
        }

        #endregion

        #region Forcing

        private Func<bool>? _shouldForceFunc;
        private Func<string>? _forceQueryGetter;
        private Func<string?>? _forceStateGetter;

        public void SetForce(Func<bool> shouldForce, Func<string> queryGetter, Func<string?> stateGetter)
        {
            if (!ValidateFrozen()) return;

            _shouldForceFunc = shouldForce;
            _forceQueryGetter = queryGetter;
            _forceStateGetter = stateGetter;
        }

        public void SetForce(Func<bool> shouldForce, string query, string? state)
            => SetForce(shouldForce, () => query, () => state);

        public void SetForce(float afterSeconds, Func<string> queryGetter, Func<string?> stateGetter)
        {
            float time = afterSeconds;

            SetForce(shouldForce, queryGetter, stateGetter);
            return;

            bool shouldForce()
            {
                // called every update
                time -= Time.deltaTime;
                return time <= 0;
            }
        }

        public void SetForce(float afterSeconds, string query, string? state)
            => SetForce(afterSeconds, () => query, () => state);

        #endregion

        private void SendForce()
        {
            _state = State.Forced;
            _shouldForceFunc = null;
            WebsocketConnection.TrySend(new ActionsForce(_forceQueryGetter!(), _forceStateGetter!(), _actions));
        }

        #region Ending

        private Func<bool>? _shouldEndFunc;

        public void SetEnd(Func<bool> shouldEnd)
        {
            if (!ValidateFrozen()) return;

            _shouldEndFunc = shouldEnd;
        }

        public void SetEnd(float afterSeconds)
        {
            float time = afterSeconds;

            SetEnd(shouldEnd);
            return;

            bool shouldEnd()
            {
                // called every update
                time -= Time.deltaTime;
                return time <= 0;
            }
        }

        #endregion

        #region Handling

        public ExecutionResult Result(ExecutionResult result)
        {
            if (_state <= State.Building) throw new InvalidOperationException("Cannot handle a result before registering thet ActionWindow.");
            if (_state >= State.Ended) throw new InvalidOperationException("Cannot handle a result after the ActionWindow has ended.");

            if (result.Successful) End();
            // else if (_state == State.Forced) SendForce(); // Vedal is now responsible for retrying forces

            return result;
        }

        private void Update()
        {
            if (_state != State.Registered) return;

            if (_shouldForceFunc != null && _shouldForceFunc())
            {
                SendForce();
            }

            if (_shouldEndFunc != null && _shouldEndFunc())
            {
                End();
            }
        }

        private void End()
        {
            NeuroActionHandler.UnregisterActions(true, _actions);
            _shouldForceFunc = null;
            _shouldEndFunc = null;
            _state = State.Ended;
            Destroy(gameObject);
        }

        #endregion
    }
}
