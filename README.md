# Neuro Game SDK

This repository contains the API documentation for allowing [Neuro-sama](https://twitch.tv/vedal987) to play games.

There are also SDKs available for Unity and Godot. If you would like to use the Neuro API in a different engine or programming language, you will have to implement the websocket protocol yourself. If you do so, consider opening a pull request to this repository to share your implementation with others.

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
- League of Legends? no (thank god)
- Celeste? no
- KTANE? yes i guess
- Uno? YES
- Monopoly? YES
- Euro Truck Sim? no
- CSGO? no lol
- Almost any visual novel ====> YES
- Almost any card game ====> YES
- Any RTS ====> vedal said maybe but he has no clue what he's talking about
- Most FPP ====> NO
- Shooters ====> NO
- Platformers ====> NO
- Tic tac toe? yes

You get the idea. Turn based games in general are perfect for this. Anything else and you're kinda stretching the limits of what the API can do.

Vedal said you can use this for more complex games but he told me "you wouldn't get it" so you lot probably wouldn't get it either.

</details>

## Links

Please familiarize yourself with the API documentation before using the SDKs.

- [API Documentation](./API/README.md)
- [Unity SDK](./Unity/README.md)
- [Unity Mod SDK]
- [Godot SDK]
- [Randy](./Randy/README.md)