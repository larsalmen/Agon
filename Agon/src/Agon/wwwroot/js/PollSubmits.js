var refreshIntervalId;

$(document).ready(function () {
    var id = $("#quizId").val();
    var pollPlayer = function () {
        $.ajax({
            url: '/quiz/GetUsernamesOfSubmits/' + id,
            type: 'GET',
            success: function (value) {
                $("#respondents").html(value.submits);
            }
        });
    };

    var interval = 1000 * 5;

    refreshIntervalId = setInterval(pollPlayer, interval);
});