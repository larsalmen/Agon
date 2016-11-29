var refreshIntervalId;

$(document).ready(function () {
    var pin = $("#actuallyStartQuiz").val();
    var pollPlayerCount = function () {
        $.ajax({
            url: '/quiz/CheckConnectedPlayers/' + pin,
            type: 'GET',
            success: function (value) {
                $("#clients").html(value.connectedPlayers);
            }
        });
    };

    var interval = 1000 * 5;

    refreshIntervalId = setInterval(pollPlayerCount, interval);
});