$(document).ready(function () {

    $(".RemoveQuiz").click(function (e) {
        e.preventDefault();
        var target = e.target;
        $.ajax({
            url: '/Quiz/RemoveQuiz',
            data: {
                id: target.name
            },
            success: function (response) {
                $("#" + target.name).remove();
            }
        });
    });
});