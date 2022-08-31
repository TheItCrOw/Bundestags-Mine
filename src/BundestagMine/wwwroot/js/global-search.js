var GlobalSearchHandler = (function () {
    // The default value of how many searchresults we want to show per page.
    GlobalSearchHandler.prototype.defaultTakeSize = 30;
    GlobalSearchHandler.prototype.defaultSpeakersTakeSize = 60;
    GlobalSearchHandler.prototype.defaultAgendaItemTakeSize = 60;
    // We need a way to store running reqeusts to potentially abort them.
    GlobalSearchHandler.prototype.searchingSpeakersRequest = undefined;
    GlobalSearchHandler.prototype.searchingSpeechesRequest = undefined;
    GlobalSearchHandler.prototype.searchingAgendaItemsRequest = undefined;
    GlobalSearchHandler.prototype.searchingPollsRequest = undefined;

    // Constructor
    function GlobalSearchHandler() { }

    // Handles the switching of the tabs to the target
    GlobalSearchHandler.prototype.switchTab = function (target) {
        // Highlight the selected tab
        $('.global-search .tabs .tab').each(function () {
            if ($(this).data('id') == target) {
                $(this).addClass('selected-tab');
            } else {
                $(this).removeClass('selected-tab');
            }
        });

        // Show the selected result
        $('.global-search .results .result').each(function () {
            if ($(this).data('id') == target) {
                $(this).show(100);
            } else {
                $(this).hide();
            }
        })
    }

    // Starts a global search for agenda items, fetches the returned html view and puts it into UI
    GlobalSearchHandler.prototype.globalSearchAgendaItems = async function (obj) {
        // Show loader
        $('.global-search .results').find('.result[data-id="agendaItems"]').find('.loader').fadeIn(100);

        // Set all the other filters to false.
        obj.searchSpeakers = false;
        obj.searchAgendaItems = true;
        obj.searchPolls = false;
        obj.searchSpeeches = false;
        obj.take = this.defaultAgendaItemTakeSize;

        // Do the request
        this.searchingAgendaItemsRequest = postNewGlobalSearch(obj,
            // On success
            function (response) {
                $('.global-search .results').find('.result[data-id="agendaItems"]').find('.result-content').html(response.result);
                this.searchingAgendaItemsRequest = undefined;
                $('.global-search .results').find('.result[data-id="agendaItems"]').find('.loader').fadeOut(100);
            },
            // On error
            function (response) {
                this.searchingAgendaItemsRequest = undefined;
                $('.global-search .results').find('.result[data-id="agendaItems"]').find('.loader').fadeOut(100);
            });
    }

    // Starts a global search for speakers, fetches the returned html view and puts it into UI
    GlobalSearchHandler.prototype.globalSearchSpeakers = async function (obj) {
        // Show loader
        $('.global-search .results').find('.result[data-id="speakers"]').find('.loader').fadeIn(100);

        // Set all the other filters to false.
        obj.searchSpeakers = true;
        obj.searchAgendaItems = false;
        obj.searchPolls = false;
        obj.searchSpeeches = false;
        obj.take = this.defaultSpeakersTakeSize;

        // Do the request
        this.searchingSpeakersRequest = postNewGlobalSearch(obj,
            // On success
            function (response) {
                $('.global-search .results').find('.result[data-id="speakers"]').find('.result-content').html(response.result);
                this.searchingSpeakersRequest = undefined;
                $('.global-search .results').find('.result[data-id="speakers"]').find('.loader').fadeOut(100);
            },
            // On error
            function (response) {
                this.searchingSpeakersRequest = undefined;
                $('.global-search .results').find('.result[data-id="speakers"]').find('.loader').fadeOut(100);
            });
    }

    // Starts a global search for speeches, fetches the returned html view and puts it into UI
    GlobalSearchHandler.prototype.globalSearchSpeeches = async function (obj) {
        // Show loader
        $('.global-search .results').find('.result[data-id="speeches"]').find('.loader').fadeIn(100);

        // Set all the other filters to false.
        obj.searchSpeakers = false;
        obj.searchAgendaItems = false;
        obj.searchPolls = false;
        obj.searchSpeeches = true;

        // Do the request
        this.searchingSpeechesRequest = postNewGlobalSearch(obj,
            // On success
            function (response) {
                $('.global-search .results').find('.result[data-id="speeches"]').find('.result-content').html(response.result);
                searchingSpeechesRequest = undefined;
                $('.global-search .results').find('.result[data-id="speeches"]').find('.loader').fadeOut(100);
            },
            // On error
            function (response) {
                searchingSpeechesRequest = undefined;
                $('.global-search .results').find('.result[data-id="speeches"]').find('.loader').fadeOut(100);
            });
    }

    // Public function: Start a new global search
    GlobalSearchHandler.prototype.startNewGlobalSearch = async function () {
        var searchString = $('.global-search-input').val();
        if (searchString == '') return;

        // Build the request model
        var obj = {
            searchString,
            searchSpeeches: $('.global-search-filter').find('input[data-id="speeches"]').is(':checked'),
            searchSpeakers: $('.global-search-filter').find('input[data-id="speakers"]').is(':checked'),
            searchAgendaItems: $('.global-search-filter').find('input[data-id="agendaItems"]').is(':checked'),
            searchPolls: $('.global-search-filter').find('input[data-id="polls"]').is(':checked'),
            from: $('.global-search-filter').find('input[data-id="from"]').val(),
            to: $('.global-search-filter').find('input[data-id="to"]').val(),
            offset: 0,
            totalCount: -1,
            take: this.defaultTakeSize
        }

        // Foreach included filter, make a request and activate the views
        // Be careful: We have to make copies of the obj we pass in each time, otherwise we overwrite
        // properties. This is probably cause Im writing this in one "function"
        if (obj.searchSpeeches) {
            this.globalSearchSpeeches(jQuery.extend(true, {}, obj));
            $('.global-search .tabs').find('.tab[data-id="speeches"]').show(100);
            this.switchTab('speeches');
        } else {
            $('.global-search .tabs').find('.tab[data-id="speeches"]').hide();
        }
        //speakers
        if (obj.searchSpeakers) {
            this.globalSearchSpeakers(jQuery.extend(true, {}, obj));
            $('.global-search .tabs').find('.tab[data-id="speakers"]').show(100);
            this.switchTab('speakers');

        } else {
            $('.global-search .tabs').find('.tab[data-id="speakers"]').hide();
        }
        //agenda items
        if (obj.searchAgendaItems) {
            this.globalSearchAgendaItems(jQuery.extend(true, {}, obj));
            $('.global-search .tabs').find('.tab[data-id="agendaItems"]').show(100);
            this.switchTab('agendaItems');
        } else {
            $('.global-search .tabs').find('.tab[data-id="agendaItems"]').hide();
        }
        //polls
        if (obj.searchPolls) {
            $('.global-search .tabs').find('.tab[data-id="polls"]').show(100);
            this.switchTab('polls');
        } else {
            $('.global-search .tabs').find('.tab[data-id="polls"]').hide();
        }

        // Set the search result infos
        $('.global-search .search-result-info').find('.search').html(searchString);
        $('.global-search .search-result-info').find('.from').html(obj.from);
        $('.global-search .search-result-info').find('.to').html(obj.to);
    }

    return GlobalSearchHandler;
}());

