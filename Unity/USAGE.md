# Neuro Unity SDK Usage

## Sending Contexts

For sending context messages, you can use the `static void Context.Send(string message, bool silent)` method.

## Creating Custom Actions

In order to create a custom action, you can extend either the `NeuroAction` or `NeuroAction<T>` class. The difference is explained below.

You will need to implement the `Name`, `Description` and `Schema` of the action that you are creating, as well as a `Validate` and `ExecuteAsync` method.

The `Validate` method should validate the incoming data from json, and make sure that it's correct, and it should also perform any kind of initial verifications and finding objects. For example, in Inscryption, checking that the card that Neuro is trying to play is valid, and that there is enough bones for it, as well as finding the actual Card object and saving that as state. At the end you should return either `ExecutionResult.Success()` or `ExecutionResult.Failure(string message)`. 

In order to pass state or context between the `Validate` and `ExecuteAsync` methods, you can use the `parsedData` out parameter. This will be passed to the `ExecuteAsync` method when it is called. The type of the `parsedData` parameter is the type parameter of the `NeuroAction<T>` class. If you have no context to pass, you can use the class without the generic type parameter.

The `ExecuteAsync` method should fully perform whaat Neuro requested. By this point, the action result has already been sent, you need to try your best to execute it.

## Registered Actions

For registering semi-permanent actions, you can use the method `static void NeuroActionHandler.RegisterActions(INeuroAction[] actions)`.

Afterwards, if you want to unregister them, you can call `static void NeuroActionHandler.UnregisterActions(INeuroAction[] actions)`. Removal is done by checking the name of the action, so even if you pass in a different instance of the same action, it will still be removed. REMEMBER, the name is a unique identifier.

There's also `static void NeuroActionHandler.UnregisterActions(string[] actionNames)` which makes more sense I guess, the above function is just for convenience.

> [!Caution]
> The Unity SDK currently handles overriding actions with the same name differently than the Neuro API.  
> The Neuro API will ignore any attempts at registering an action with the same name as an already registered action, even if the schema or description is different.  
> The Neuro Unity SDK will always override the existing action with the new one.  
> This needs to be fixed eventually.

## Action Windows

For using ephemeral actions, such as in a turn-based game during the player's turn, you can use the `ActionWindow` class.

Create an instance using `static ActionWindow ActionWindow.Create(Transform parent)`. Be careful with what parent you choose, because if that object is destroyed, the window will be automatically ended.

After you have finished setting up your action window, you can call `void ActionWindow.Register()` to register it with Neuro. This will make it immutable and register all of the actions over the websocket.

An `ActionWindow` can be in one of 4 possible states:
- `Building`: This window has just been created and is currently being setup.
- `Registered`: This window has been registered and is now immutable and waiting for a response.
- `Forced`: An action force has been sent for this window.
- `Ended`: This window has successfuly received an action and is waiting to be destroyed.

### Adding a Context

If you want to add a context message to an action window, you can use the method `void ActionWindow.SetContext(string message, bool silent)`. This will send the context message when the window is registered. Use this to pass in state relevant to the available actions.

### Forcing

If you want the action window to be forced (i.e. Neuro must perform an action as soon as she can), you can use the of the methods `void ActionWindow.SetForce(...)`. This allows you to pass in various parameters that dictate when the action force for the window should be sent.

### Ending

If you want the action window to end programmatically, you can call one of the `void ActionWindow.SetEnd(...)` methods to describe when that should happen.

When an action window is ended, all of the actions that were registered with it are unregistered automatically.

An action window will be automatically ended when an action is received and is executed successfully.

### Adding Actions

Using the `void ActionWindow.AddAction(INeuroAction action)` method, you can add an action to the window. This will add this action as one of the possible responses that Neuro can pick from.

Each action window can register any number of actions, but only one of them will be returned by Neuro. This just asks Neuro to pick one of them basically.

> [!Caution]
> The Unity SDK currently handles overriding actions with the same name differently than the Neuro API.  
> The Neuro API will ignore any attempts at registering an action with the same name as an already registered action, even if the schema or description is different.  
> The Neuro Unity SDK will always override the existing action with the new one.  
> This needs to be fixed eventually.
