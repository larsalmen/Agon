$(document).ready(function () {

    $("#saveQuiz").click(function () {
        $("#saveQuiz").prop('disabled', true);
        var name = $("#quizName").val();
        $("#saveStatus").html("Saving!")
        $.ajax({
            url: '/Quiz/SaveQuiz',
            data: {
                quizName: name
            },
            success: function (response) {
                $("#saveStatus").html("Quiz saved!")
                $("#saveQuiz").prop('disabled', false);
                setTimeout(function () {
                    $("#saveStatus").html("&nbspSave");
                }, 3 * 1000);

            }
        })
    });
});



