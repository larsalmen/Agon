$(document).ready(function () {
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
        $('.isMobile').css('display', 'block');
        $('.isDesktop').css('display', 'none');
    }
});