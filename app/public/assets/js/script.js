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

	/*
	$('#small_bubble_offset_input').change(function(){ 
		window.socket.emit('changeSmallBubbleOffset', {value: Number($(this).val())});
	});
	*/


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

	window.socket.on('setPreviewCountdown', function (data) {
		$('#countdown .progress-bar').css('width', (data.value * 100) + '%');
	});
});