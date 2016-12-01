$(document).ready(function () {
    var fewSeconds = 5;
    $('#startQuizButton').mouseup(function () {
        $('#StartQuizForm').submit();
        var btn = $(this);
        btn.prop('disabled', true);
        setTimeout(function () {
            btn.prop('disabled', false);
        }, fewSeconds * 1000);
    });
});