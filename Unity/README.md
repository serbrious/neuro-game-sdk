# Neuro Unity SDK

This is the documentation for the Unity version of the Neuro SDK. If you are looking for the general API documentation, look [here](../API/README.md).

## Installation

> [!Note]  
> This package comes with a pre-built version of websocket-sharp.  
> If you are already using websocket-sharp in your project, set the `NEURO_SDK_DISABLE_WEBSOCKET_SHARP` scripting define symbol in your project settings to avoid conflicts.

### Using the Unity Package Manager (recommended)

1. Install UniTask in your project [(guide)](https://github.com/Cysharp/UniTask?tab=readme-ov-file#install-via-git-url)
2. Install from this git URL: `https://github.com/VedalAI/neuro-game-sdk.git?path=Unity/Assets`

### Manual Installation

Clone or download this repository, then copy the `Unity/Assets` folder into your Unity project's `Assets` or `Packages` folder.

## Setup

1. Drag the `NeuroSdk` prefab into whatever scenes you need to use it in. Ideally, it should be added to the first scene that is loaded, like the title screen or main menu. It will move itself into `DontDestroyOnLoad` after, and multiple instances will be automatically destroyed so you don't have to worry about them.
2. Fill in the `Game` field in the `Websocket Connection` component with the game name.

### For testing:

3. Fill in the `Websocket Url` field with the URL of the websocket server you are running (Randy). It should look like `ws://localhost:8080`.
4. Enable the `Testing Websocket Starter` component. This component will automatically start the websocket on load.

### For production:

3. From your own script, set the `websocketUrl` field on the `WebsocketConnection` instance to the URL of the websocket server which you grabbed from whatever config you are using.
4. Invoke the `StartWs` method on the `WebsocketConnection` instance.

### Usage

Please refer to the [`USAGE.md`](./USAGE.md) file for information on how to use the SDK.