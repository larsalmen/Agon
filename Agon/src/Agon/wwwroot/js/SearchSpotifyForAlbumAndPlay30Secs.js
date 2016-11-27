var templateSource = document.getElementById('results-template').innerHTML,
    template = Handlebars.compile(templateSource),
    resultsPlaceholder = document.getElementById('results'),
    playingCssClass = 'playing',
    audioObject = null;

var searchTracks = function (query) {
    $.ajax({
        url: 'https://api.spotify.com/v1/search',
        data: {
            q: query,
            type: 'track'
        },
        success: function (response) {
            resultsPlaceholder.innerHTML = template(response);
        }
    });
};


results.addEventListener('click', function (e) {
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
});

document.getElementById('search-form').addEventListener('submit', function (e) {
    e.preventDefault();
    searchTracks(document.getElementById('query').value);
}, false);

//var fetchTracks = function (albumId, callback) {
//    $.ajax({
//        url: 'https://api.spotify.com/v1/albums/' + albumId,
//        success: function (response) {
//            callback(response);
//        }
//    });
//};

//var searchAlbums = function (query) {
//    $.ajax({
//        url: 'https://api.spotify.com/v1/search',
//        data: {
//            q: query,
//            type: 'album'
//        },
//        success: function (response) {
//            resultsPlaceholder.innerHTML = template(response);
//        }
//    });
//};

//results.addEventListener('click', function (e) {
//    var target = e.target;
//    if (target !== null && target.classList.contains('play-button')) {
//        if (target.classList.contains(playingCssClass)) {
//            audioObject.pause();
//        } else {
//            if (audioObject) {
//                audioObject.pause();
//            }
//            fetchTracks(target.getAttribute('data-album-id'), function (data) {
//                audioObject = new Audio(data.tracks.items[0].preview_url);
//                audioObject.play();
//                target.classList.add(playingCssClass);
//                audioObject.addEventListener('ended', function () {
//                    target.classList.remove(playingCssClass);
//                });
//                audioObject.addEventListener('pause', function () {
//                    target.classList.remove(playingCssClass);
//                });
//            });
//        }
//    }
//});
