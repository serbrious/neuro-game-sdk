# Neuro Unity SDK

This is the documentation for the Unity version of the Neuro SDK. If you are looking for the general API documentation, look [here](../API/README.md).

This SDK has been built for and tested with Unity 2022.3, but other versions will probably work as well.

If you encounter any issues while using this SDK, please open an issue in this repository.

## Installation

> [!Note]  
> This package comes with a pre-built version of websocket-sharp.  
> If you are already using websocket-sharp in your project, set the `NEURO_SDK_DISABLE_WEBSOCKET_SHARP` scripting define symbol in your project settings to avoid conflicts.

### Using the Unity Package Manager (recommended)

1. Install UniTask in your project [(guide)](https://github.com/Cysharp/UniTask?tab=readme-ov-file#install-via-git-url)
2. Install from this git URL: `https://github.com/VedalAI/neuro-game-sdk.git?path=Unity/Assets`

### Manual Installation (if you want to make changes)

1. Install UniTask in your project [(guide)](https://github.com/Cysharp/UniTask?tab=readme-ov-file#install-via-git-url)
2. Install Newtonsoft Json in your project (com.unity.nuget.newtonsoft-json@3.2) [(package docs)](https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.2/manual/index.html)
3. Install NativeWebSocket in your project [(guide)](https://github.com/endel/NativeWebSocket?tab=readme-ov-file#install-via-upm-unity-package-manager)
4. Clone or download this repository, then copy the [`Unity/Assets`](./Assets/) folder into your Unity project's `Assets` or `Packages` folder.

## Setup

1. Drag the `NeuroSdk` prefab into whatever scenes you need to use it in. Ideally, it should be added to the first scene that is loaded, like the title screen or main menu. It will move itself into `DontDestroyOnLoad` after, and multiple instances will be automatically destroyed so you don't have to worry about them.
2. Fill in the `Game` field in the `Websocket Connection` component with the game name.
3. Set the `NEURO_SDK_WS_URL` environment variable to the websocket URL you use for testing.

### WebGL Additional Setup

1. Go to `Project Settings > Player` and set `Compression Format` to `Gzip` and enable `Decompression Fallback`.
2. Do one of the following:
    1. Bundle your build along with a web server that can query the `NEURO_SDK_WS_URL` environment variable via a GET request to `/$env/NEURO_SDK_WS_URL` [(Superbox's Web Bundler)](https://discord.com/channels/574720535888396288/1316605675631022192/1317153897340735581)
    2. Access your web server with the `WebSocketURL` URL parameter (ex. `http://localhost:8080?WebSocketURL=ws://localhost:8000`)

### Usage

Please refer to the [`USAGE.md`](./USAGE.md) file for information on how to use the SDK.
