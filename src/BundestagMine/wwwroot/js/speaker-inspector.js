var SpeakerInspectorHandler = (function () {

    // Fetches a new SpeakerInspector view and opens the modal
    SpeakerInspectorHandler.prototype.openSpeakerInspector = async function (speakerId) {
        try {
            // Open the modal.
            $('#speakerInspectorModal').modal('show');
            // Show a loading screen
            $('#speakerInspectorModal .loader').fadeIn(150);
            // Fetch the rendered view
            var modalContent = await getSpeakerInspectorView(speakerId);
            // Fill the modal
            $('#speakerInspectorModal .view').html(modalContent);
            // Show speeches
            this.switchTabs('speeches');
        } catch (exception) {
            console.log(exception);
        } finally {
            $('#speakerInspectorModal .loader').fadeOut(150);
        }
    }

    // Handles the full loading of the given entity type such as speeches, polls etc.
    SpeakerInspectorHandler.prototype.loadAll = async function (speakerId, type) {
        // Handle the loading of speeches
        if (type == 'speeches') {
            // Show loader
            $('#speakerInspectorModal .content[data-type="speeches"] .loader').fadeIn(150);
            // Fetch the data
            var allSpeechesView = await getSpeechViewModelListViewOfSpeaker(speakerId);
            $('#speakerInspectorModal .content[data-type="speeches"] .result').html(allSpeechesView);
            // Hide loader
            $('#speakerInspectorModal .content[data-type="speeches"] .loader').fadeOut(150);
        }
    }

    // Handles the switching of the tabs and views
    SpeakerInspectorHandler.prototype.switchTabs = async function (type) {
        // Highlight the correct tab
        $('#speakerInspectorModal .tabs .tab').each(function () {
            if ($(this).data('type') == type) {
                $(this).addClass('selected-tab');
            } else {
                $(this).removeClass('selected-tab');
            }
        })

        // Show the right content
        $('#speakerInspectorModal .tab-content .content').each(function () {
            if ($(this).data('type') == type) {
                $(this).show(100);
            } else {
                $(this).hide();
            }
        })
    }
})

// The object that holds the logic for the speaker inspector
var speakerInspectorHandler = new SpeakerInspectorHandler();

// Testing
speakerInspectorHandler.openSpeakerInspector('11003638');

// Handles the switching of the tabs
$('#speakerInspectorModal').on('click', '.tabs .tab', function () {
    var type = $(this).data('type');
    speakerInspectorHandler.switchTabs(type);
})

// Handles the full loading of the entities such as speeches, polls etc.
$('#speakerInspectorModal').on('click', '.tab-content .load-all', function () {
    var type = $(this).data('type');
    var speakerId = $(this).data('id');
    speakerInspectorHandler.loadAll(speakerId, type);
})