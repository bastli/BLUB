=== BLUB App ===

The 'BLUB App' provides a graphical front-end to BLUB. People can draw together, and there is an option to administrate 'BLUB'. The app consists of a node.js webapp and communicates to the 'BLUB' control board by UART.

To run the app, install node.js (http://nodejs.org/), ensure there is no other Http-Server running and execute

```sh
npm install socket.io # package for real-time WebSocket
npm install node-static # package for static content
node ./app/app.js # start the webapp
```