// Use this item to call all globalsearch logic
var globalSearchHandler = new GlobalSearchHandler();

// Handles the start of the search with the search button
$('body').on('click', '.global-search-start-btn', function () { globalSearchHandler.startNewGlobalSearch(); })

// Handles the start of the with the enter button
$('body').on('keypress', '.global-search-input', function (e) {
    if (e.which == 13) {
        globalSearchHandler.startNewGlobalSearch();
    }
})

// Handles the switching of the pages in the result views
$('body').on('click', '.global-search .result-content .all-result-pages .switch-page-btn', function () {
    var type = $(this).data('id');
    var offset = parseInt($(this).html() - 1); // 1, 2, 3 needs to be 0,1,2

    var obj = {
        searchString: $('.global-search .search-result-info').find('.search').html(),
        from: $('.global-search .search-result-info').find('.from').html(),
        to: $('.global-search .search-result-info').find('.to').html(),
        offset,
        totalCount: $(this).data('total'),
        take: globalSearchHandler.defaultTakeSize
    }
    console.log(obj);
    if (type == 'speeches') {
        globalSearchHandler.globalSearchSpeeches(obj);
    } else if (type == 'speakers') {
        globalSearchHandler.globalSearchSpeakers(obj);
    } else if (type == 'agendaItems') {
        globalSearchHandler.globalSearchAgendaItems(obj);
    }
})

// Handles the switching of the tabs
$('body').on('click', '.global-search .tabs .tab', function () { globalSearchHandler.switchTab($(this).data('id')); })

