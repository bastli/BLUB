var HOST = 'localhost';
var PORT = 3000;
var WEB_PORT = 80;

var activity_timeout = 60000;


// Constants: Tubes are the vertical columns, the preview is the whole preview frame.

var tubes = 62; // [#] Number of tubes
var bubble_width = 1; // [cm] The 2D projected width of a bubble
var bubble_height = 1.5; // [cm] The 2D projected height of a bubble
var tube_width = 1.5; // [cm] The width of one tube
var min_bubble_distance = 2.5; // [cm] The minimal vertical distance between two bubble centers
var output_speed = 12; // [cm per seconds] Speed of the bubbles
var small_bubble_offset = 0.4;// [cm] The approx. distance each bubble lift its upper predecessor bubbles.


var frame_height = 170; // [cm]
var bubble_preview_height = frame_height;
var minimal_visible_preview = 100; // [cm]
var insert_picture_offset = 10; // [cm]
var max_preview_mask_pieces = 100;

var blackiness = 0.6; //[0-1]


// Variables & Objects
var preview_frame_buffer;
var preview_vertical_bubbles = Math.ceil(bubble_preview_height / min_bubble_distance);

var active_frame_buffer;


function copy_preview_to_active(){
	active_frame_buffer = new Array(preview_vertical_bubbles);
  	for (var i = 0; i < preview_vertical_bubbles; i++) {
    	active_frame_buffer[i] = new Array(tubes);
  	}

  	for (var j = 0; j < tubes; j++) {
  		var count = 0;

  		for (var i = 0; i < preview_vertical_bubbles; i++) {
  			if (preview_frame_buffer[preview_vertical_bubbles - (1 + i)][j])
  			{
  				var offset = Math.round(count * small_bubble_offset / min_bubble_distance);
  				if (offset <= i)
    				active_frame_buffer[preview_vertical_bubbles - (1 + i - offset)][j] = true;
    			count++;
    		}
    	}
  	}
}


function create_frame_buffer(){
	preview_frame_buffer = new Array(preview_vertical_bubbles);
  	for (var i = 0; i < preview_vertical_bubbles; i++) {
    	preview_frame_buffer[i] = new Array(tubes);
  	}
}

create_frame_buffer();
copy_preview_to_active();




console.log("Framebuffer is " + preview_vertical_bubbles + " bubbles high.");


function drawPixel(coord) {
	if (coord.x >= 0 && coord.x < tubes && coord.y >= 0 && coord.y < preview_vertical_bubbles)
	{
		cleared = false;
		preview_frame_buffer[coord.y][coord.x] = true;
		return true;
	}

	return false;
}

function clearPreview(coord) {
	cleared = true;
	create_frame_buffer();
}

// Including libraries

var app = require('http').createServer(handler),
	io = require('socket.io').listen(app),
	static = require('node-static'), // for serving files
	net = require('net');

app.listen(WEB_PORT);

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

	console.log('Client connected: ' + socket.handshake.address.address + ":" + socket.handshake.address.port);

	// Start listening for mouse move events
	socket.on('drawPixel', function (data) {

		if (drawPixel(data.coord))
		{
			last_update = (new Date()).getTime();
			socket.broadcast.emit('drawPixel', data);
		}
	});

	socket.on('getFrameBuffer', function () {
		socket.emit('setFrameBuffer', {frameBuffer: preview_frame_buffer});
	});

	socket.on('clearPreview', function () {
		clearPreview();
		socket.broadcast.emit('clearPreview');
		last_update = (new Date()).getTime();
	});

	socket.on('disconnect', function () { console.log('Client disconnected: ' + socket.handshake.address.address + ":" + socket.handshake.address.port); });

	socket.emit('setFrameBuffer', {frameBuffer: preview_frame_buffer});

	// Debug code
	socket.on('changeSmallBubbleOffset', function(data) {
		small_bubble_offset = data.value;
	});
});


var cleared = false;
var offset = preview_vertical_bubbles;
var last_time = (new Date()).getTime();
var last_update = (new Date()).getTime();
var time_per_row = 1000 * min_bubble_distance / output_speed;
var full_byte = String.fromCharCode(254);
var all_LED = "L" + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + "\n";

function writeFrameBuffer() {

	var time = (new Date()).getTime();

	if (time - last_update > activity_timeout)
	{
		if (!cleared)
		{
			socket.write("t");
			
			offset = preview_vertical_bubbles;
			io.sockets.emit('setPreviewCountdown', {value: 0});

			clearPreview();
			io.sockets.emit('clearPreview');
		}

		return;
	}

	socket.write(all_LED);

	if ((new Date()).getTime() - last_time >= time_per_row)
	{
		offset += 1;

		io.sockets.emit('setPreviewCountdown', {value: (offset / (preview_vertical_bubbles - 1))});
		last_time = (new Date()).getTime()
	}

	if(offset >= preview_vertical_bubbles)
	{
		offset %= preview_vertical_bubbles;
		copy_preview_to_active();
	}

	var valve_states = "";

	for (var i = 0; i < tubes; i++)
	{
		if (active_frame_buffer[offset][i])
			valve_states += "1";
		else
			valve_states += "0";
	}

	socket.write("V" + valve_states + "00");
}



var interval = null;

var socket = null

try
{
  	socket = net.createConnection(PORT, HOST);
}
catch (e)
{
	socket = new net.Socket();
}

console.log('TCP socket created.');


socket.on('data', function(data) {
  // Log the response from the HTTP server.
  console.log('TCP response: ' + data);
}).on('connect', function() {

	console.log('TCP socket connected.');

  	socket.write("cBLUBapp\n");

  	interval = setInterval(writeFrameBuffer, 100);
}).on('end', function() {
	console.log('TCP socket ended.');

	clearInterval(interval);
}).on('close', function() {
	console.log('TCP socket closed.');
	

	setTimeout(function(){
		socket.connect(PORT, HOST);
	}, 10000);
}).on('error', function() {
	console.log('TCP socket has error.');

	socket.end();
});


process.on( 'SIGINT', function() {
	socket.end();
	console.log("TCP closed.");

	console.log("Gracefully shutting down from SIGINT (Ctrl-C)");
	// some other closing procedures go here
	process.exit();
})


console.log('Server running at http://localhost:' + WEB_PORT + '/');
