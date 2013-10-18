// Including libraries

var app = require('http').createServer(handler),
	io = require('socket.io').listen(app),
	static = require('node-static'); // for serving files

app.listen(8124);

function handler (request, response) {
	request.addListener('end', function () {
        fileServer.serve(request, response);
    }).resume();
}


// This will make all the files in the current folder
// accessible from the web
var fileServer = new static.Server('./public');




	
// This is the port for our web server.
// you will need to go to http://localhost:8124 to see it

// Delete this row if you want to see debug messages
io.set('log level', 1);

// Listen for incoming connections from clients
io.sockets.on('connection', function (socket) {

	// Start listening for mouse move events
	socket.on('mousemove', function (data) {
		
		// This line sends the event (broadcasts it)
		// to everyone except the originating client.
		socket.broadcast.emit('moving', data);
	});
});

console.log('Server running at http://localhost:8124/');