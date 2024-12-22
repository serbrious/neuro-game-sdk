# Neuro Unity SDK

This is the documentation for the Unity version of the Neuro SDK. If you are looking for the general API documentation, look [here](../API/README.md).

This SDK has been built for and tested with Unity 2022.3, but other versions will probably work as well.

If you encounter any issues while using this SDK, please open an issue in this repository.

## Installation

### In Unity

For installing this SDK in your project, you will first need to install the following dependencies:
- [UniTask](https://github.com/Cysharp/UniTask?tab=readme-ov-file#install-via-git-url)
- [Native WebSockets](https://github.com/endel/NativeWebSocket?tab=readme-ov-file#install-via-upm-unity-package-manager)

Afterwards, you can install the SDK in one of the following ways:
- Using the Unity Package Manager (recommended), install from the following git URL: `https://github.com/VedalAI/neuro-game-sdk.git?path=Unity/Assets`
- Otherwise, if you want to make local changes to the SDK, you can install it locally by cloning or downloading this repository. Afterwards, copy the [`Unity/Assets`](./Assets) folder into your Unity project's `Packages` folder and rename it to `NeuroSdk`. Unity should detect it as a package and install the dependencies that are available in the Unity registry. You still need to install the other git dependencies manually.

### For Modding

If you would like to use the SDK in a modded Unity environment, install the `VedalAI.NeuroSdk.Unity` NuGet package.

At the moment, due to the dependency on UniTask, only games built in Mono are supported. IL2CPP support is planned for the future.

## Setup

> [!Important]  
> Set the `NEURO_SDK_WS_URL` environment variable to the websocket URL you use for testing.

### Using Prefabs

Drag the `NeuroSdk` prefab into whatever scene you need to use it in. Ideally, it should be added to the first scene that is loaded, like the title screen or main menu. It will move itself into `DontDestroyOnLoad` after, and multiple instances will be automatically destroyed so you don't have to worry about them. Afterwards, fill in the `Game` field in the `Websocket Connection` component with the game name.

### Using Code

Call `NeuroSdkSetup.Initialize` with the name of the game that you are using. This will automatically create the necessary objects and set up the SDK. This is the only option you have if you are using the NeuroSdk in a modded environment, from a NuGet package.

### WebGL Additional Setup

1. Go to `Project Settings > Player` and set `Compression Format` to `Gzip` and enable `Decompression Fallback`.
2. Do one of the following:
    - Bundle your build along with a web server that can query the `NEURO_SDK_WS_URL` environment variable via a GET request to `/$env/NEURO_SDK_WS_URL` [(Superbox's Web Bundler)](https://github.com/Superbox2147/simple-http-server). 
    - Specify the `WebSocketURL` URL parameter (ex. `http://localhost:8080?WebSocketURL=ws://localhost:8000`)

> [!Note]  
> If you are building in WebGL and want to use the Neuro SDK, your application cannot be run on itch.io or other websites which embed it, instead it needs to be manually run on localhost.  
> As such, it is technically not a "browser game" anymore, so you might as well build it normally.

### Usage

Please refer to the [`USAGE.md`](./USAGE.md) file for information on how to use the SDK.
