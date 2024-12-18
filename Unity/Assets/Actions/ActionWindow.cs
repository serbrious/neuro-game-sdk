#nullable enable

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NeuroSdk.Messages.Outgoing;
using NeuroSdk.Websocket;
using UnityEngine;

namespace NeuroSdk.Actions
{
    /// <summary>
    /// A wrapper class around the concept of an action window, which handles sending context, registering actions, forcing actions and unregistering the actions afterwards.
    /// </summary>
    [PublicAPI]
    public sealed class ActionWindow : MonoBehaviour
    {
        #region Creation

        private static bool _isCreatedCorrectly = false;

        /// <summary>
        /// Creates a new ActionWindow. If the parent is destroyed, this ActionWindow will be automatically ended.
        /// </summary>
        public static ActionWindow Create(GameObject parent)
        {
            try
            {
                _isCreatedCorrectly = true;
                return parent.AddComponent<ActionWindow>();
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

        /// <summary>
        /// Register this ActionWindow, sending an actions register to the websocket and making this window immutable.
        /// </summary>
        public void Register()
        {
            if (_state != State.Building)
            {
                Debug.LogError("Cannot register an ActionWindow multiple times.");
                return;
            }

            if (_actions.Count == 0)
            {
                Debug.LogError("Cannot register an ActionWindow with no actions.");
                return;
            }

            if (_contextMessage is not (null or ""))
                Context.Send(_contextMessage, _contextSilent!.Value);
            NeuroActionHandler.RegisterActions(_actions);

            _state = State.Registered;
        }

        #endregion

        #region Context

        private string? _contextMessage;
        private bool? _contextSilent;

        /// <summary>
        /// Set a context message to be sent alongside the action register.
        /// </summary>
        public void SetContext(string message, bool silent = false)
        {
            if (!ValidateFrozen()) return;

            _contextMessage = message;
            _contextSilent = silent;
        }

        #endregion

        #region Actions

        private readonly List<INeuroAction> _actions = new();

        /// <summary>
        /// Add a new action to the list of possible actions that Neuro can pick from
        /// </summary>
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
        private bool? _forceEphemeralContext;

        /// <summary>
        /// Specify a condition under which the actions should be forced.
        /// </summary>
        /// <param name="shouldForce">When this returns true, the actions will be forced.</param>
        /// <param name="queryGetter">A getter for the query of the action force, invoked at force-time.</param>
        /// <param name="stateGetter">A getter for the state of the action force, invoked at force-time.</param>
        /// <param name="ephemeralContext">If true, the query and state won't be remembered after the action force is finished.</param>
        public void SetForce(Func<bool> shouldForce, Func<string> queryGetter, Func<string?> stateGetter, bool? ephemeralContext)
        {
            if (!ValidateFrozen()) return;

            _shouldForceFunc = shouldForce;
            _forceQueryGetter = queryGetter;
            _forceStateGetter = stateGetter;
            _forceEphemeralContext = ephemeralContext;
        }

        /// <summary>
        /// Specify a condition under which the actions should be forced.
        /// </summary>
        /// <param name="shouldForce">When this returns true, the actions will be forced.</param>
        /// <param name="ephemeralContext">If true, the query and state won't be remembered after the action force is finished.</param>
        public void SetForce(Func<bool> shouldForce, string query, string? state, bool? ephemeralContext)
            => SetForce(shouldForce, () => query, () => state, ephemeralContext);

        /// <summary>
        /// Specify a time in seconds after which the actions should be forced.
        /// </summary>
        /// <param name="queryGetter">A getter for the query of the action force, invoked at force-time.</param>
        /// <param name="stateGetter">A getter for the state of the action force, invoked at force-time.</param>
        /// <param name="ephemeralContext">If true, the query and state won't be remembered after the action force is finished.</param>
        public void SetForce(float afterSeconds, Func<string> queryGetter, Func<string?> stateGetter, bool? ephemeralContext)
        {
            float time = afterSeconds;

            SetForce(shouldForce, queryGetter, stateGetter, ephemeralContext);
            return;

            bool shouldForce()
            {
                // called every update
                time -= Time.deltaTime;
                return time <= 0;
            }
        }

        /// <summary>
        /// Specify a time in seconds after which the actions should be forced.
        /// </summary>
        /// <param name="ephemeralContext">If true, the query and state won't be remembered after the action force is finished.</param>
        public void SetForce(float afterSeconds, string query, string? state, bool? ephemeralContext)
            => SetForce(afterSeconds, () => query, () => state, ephemeralContext);

        private void SendForce()
        {
            _state = State.Forced;
            _shouldForceFunc = null;
            WebsocketConnection.TrySend(new ActionsForce(_forceQueryGetter!(), _forceStateGetter!(), _forceEphemeralContext, _actions));
        }

        #endregion

        #region Ending

        private Func<bool>? _shouldEndFunc;

        /// <summary>
        /// Specify a condition under which the actions should be unregisterd and this window closed.
        /// </summary>
        /// <param name="shouldEnd">When this returns true, the actions will be unregistered.</param>
        public void SetEnd(Func<bool> shouldEnd)
        {
            if (!ValidateFrozen()) return;

            _shouldEndFunc = shouldEnd;
        }

        /// <summary>
        /// Specify a time in seconds after which the actions should be unregistered and this window closed.
        /// </summary>
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

        private void OnDestroy()
        {
            if (_state == State.Ended) return;
            End();
        }

        #endregion

        #region Handling

        /// <summary>
        /// Run an <see cref="ExecutionResult"/> through this ActionWindow. This is invoked automatically in <see cref="NeuroAction"/>, but if you are not using that class you will need to invoke this manually.
        /// </summary>
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
            NeuroActionHandler.UnregisterActions(_actions);
            _shouldForceFunc = null;
            _shouldEndFunc = null;
            _state = State.Ended;
            Destroy(this);
        }

        #endregion
    }
}
