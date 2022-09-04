var SpeakerInspectorHandler = (function () {

    // Fetches a new SpeakerInspector view and opens the modal
    SpeakerInspectorHandler.prototype.openSpeakerInspector = async function (speakerId) {
        // Fetch the rendered view
        var modalContent = await getSpeakerInspectorView(speakerId);
        // Fill the modal
        $('#speakerInspectorModal .view').html(modalContent);
        // Open the modal.
        $('#speakerInspectorModal').modal('show');
    }
})

// The object that holds the logic for the speaker inspector
var speakerInspectorHandler = new SpeakerInspectorHandler();

// Testing
speakerInspectorHandler.openSpeakerInspector('11003638');

// Handles the switching of the tabs
$('#speakerInspectorModal').on('click', '.tabs .tab', function () {
    var type = $(this).data('type');

    // Highlight the correct tab
    $('#speakerInspectorModal .tabs .tab').each(function () {
        $(this).removeClass('selected-tab');
    })
    $(this).addClass('selected-tab');

    // Show the right content
    $('#speakerInspectorModal .tab-content .content').each(function () {
        if ($(this).data('type') == type) {
            $(this).show(100);
        } else {
            $(this).hide();
        }
    })
})