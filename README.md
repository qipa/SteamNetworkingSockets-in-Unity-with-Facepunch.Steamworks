# SteamNetworkingSockets in Unity with Facepunch.Steamworks
 A simple implimentation of SteamNetworkingSockets using Facepunch.Steamworks
SteamNetworkingSockets is the same transport that CSGO uses, Which valve has made publicly available to use as part of the steam SDK. It allows for both peer to peer connections (using steams relay servers), and dedicated hosting. This project uses the relayed peer to peer, though it wouldnt be too difficult to switch it over to the dedicated server setup. This project is inspired by Tom Weilands TCP/UDP unity tutorials, so using this project will be very similar in the implimentation. 

This project also makes use of Steam Friends and Lobbies to be used with the relayed networking. It is by no means a complete implimentation of the features, this is something i recomend you do yourself using the Facepunch.Steamworks wiki: https://wiki.facepunch.com/steamworks/

Ontop of setting up a basic interface for using the SteamNetworkingSockets transport, this project impliments a very simple networked character controller, with a simple implimentation of client side prediction, which should mask network delay.

How to use the demo:
 - first install TextMeshPro
 - you must have steam open for the project to work
 - join someone elses lobby by right clicking their steam profile picture and clicking "join game"
 - WASD to move around. left Shift to sprint. each players steam name, steam id, inputs, and ping will be displayed on the UI.

Implimentation tutorial: https://docs.google.com/document/d/1JddgNGP8Np8ry60fB9KGXBX1APC-VosubdV93ELO-wM/edit?usp=sharing

This project is running Unity 2021.2.7f1, but should work on any unity version

For any questions you may have, you can join my discord server (discord.gg/SPVmPSM), or DM me directly (milk_drinker01#5765). Also, if this is helpful to you at all feel free to subscribe to my youtube, and check out the multiplayer FPS im developing: https://www.youtube.com/c/MilkDrinker01
