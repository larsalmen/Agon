$(document).ready(function () {
    $('#togglespotifyplaybutton').click(function () {
        $('#playbutton').toggle(700);
    });
    $('.showbutton').click(function () {
        var counter = this.name;
        $('#song' + counter).toggle(700);
    });
});