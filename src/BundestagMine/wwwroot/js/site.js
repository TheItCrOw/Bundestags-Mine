// Main js script for the page
// author Kevin Bönisch
var allSpeaker = [];
var allParties = [];
var allFractions = [];
var allProtocols = [];
var currentView = "homeContent";

// set the dimensions and margins of each of the d3 svg graphs
var margin = { top: 10, right: 30, bottom: 30, left: 40 },
    width = window.innerWidth - 240, // 120 from the sidemenu
    height = window.innerHeight;

// Check if the user is accessing via mobile device
window.mobileCheck = function () {
    let check = false;
    (function (a) {
        if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4))) check = true;
    })(navigator.userAgent || navigator.vendor || window.opera);
    return check;
};

// The entry method of the page
$(document).ready(async function () {

    // If the user has a mobile device, restrict some features since they really aren't made for mobile.
    var isMobile = window.mobileCheck();
    if (isMobile) {
        console.log('Using mobile device, so we restrict some features');

        // Restrict the topic map
        $('#topicMapContent').remove();
        $('.nav-item[data-id="topicMapContent"]').remove();

        // Restrict the topic race
        $('#topicRaceContent').remove();
        $('.nav-item[data-id="topicRaceContent"]').remove();

        // Restrict the comment network
        $('#networkContent').remove();
        $('.nav-item[data-id="networkContent"]').remove();

        // Dont show the speaker inspector button
        $('#speakerInspectorButton').remove();

        // Tell the user.
        showToast('Mobile Device ', 'Wir haben erkannt, dass Sie ein Mobile Device nutzen. Manche Features der Bundestags-Mine sind nicht auf die kleinen Bildschirme von Smartphones ausgelegt, weshalb wir diese ausblenden. Um die komplette Bundestags-Mine zu nutzen, empfehlen wir <b>mindestens</b> ein Tablet.');
    }

    // Setup all relevant data first
    allSpeaker = await getSpeakers();
    $('#startProgress').get(0).style.width = '25%';
    $('#startLoaderStatusMessage').html('Redner(innen) geladen');

    allFractions = await getFractions();
    $('#startProgress').get(0).style.width = '50%';
    $('#startLoaderStatusMessage').html('Fraktionen geladen');

    allParties = await getParties();
    $('#startProgress').get(0).style.width = '75%';
    $('#startLoaderStatusMessage').html('Parteien geladen');

    allProtocols = await getProtocols();
    $('#startProgress').get(0).style.width = '100%';
    $('#startLoaderStatusMessage').html('Protokolle geladen');

    // Init the dashboarc creation search
    search('');

    // Build the protocol tree
    buildProtocolTree(allProtocols);

    // Init the topic analysis
    initTopicAnalysis();

    // Init the download center
    downloadCenterHandler.init(allFractions, allParties, allSpeaker);

    // Init the daily paper
    dailyPaperHandler.init();

    // Hide the navigation menu
    var $navbar = $('.navbar-nav');
    $navbar.hide()

    // Activate popovers
    $('[data-toggle="popover"]').popover();
    $('.start-loader').fadeOut(500);

    // We cannot hide the faqcontent direclty, because then the masonry bugs.
    // So hide it here when finished loading.
    $('#faqContent').hide();
})

// Open the speaker inspector
$('body').on('click', '.open-speaker-inspector', function () { speakerInspectorHandler.openSpeakerInspector($(this).data('id')) })
$('body').on('click', '#speakerInspectorButton', function () { speakerInspectorHandler.openSpeakerInspector('') })

// Adds a new dsahboard to the Parlament radar
async function addNewDashboard(name, fetchId, type, from, to) {
    // Create a uniqe dashboard id
    var dashboardId = generateUUID();

    // Start building the dashboard
    buildNewDashboard(dashboardId, name, fetchId, type, from, to);

    // Add a new ribbon button and highlight it
    var ribbon = document.createElement('div');
    ribbon.setAttribute('data-id', dashboardId);
    ribbon.classList.add('ribbon-item');
    ribbon.innerHTML += `<h5>${name}</h5>`;
    $('.dashboard-ribbon').append(ribbon);

    // Highlight and show the correct ribbon and dashboard
    // ribbon
    $('.ribbon-item').each(function () {
        $(this).removeClass('selected-ribbon');
    })
    $(ribbon).addClass('selected-ribbon');

    // dashboard
    $('.dashboard').each(function () {
        $(this).fadeOut(0);
    })
    $(`#${dashboardId}`).fadeIn(500);

    // Reactive the popovers again for the newly added html
    $('[data-toggle="popover"]').popover();
    // Do a small animation
    window.scrollTo(0, 0);
    $('#speaker-tooltip').hide();
    // We want a small animation
    doPageTransition();

    await delay(500);
}

