$(document).ready(function () {
    $(".showbutton").click(function (e) {
        e.preventDefault();
        var target = e.target;
        var counter = target.name;
        $('#songPlaceholder' + counter).hide();
        $('#song' + counter).show();
    });
});