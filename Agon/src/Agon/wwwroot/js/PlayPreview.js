var playingCssClass = 'playing',
    audioObject = null;

$(document).ready(function () {
    $("#playListener").click(function (e) {
        //////
        var playIcon = 'glyphicon-play';
        var pauseIcon = 'glyphicon-pause';
        var target = e.target;
        if (target !== null && target.classList.contains('play-button')) {
            if (target.classList.contains(playingCssClass)) {
                audioObject.pause();
                //PAUSE ==> PLAY
                target.children[0].classList.remove(pauseIcon);
                target.children[0].classList.add(playIcon);
            } else {
                if (audioObject) {
                    audioObject.pause();
                }
                //fetchTracks(target.getAttribute('preview-url'), function (data) {
                audioObject = new Audio(target.getAttribute('preview-url'));
                audioObject.play();
                target.classList.add(playingCssClass);
                //PLAY ==PAUSE
                target.children[0].classList.remove(playIcon);
                target.children[0].classList.add(pauseIcon);

                //target.children[0].classList.add(pauseIcon);
                audioObject.addEventListener('ended', function () {
                    target.classList.remove(playingCssClass);
                    //PAUSE ==> PLAY
                    target.children[0].classList.remove(pauseIcon);
                    target.children[0].classList.add(playIcon);
                    //target.children[0].classList.add('glyphicon-play');
                });
                audioObject.addEventListener('pause', function () {
                    target.classList.remove(playingCssClass);
                    //PAUSE ==> PLAY
                    target.children[0].classList.remove(pauseIcon);
                    target.children[0].classList.add(playIcon);
                });
                //});
            }
        }





        /////
    });
});
