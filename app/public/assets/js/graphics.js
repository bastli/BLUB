// Constants: Tubes are the vertical columns, the preview is the whole preview frame.

var tubes = 62; // [#] Number of tubes
var bubble_width = 1; // [cm] The 2D projected width of a bubble
var bubble_height = 1.5; // [cm] The 2D projected height of a bubble
var tube_width = 1.5; // [cm] The width of one tube
var min_bubble_distance = 2.5; // [cm] The minimal vertical distance between two bubble centers
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

var pixel_per_cm;

var preview_bubble_grid = new Group();
var preview_bubble_mask = new Group();

var bubble_symbol;
var bubble_mask_symbol;

function create_bubble(width, height){
	// Check if bubble dimensions are possible. If not, create a circle bubble.
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

function create_red_bar() {
	var path = new Path();
	// Give the stroke a color
	path.strokeColor = 'red';
	path.strokeWidth = 1;
	path.moveTo(new Point(0, 0));
	path.lineTo(new Point(view.size.width, 0));
	path.remove();
	return path;
}

function calculateCoord(point) {
	var coord = new Point(Math.floor(point.x / (tube_width * pixel_per_cm)), Math.floor(point.y / (min_bubble_distance * pixel_per_cm)));
	if(coord.x < 0 || coord.x >= tubes || coord.y < 0 || coord.y >= preview_vertical_bubbles)
		return null;
	else
		return coord;
}

function calculatePosition(coord) {
	return new Point((coord.x + 1/2) * pixel_per_cm * tube_width, (coord.y + 1/2) * pixel_per_cm * min_bubble_distance);
}

function drawPixelAtCoord(coord)
{
	if (coord === null)
		return;

	var mask_piece_position = calculatePosition(coord);
	
	var bubble_mask_piece = bubble_mask_symbol.place();
	bubble_mask_piece.position = mask_piece_position;
	preview_bubble_mask.addChild(bubble_mask_piece);

	rasterizePreviewBubbleMask();
}

function socketDrawPixelAtCoord(coord)
{
	drawPixelAtCoord(coord);
	window.socket.emit('drawPixel', {coord: {x: coord.x, y: coord.y}});
}

function rasterizePreviewBubbleMask() {
	if (preview_bubble_mask.children.length > max_preview_mask_pieces)
	{
		var preview_bubble_mask_rasterized = preview_bubble_mask.rasterize();
		preview_bubble_mask.removeChildren();
		preview_bubble_mask.addChild(preview_bubble_mask_rasterized);
	}
}

function init_bubble_preview_bubbles() {
	console.log("Canvas size is " + view.viewSize.width + " x " + view.viewSize.height + " rows in preview.");
	console.log("Bubble grid will have " + preview_vertical_bubbles + " rows in preview.");

	// Create bubble symbol
	bubble_symbol = new Symbol(create_bubble(Math.floor(bubble_width * pixel_per_cm), Math.floor(bubble_height * pixel_per_cm)));

	// Create preview bubble grid (looks like a pixel raster fully out of bubbles).
	preview_bubble_grid.remove();
	preview_bubble_grid = new Group();

	// Construct Bubble texture
	for (var i = 0; i < tubes; i++)
	{
		for (var j = 0; j < preview_vertical_bubbles; j++)
		{
			var bubble_placed = bubble_symbol.place();
			bubble_placed.translate(new Point(Math.floor(tube_width * pixel_per_cm * (1/2 + i)), Math.floor((1/2 + j) * min_bubble_distance * pixel_per_cm)));
			preview_bubble_grid.addChild(bubble_placed);
		}
	}

	// Rasterize preview_bubble_grid for speed up.
	var preview_bubble_grid_unrasterized = preview_bubble_grid;
	preview_bubble_grid = preview_bubble_grid.rasterize();
	preview_bubble_grid_unrasterized.remove();
	
	
	// Create preview bubble mask to only show the masked bubbles in the bubble grid.
	preview_bubble_mask.remove();
	preview_bubble_mask = new Group();
	preview_bubble_mask.name = "preview_bubble_mask";
	preview_bubble_mask.blendMode = "destination-atop";


	// Create one single bubble mask rectangle. To be used whenever there is a bubble "pixel" drawn.
	var bubble_mask_piece = new Path.Rectangle(new Point(0,0), new Point(Math.ceil(tube_width * pixel_per_cm), Math.ceil(min_bubble_distance * pixel_per_cm)));
	bubble_mask_piece.fillColor = "#eee";

	var bubble_mask_piece_unrasterized = bubble_mask_piece;
	bubble_mask_piece = bubble_mask_piece.rasterize();
	bubble_mask_piece_unrasterized.remove();

	bubble_mask_piece.remove();
	bubble_mask_symbol = new Symbol(bubble_mask_piece);


	// Set tool accuracy
	tool.maxDistance = min_bubble_distance * pixel_per_cm / 2;
	tool.minDistance = tube_width * pixel_per_cm / 2;
}

function init_bubble_preview_bounds() {
	/*
	// Don't resize, if there are no other dimensions.
	if(view.size.width == $('#bubble_preview').width())
		return false;
	*/
	
	var width = $('#bubble_preview').width();
	var height = Math.ceil(bubble_preview_height * pixel_per_cm);
	var viewSize = new Size(width, height);
	pixel_per_cm = width / (tubes * tube_width);

	if(view.viewSize.equals(viewSize))
		return false;

	view.viewSize = viewSize;
	return true;
}

function setup_bubble_preview() {	
	if (init_bubble_preview_bounds())
	{
		init_bubble_preview_bubbles();
		window.socket.emit('getFrameBuffer');
	}
}

function draw_frame_buffer(frameBuffer) {
	for (var i = 0; i < preview_vertical_bubbles; i++) {
    	for (var j = 0; j < tubes; j++) {
    		if (frameBuffer[i][j])
    			drawPixelAtCoord({x: j, y: i});
    	}
  	}
}

function clear_preview() {
	preview_bubble_mask.removeChildren();
}

function socket_clear_preview() {
	window.socket.emit('clearPreview');
	clear_preview();
}

/* Hooks */

function onResize()
{
	setup_bubble_preview();
}

function brushAtPoint(point) {
	socketDrawPixelAtCoord(calculateCoord(point));
	socketDrawPixelAtCoord(calculateCoord(point + (new Point(tube_width * pixel_per_cm, 0))));
	socketDrawPixelAtCoord(calculateCoord(point - (new Point(tube_width * pixel_per_cm, 0))));
	socketDrawPixelAtCoord(calculateCoord(point + (new Point(0, min_bubble_distance * pixel_per_cm))));
	socketDrawPixelAtCoord(calculateCoord(point - (new Point(0, min_bubble_distance * pixel_per_cm))));
}

function onMouseDown(event) {
	brushAtPoint(event.point);
}

function onMouseDrag(event) {
	brushAtPoint(event.middlePoint);
}

function draw_raster(raster){
	raster.remove();
	raster.fitBounds(view.bounds);

	var offset = Math.floor(insert_picture_offset / min_bubble_distance);
	var pixel_per_cm = raster.width / (tubes * tube_width);

	var image_bubble_height = Math.floor(Math.min(frame_height / min_bubble_distance, raster.height / (pixel_per_cm * min_bubble_distance)));
	
	for (var y = 0; y < image_bubble_height; y++) {
		for (var x = 0; x < tubes; x++) {

			var color_sample = raster.getPixel((1/2 + x) * tube_width * pixel_per_cm, (1/2 + y) * min_bubble_distance * pixel_per_cm);
			
			if (color_sample.brightness > blackiness || color_sample.alpha < blackiness)
				continue;

			socketDrawPixelAtCoord(new Point(x , y + offset));
		}
	}
}

function handle_image(image){
	socket_clear_preview();

	var raster = new Raster(image);

	raster.remove();

	var offset = Math.floor(insert_picture_offset / min_bubble_distance);
	var pixel_per_cm = raster.width / (tubes * tube_width);

	var image_bubble_height = Math.floor(Math.min(frame_height / min_bubble_distance, raster.height / (pixel_per_cm * min_bubble_distance)));
	
	for (var y = 0; y < image_bubble_height; y++) {
		for (var x = 0; x < tubes; x++) {

			var color_sample = raster.getPixel((1/2 + x) * tube_width * pixel_per_cm, (1/2 + y) * min_bubble_distance * pixel_per_cm);
			
			if (color_sample.brightness > blackiness || color_sample.alpha < blackiness)
				continue;

			socketDrawPixelAtCoord(new Point(x , y + offset));
		}
	}
}

function handle_url_image(url){
	socket_clear_preview();

	var raster = null;

	new Raster(url).onLoad(function (){

		raster = this;
		raster.remove();

		var offset = Math.floor(insert_picture_offset / min_bubble_distance);
		var pixel_per_cm = raster.width / (tubes * tube_width);

		var image_bubble_height = Math.floor(Math.min(frame_height / min_bubble_distance, raster.height / (pixel_per_cm * min_bubble_distance)));
		
		for (var y = 0; y < image_bubble_height; y++) {
			for (var x = 0; x < tubes; x++) {

				var color_sample = raster.getPixel((1/2 + x) * tube_width * pixel_per_cm, (1/2 + y) * min_bubble_distance * pixel_per_cm);
				
				if (color_sample.brightness > blackiness || color_sample.alpha < blackiness)
					continue;

				socketDrawPixelAtCoord(new Point(x , y + offset));
			}
		}
	});
}

function draw_text(content){

	var text = new PointText(view.center);
	text.style = {
		fontSize: 100,
		fillColor: 'black',
		justification: 'center'
	};
	// Set the content of the text item:
	text.content = content;

	var raster = text.rasterize();
	text.remove();
	draw_raster(raster);
}

function onFrame(event){
	
}


function onDocumentDrag(event) {
	event.preventDefault();
}

function onDocumentDrop(event) {
	event.preventDefault();

	var file = event.dataTransfer.files[0];
	var reader = new FileReader();

	reader.onload = function ( event ) {
		var image = document.createElement('img');
		image.onload = function () {
			handle_image(image);
		};
		image.src = event.target.result;
	};
	reader.readAsDataURL(file);
}

DomEvent.add(document, {
	drop: onDocumentDrop,
	dragover: onDocumentDrag,
	dragleave: onDocumentDrag
});

window.draw_text = draw_text;
window.clear_preview = clear_preview;
window.socket_clear_preview = socket_clear_preview;
window.draw_pixel = drawPixelAtCoord;
window.draw_frame_buffer = draw_frame_buffer;