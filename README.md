ozwego
======
Ozwego is a real-time multiplayer word game for the Windows Store.  Ozwego mimics the gameplay of the popular fast paced game of Bananagrams.  Users can find friends to play against, or face other online users through the matchmaking system.

Ozwego uses the Xaml/C# framework for its client implementation.  The UI is optimized for both touch and mouse, so that both desktop and tablet users can play competitively against each other.

The backend is implemented in C# on a Windows Azure Virtual Machine, for a highly scalable solution that can support thousands of players.  The backend was designed to be multi-instance with a frontend load balancer to guarantee the service will have 99% uptime as per Azure's uptime guarantee.

Gameplay is in real-time, so server-client communication is handled through TCP.  
