BLUB App
========

The *BLUB App* provides a graphical front-end to *BLUB*. People can draw together, and there is an option to administrate BLUB. The app consists of a node.js webapp and communicates to the BLUB control board by UART.

To run the app, install node.js (http://nodejs.org/), ensure there is no other HTTP-Server running and execute

```bash
npm install socket.io node-static # package for real-time WebSocket implementation and package for static content
node ./app/app.js # start the webapp
```

The app is based on a small node.js tutorial: http://tutorialzine.com/2012/08/nodejs-drawing-game/
