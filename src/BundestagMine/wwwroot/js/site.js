// Main js script for the page
// author Kevin Bönisch
var allSpeaker = [];
var allParties = [];
var allFractions = [];
var allProtocols = [];
var allAgendaItems = [];

// set the dimensions and margins of each of the d3 svg graphs
var margin = { top: 10, right: 30, bottom: 30, left: 40 },
    width = window.innerWidth - 120, // 120 from the sidemenu
    height = window.innerHeight;

$(document).ready(async function () {
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

    // some animations
    setTimeout(function () {
        $('.speeches-count').slideDown(500);
    }, 1500);

    // Activate popovers
    $('[data-toggle="popover"]').popover();
    $('.start-loader').fadeOut(500);

    // Title animation
    var tl = new TimelineLite({ delay: 1 }),
        firstBg = document.querySelectorAll('.text__first-bg'),
        secBg = document.querySelectorAll('.text__second-bg'),
        word = document.querySelectorAll('.text__word');
    tl
        .to(firstBg, 0.2, { scaleX: 1 })
        .to(secBg, 0.2, { scaleX: 1 })
        .to(word, 0.1, { opacity: 1 }, "-=0.1")
        .to(firstBg, 0.2, { scaleX: 0 })
        .to(secBg, 0.2, { scaleX: 0 });

    // We cannot hide the faqcontent direclty, because then the masonry bugs.
    // So hide it here when finished loading.
    $('#faqContent').hide();
})

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
})

// Close the navbart items menus when clicking anywhere
$(function () {
    $(document).click(function (event) {
        $('#collapseUtilities').collapse('hide');
    });
});