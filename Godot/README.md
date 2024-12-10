# Neuro Godot SDK

This is the documentation for the Godot version of the Neuro SDK. If you are looking for the general API documentation, look [here](../API/README.md).

If you encounter any issues while using this SDK, please open an issue in this repository.

## Installation

1. Clone or download this repository, then copy the [`Godot/addons`](./addons) folder into your Godot project folder.

## Setup

1. Add `neuro-sdk.tscn` to your project autoloads:
    - In the Godot editor, go to `Project > Project Settings > Autoload`,
    - Click the folder icon and navigate to [`res://addons/neuro-sdk`](./addons/neuro-sdk/),
    - Select `neuro-sdk.tscn` and then click `Open`,
    - Disable `Global Variable`, then click `Add`.
2. Set the `game` variable in the [`res://addons/neuro-sdk/neuro_sdk_config.gd`](./addons/neuro-sdk/neuro_sdk_config.gd) script to the name of your game.
3. Set the `NEURO_SDK_WS_URL` environment variable to the websocket URL you use for testing.

### Usage

Please refer to the [`USAGE.md`](./USAGE.md) file for information on how to use the SDK.