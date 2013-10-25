// Constants: Tubes are the vertical columns, the spool is the whole preview frame.

var tubes = 64; // [#] Number of tubes
var bubble_width = 1; // [cm] The 2D projected width of a bubble
var bubble_height = 1.5; // [cm] The 2D projected height of a bubble
var tube_width = 1.5; // [cm] The width of one tube
var min_bubble_distance = 2.5; // [cm] The minimal vertical distance between two bubble centers
var output_speed = 10; // [cm per seconds] Speed of the bubbles
var small_bubble_offset = 1.5;// [cm] The approx. distance each bubble lift its upper predecessor bubbles.


var frame_height = 200; // [cm]
var spool_length = 300; // [cm]
var minimal_visible_spool = 100; // [cm]
var insert_picture_offset = 10; // [cm]



// Variables & Objects
var spool_max_vertical_bubbles = Math.ceil(spool_length / min_bubble_distance);
var spool_fitting_vertical_bubbles;

var spool_bounds;
var spool_rectangle;
var pixel_per_cm;
var bubble_piece;
var spool_group;


function create_bubble(width, height){
	// Check if bubble dimensions are possible. Return if invalid.
	if (width > height)
	{
		console.log('Problem creating bubble: Wrong dimensions, to short. Creating a round one with same width.');
		height = width;
	}

	// Create a Paper.js Path to draw a line into it:
	var path = new Path();
	path.remove(); // Do not draw
	// Give the stroke a color
	path.fillColor = 'black';
	path.moveTo(new Point(0, width / 2));
	path.arcTo(new Point(width, width / 2));
	path.lineTo(new Point(width, height - width / 2));
	path.arcTo(new Point(0, height - width / 2));
	path.closePath();
	var raster_bubble = path.rasterize();
	raster_bubble.remove(); // Do not draw raster bubble
	return raster_bubble;
}

function create_red_bar(offset) {
	var path = new Path();
	// Give the stroke a color
	path.strokeColor = 'red';
	path.strokeWidth = 1;
	path.moveTo(new Point(0, offset));
	path.lineTo(new Point(view.size.width, offset));
	path.remove();
	return path;
}

var shape;

function init_bubble_spool(){

	// Decide wether we scale the spool for widescreen (landscape, e.g. notebooks) or portrait (e.g. smartphones) devices.
	if (view.size.width * minimal_visible_spool / (tubes * tube_width) <= view.size.height)
	{
		console.log("Spool gets mapped on portrait display.");
		pixel_per_cm = view.size.width / (tubes * tube_width);
		spool_bounds = new Rectangle(new Point(0,0), new Size(view.size.width, Math.ceil(spool_length * pixel_per_cm)));
	}
	else 
	{
		console.log("Spool gets mapped on widescreen display.");
		pixel_per_cm = view.size.height / minimal_visible_spool;
		var side = Math.floor((view.size.width - pixel_per_cm * tubes * tube_width) / 2);
		spool_bounds = new Rectangle(new Point(side,0), new Size(Math.ceil(tubes * tube_width * pixel_per_cm), Math.ceil(spool_length * pixel_per_cm)));
	}

	console.log(spool_bounds);

	if (spool_rectangle)
		spool_rectangle.remove();

	spool_rectangle = new Path.Rectangle(spool_bounds);
	spool_rectangle.fillColor = new Color(0,0,0,0.03);

	spool_fitting_vertical_bubbles = Math.ceil(view.size.height / pixel_per_cm / min_bubble_distance) + 2;

	bubble_piece = new Symbol(create_bubble(Math.floor(bubble_width * pixel_per_cm), Math.floor(bubble_height * pixel_per_cm)));

	if (spool_group)
		spool_group.remove();
	spool_group = new Group();

	for (var i = 0; i < tubes; i++)
	{
		for (var j = 0; j < spool_fitting_vertical_bubbles; j++)
		{
			var bubble_placed = bubble_piece.place();
			bubble_placed.translate(new Point(Math.floor(tube_width * pixel_per_cm * (1/2 + i)), Math.floor(j * min_bubble_distance * pixel_per_cm)));
			spool_group.addChild(bubble_placed);
		}
	}

	spool_group.remove();
	spool_group = spool_group.rasterize();
	spool_group.position += new Point(Math.floor(spool_bounds.center.x - spool_group.position.x), 0);
}


function onResize(event){
	init_bubble_spool();
}

var offset_movement = 0;

function onFrame(event){
	offset_movement += pixel_per_cm * event.delta * output_speed;
	if (offset_movement >= min_bubble_distance * pixel_per_cm)
		offset_movement %= min_bubble_distance * pixel_per_cm;

	spool_group.position = new Point(spool_group.position.x, -offset_movement + view.size.height / 2);
}