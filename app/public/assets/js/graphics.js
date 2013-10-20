var bubble_channel_width = 1.5;
var output_speed = 1 * bubble_channel_width; // [cm per seconds] Speed of the bubbles
var small_bubble_offset = 1 * bubble_channel_width;// [cm]
var header_offset = 0;

// Calculate the header offset and register callback
function calculate_header_offset(){
	header_offset = $('header.navbar').height();
}

function onResize(event) {
	calculate_header_offset();
}

calculate_header_offset();


function create_bubble(x, y, width, height){
	// Check if bubble dimensions are possible. Return if invalid.
	if (width > height)
	{
		console.log('Cannot draw bubble: Wrong dimensions.');
		return false;
	}

	// Create a Paper.js Path to draw a line into it:
	var path = new Path();
	// Give the stroke a color
	path.fillColor = 'black';
	path.moveTo(new Point(x, y + width / 2 + header_offset));
	path.arcTo(new Point(x + width, y + width / 2 + header_offset));
	path.lineTo(new Point(x + width, y + height - width / 2 + header_offset));
	path.arcTo(new Point(x, y + height - width / 2 + header_offset));
	path.closePath();
	return path;
}

function create_red_bar(offset) {

	var path = new Path();
	// Give the stroke a color
	path.strokeColor = 'red';
	path.strokeWidth = 1;
	path.moveTo(new Point(0, offset + header_offset));
	path.lineTo(new Point(view.size.width, offset + header_offset));
	return path;
}

var bubbles = new Array();
for (var i = 0; i < 100; i++)
{
	bubbles.push(create_bubble(100, 100 - i * 400, 40, 200));
}
var red_bar = create_red_bar(2);

console.log(bubbles);


function onFrame(event) {
	for (var i = 0; i < 100; i++)
	{
		bubbles[i].translate(new Point(0, event.delta * 1000));
	}

	red_bar.translate(new Point(0, event.delta * 100));
}