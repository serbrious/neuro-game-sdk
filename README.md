# Neuro Game SDK

This repository contains the API documentation for allowing [Neuro-sama](https://twitch.tv/vedal987) to play games.

There are also SDKs available for Unity and Godot. If you would like to use the Neuro API in a different engine or programming language, you will have to implement the websocket protocol yourself. If you do so, consider opening a pull request to this repository to share your implementation with others.

Please familiarize yourself with the API documentation before using the SDKs.

## Information 

The SDKs have been created and optimized for turn-based games, namely Inscryption, Liar's Bar and Buckshot Roulette.

**Due to how the API works, it is extremely unlikely that real-time games will work with it.**

Since you need to describe the entire game state in text, and receive actions in text, only games where that is feasible will work with this API.
<details>
<summary>Examples</summary>

- Inscryption? yes
- Liar's Bar? yes
- Buckshot Roulette? yes
- Among Us? no!!!
- Skyrim? no
- League of Legends? no
- Celeste? no
- KTANE? yes i guess
- Uno? YES
- Monopoly? YES
- Euro Truck Sim? no
- CSGO? no lol
- Almost any visual novel ====> YES

</details>

## Links
- [API Documentation](./API/README.md)
- [Unity SDK Documentation](./Unity/README.md)
- [Unity Mod SDK Documentation]
- [Godot SDK Documentation]