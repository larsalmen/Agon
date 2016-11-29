$(document).ready(function () {
    var pin = $("#actuallyStartQuiz").val();
    var pollPlayerCount = function () {
        $.ajax({
            url: '/quiz/CheckConnectedPlayers/' + pin,
            type: 'GET',
            success: function (value) {
                $("#clients").html(value.connectedPlayers);
                console.log("set connected players to" + value.connectedPlayers);
            }
        })
    };

    var interval = 1000 * 5; // where X is your every X minutes clients

    setInterval(pollPlayerCount, interval);
});