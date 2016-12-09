$(document).ready(function () {
    $('.showbutton').click(function () {
        var counter = this.name;
        $('#song' + counter).toggle(1000);
    });
});