var refreshIntervalId;

$(document).ready(function () {
    var pin = $("#actuallyStartQuiz").val();
    var pollPlayer = function () {
        $.ajax({
            url: '/quiz/CheckConnectedPlayers/' + pin,
            type: 'GET',
            success: function (value) {
                $("#clients").html(value.connectedPlayers);
            }
        });

        $.ajax({
            url: '/quiz/GetUsernamesOfConnectedPlayers/' + pin,
            type: 'GET',
            success: function (value) {
                $("#connectedPlayers").html(value.playerNames);
            }
        });
    };

    var interval = 1000 * 5;

    refreshIntervalId = setInterval(pollPlayer, interval);
});