// Resize the canvas to prevent a strange bug, where some pixels of body get added to the bottom, so there is always a scroll bar.
$(function(){
	$(window).resize(function(){
		$('#bubble_spool').height($(window).height() - $('header').outerHeight());
	})

	$('#bubble_spool').height($(window).height() - $('header').outerHeight());
});