// Handles the clicking onto the ribbons
$('body').on('click', '.ribbon-item', function () {
    // We have to reset the scrollbar. Otherwise we get weird whitespaces.
    window.scrollTo(0, 0);

    // Highlight the corret rubbon
    $('.ribbon-item').each(function (index, ribbon) {
        $(ribbon).removeClass('selected-ribbon');
    })
    $(this).addClass('selected-ribbon');

    var dashboardId = $(this).data('id');
    // SHow the correct dashboard
    $('.dashboard').each(function () {
        $(this).fadeOut(0);
    })
    $(`#${dashboardId}`).fadeIn(500);
})

// Handles the deleting of a dashboard
$('body').on('click', '.delete-dashboard-btn', function () {
    // remove the dashboard
    var dashboard = $(this).closest('.dashboard');
    var dashboardId = dashboard.attr('id');
    dashboard.remove();
    // remove the ribbon
    $('.ribbon-item').each(function () {
        var ribbon = $(this);
        var id = ribbon.data('id');
        if (id == dashboardId) {
            ribbon.remove();
        }
    })

    $('.ribbon-item').first().addClass('selected-ribbon');
    $('.new-dashboard').first().fadeIn(500);
    $('.popover').popover('hide');
})

// We only want to close the search list of speakers if we click somewhere else but the input
// and the actual list!
$(document).on('click', function (event) {
    if (event.target.classList.contains('exception-click')) return;
    $('#dashboardCreateSearchList').hide();
    $('#downloadCenterContent .filter .speaker-list-div').hide();
})

// Handles the switching of the differen views.
$('body').on('click', '.nav-item-switcher', async function () {
    // We have to reset the scrollbar. Otherwise we get weird whitespaces.
    window.scrollTo(0, 0);
    $('#speaker-tooltip').hide();
    // We want a small animation
    doPageTransition();
    await delay(500);

    $('.nav-item-switcher').each(function () {
        var id = $(this).data('id');
        // Network nav items have sub menus.
        if (!$(this).hasClass('nav-item')) {
            $(this).closest('.nav-item').removeClass('selected-nav-item');
        } else {
            $(this).removeClass('selected-nav-item');
        }
        $(`#${id}`).hide();
    });

    if (!$(this).hasClass('nav-item')) {
        $(this).closest('.nav-item').addClass('selected-nav-item');
    } else {
        $(this).addClass('selected-nav-item');
    }
    $(`#${$(this).data("id")}`).fadeIn(500);
    // soter the current view.
    currentView = $(this).data('id');

    // Set the correct name
    var viewName = $(this).find('span').html();
    $('#currentViewTitle').html(viewName);
})

// Open the helper video box
$('body').on('click', '.view-helper-tooltip', function () {
    if (currentView == 'globalSearchContent') {
        $('.view-helper-video-box iframe').attr('src', 'https://www.youtube.com/embed/5g5UIJLaBzE');
        $('.view-helper-video-box').fadeIn(250);
    } else if (currentView == 'speechContent') {
        $('.view-helper-video-box iframe').attr('src', 'https://www.youtube.com/embed/btkIGegQEdA');
        $('.view-helper-video-box').fadeIn(250);
    } else if (currentView == 'topicAnalysis') {
        $('.view-helper-video-box iframe').attr('src', 'https://www.youtube.com/embed/W78jl_P9rD0');
        $('.view-helper-video-box').fadeIn(250);
    } else if (currentView == 'topicMapContent') {
        $('.view-helper-video-box iframe').attr('src', 'https://www.youtube.com/embed/cNJ_xi8TiLU');
        $('.view-helper-video-box').fadeIn(250);
    } else if (currentView == 'topicRaceContent') {
        $('.view-helper-video-box iframe').attr('src', 'https://www.youtube.com/embed/tfj--n-KZN4');
        $('.view-helper-video-box').fadeIn(250);
    } else if (currentView == 'networkContent') {
        $('.view-helper-video-box iframe').attr('src', 'https://www.youtube.com/embed/p6ebl-WwxzA');
        $('.view-helper-video-box').fadeIn(250);
    } else if (currentView == 'downloadCenterContent') {
        $('.view-helper-video-box iframe').attr('src', 'https://www.youtube.com/embed/UKvZRnxYwmk');
        $('.view-helper-video-box').fadeIn(250);
    }
})

// Close the navbar items menus when clicking anywhere
$(function () {
    $(document).click(function (event) {
        $('#collapseUtilities').collapse('hide');
    });
});

// Close the navigation menu when the dropback is clicked 
$('body').on('click', '.navbar-nav', function () {
    openOrCloseNavigation();
})

// Handles the correct opening and closing of the navigation
function openOrCloseNavigation() {
    var expanded = $('.navbar-nav').data('expanded');
    var $navbar = $('.navbar-nav');
    if (expanded) {
        setTimeout(function () { $navbar.hide(); }, 250)
        $navbar.animate({ left: '-100%' }, 250);
    } else {
        $navbar.show();
        $navbar.animate({ left: 0 }, 250);
    }

    $('.navbar-nav').data('expanded', !expanded);
}