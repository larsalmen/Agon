$(document).ready(function () {
    var id = $("#quizID").val();
    var pollSubmits = function () {
        $.ajax({
            url: '/quiz/CheckNumberOfSubmits/' + id,
            type: 'GET',
            success: function (value) {
                $("#submits").html(value.noOfSubmits);
            }
        });

        $.ajax({
            url: '/quiz/GetUsernamesOfSubmits/' + id,
            type: 'GET',
            success: function (value) {
                $("#submitterNames").html(value.submits);
            }
        });
    };

    var interval = 1000 * 5;

   setInterval(pollSubmits, interval);
});