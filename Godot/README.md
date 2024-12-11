# Neuro Godot SDK

This is the documentation for the Godot version of the Neuro SDK. If you are looking for the general API documentation, look [here](../API/README.md).

This SDK has been built for and tested with Godot 4.1, but later versions of Godot 4 should also work.

If you encounter any issues while using this SDK, please open an issue in this repository.

## Installation

1. Clone or download this repository, then copy the [`Godot/addons`](./addons) folder into your Godot project folder. In your projects

## Setup

1. Enable the plugin:
    - In the Godot editor, go to `Project > Project Settings > Plugins`,
    - Click on the Enable checkbox on the `Neuro SDK` entry to enable the plugin.
2. Set the `game` variable in the [`res://addons/neuro-sdk/neuro_sdk_config.gd`](./addons/neuro-sdk/neuro_sdk_config.gd) script to the name of your game.
3. Set the `NEURO_SDK_WS_URL` environment variable to the websocket URL you use for testing.

### Usage

Please refer to the [`USAGE.md`](./USAGE.md) file for information on how to use the SDK.
