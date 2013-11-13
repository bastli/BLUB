var HOST = '129.132.201.56';
var PORT = 3000;


// Constants: Tubes are the vertical columns, the preview is the whole preview frame.

var tubes = 62; // [#] Number of tubes
var bubble_width = 1; // [cm] The 2D projected width of a bubble
var bubble_height = 1.5; // [cm] The 2D projected height of a bubble
var tube_width = 1.5; // [cm] The width of one tube
var min_bubble_distance = 2.5; // [cm] The minimal vertical distance between two bubble centers
var output_speed = 12; // [cm per seconds] Speed of the bubbles
var small_bubble_offset = 1.5;// [cm] The approx. distance each bubble lift its upper predecessor bubbles.


var frame_height = 150; // [cm]
var bubble_preview_height = frame_height;
var minimal_visible_preview = 100; // [cm]
var insert_picture_offset = 10; // [cm]
var max_preview_mask_pieces = 100;

var blackiness = 0.6; //[0-1]


// Variables & Objects
var preview_frame_buffer;
var preview_vertical_bubbles = Math.ceil(bubble_preview_height / min_bubble_distance);



function create_frame_buffer(){
	preview_frame_buffer = new Array(preview_vertical_bubbles);
  	for (var i = 0; i < preview_vertical_bubbles; i++) {
    	preview_frame_buffer[i] = new Array(tubes);
  	}
}

create_frame_buffer();




console.log("Framebuffer is " + preview_vertical_bubbles + " bubbles high.");


function drawPixel(coord) {
	if (coord.x >= 0 && coord.x < tubes && coord.y >= 0 && coord.y < preview_vertical_bubbles)
	{
		preview_frame_buffer[coord.y][coord.x] = true;
		return true;
	}

	return false;
}

function clearPreview(coord) {
	create_frame_buffer();
}

// Including libraries

var app = require('http').createServer(handler),
	io = require('socket.io').listen(app),
	static = require('node-static'), // for serving files
	net = require('net');

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
//io.set('log level', 1);

// Listen for incoming connections from clients
io.sockets.on('connection', function (socket) {

	console.log('Client connected.');

	// Start listening for mouse move events
	socket.on('drawPixel', function (data) {

		if (drawPixel(data.coord))
		{
			socket.broadcast.emit('drawPixel', data);
		}
	});

	socket.on('getFrameBuffer', function () {
		socket.emit('setFrameBuffer', {frameBuffer: preview_frame_buffer});
	});

	socket.on('clearPreview', function () {
		clearPreview();
		socket.broadcast.emit('clearPreview');
	});

	socket.on('disconnect', function () { console.log('Client disconnected.'); });

	socket.emit('setFrameBuffer', {frameBuffer: preview_frame_buffer});
});


var offset = 0;
var last_time = (new Date()).getTime();
var time_per_row = 1000 * min_bubble_distance / output_speed;
var full_byte = String.fromCharCode(254);
var all_LED = "L" + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + full_byte + "\n";

console.log(full_byte);

function writeFrameBuffer() {

	socket.write(all_LED);

	var time = (new Date()).getTime()
	if ((new Date()).getTime() - last_time >= time_per_row)
	{
		offset += 1;
		last_time = (new Date()).getTime()
	}

	if(offset >= preview_vertical_bubbles)
		offset %= preview_vertical_bubbles;

	var valve_states = "";

	for (var i = 0; i < tubes; i++)
	{
		if (preview_frame_buffer[offset][i])
			valve_states += "1";
		else
			valve_states += "0";
	}

	socket.write("V" + valve_states + "00");
}


var socket = net.createConnection(PORT, HOST);
var interval = null;

console.log('Socket created.');
socket.on('data', function(data) {
  // Log the response from the HTTP server.
  console.log('TCP response: ' + data);
}).on('connect', function() {
  // Manually write an HTTP request.
  socket.write("cBLUBapp\n");
  interval = setInterval(writeFrameBuffer, 100);
}).on('end', function() {
	clearInterval(interval);
  console.log('TCP server disconnected.');
});


console.log('Server running at http://localhost:8124/');

