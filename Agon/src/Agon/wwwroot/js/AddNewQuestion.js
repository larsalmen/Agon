var id = 0;
$(document).ready(function () {
    document.getElementById('addField').addEventListener('click', function (e) {
        e.preventDefault();
        id = parseInt(document.getElementById('addField').getAttribute('name'));
        var string = '<div id="input-group-' + parseInt(id) + '" class="input-group" style="margin-left:auto;margin-right:auto;min-width:200px;max-width:400px;padding-bottom:20px">';
        string = string + '<span class="input-group-addon">';
        string = string + '<button type="button" aria-label="..."name="' + parseInt(id) + '" id="' + parseInt(id) + '" class="removeRow btn btn-danger" value="Remove"><span class="glyphicon glyphicon-trash" style="pointer-events:none;"></span>&nbspRemove</button>';
        string = string + '</span>';
        string = string + '<input type="text" class="form-control" name="item.Text" id="Question-' + parseInt(id) + '">';
        string = string + '<input type="text" class="form-control" name="item.CorrectAnswer" id="Answer-' + parseInt(id) + '">';
        string = string + '</div>';
        string = string + '<div id="divCounter-' + parseInt(id) + '" style="display: none;"></div>';
        var newInput = string;
        $('div#divCounter-' + (id - 1)).after(newInput);
        id = parseInt(1 + id);
        document.getElementById('addField').setAttribute('name', id);

    },
        false);
});

$(document).ready(function () {
    $("#formDiv").click(function (e) {
        var target = e.target;
        if (target !== null && target.classList.contains('removeRow')) {
            $('#input-group-' + target.id).remove();
            id = id - 1;
        }

    });
});