$(function(){

	$('#text_form').submit(function(event){
		event.preventDefault();

		window.socket_clear_preview();
		window.draw_text($('#text_input').val());
		$('#text_input').val('');
	});

	$('#clear_button').click(function(event){
		event.preventDefault();
		window.socket_clear_preview();
	});


	// Socket.IO magic

	window.socket = io.connect();
	window.socket.on('drawPixel', function (data) {
		console.log('test');
		window.draw_pixel({x: data.coord.x, y: data.coord.y});
	});

	window.socket.on('clearPreview', function (data) {
		window.clear_preview();
	});

	window.socket.on('setFrameBuffer', function (data) {
		window.clear_preview();
		window.draw_frame_buffer(data.frameBuffer);
	});
});