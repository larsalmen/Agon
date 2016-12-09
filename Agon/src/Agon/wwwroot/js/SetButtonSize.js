$(document).ready(function () {
    var trackButtons = document.getElementsByClassName("btn-default");
    var widestWidth = 0;
    for (var i = 0; i < trackButtons.length; i++) {
        var currentWidth = $(trackButtons[i]).width();
        if (currentWidth > widestWidth) {
            widestWidth = currentWidth;
        }
        //alert("Den här: " + currentWidth + "Den största: " + widestWidth);
    }
    //sätt alla knappar till den störstas storlek
    for (i = 0; i < trackButtons.length; i++) {
        trackButtons[i].style.width = (widestWidth*1.05)+"px";
    }
});


