// author Kevin Bönisch
const baseURL = 'http://localhost:4567'

// Cleans requestss parameters for potential problems
function cleanParameter(s) {
    return s.replace('/', '{SLASH}'); // This can destroy the url cause of the slash
}

// Gets all metadata for homescreen
async function getHomescreenData() {
    try {
        const result = await $.ajax({
            url: "/api/DashboardController/GetHomescreenData",
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all speakers
async function getSpeakers(limit = '', from = '', to = '', fraction = '', party = '') {
    try {
        var param = cleanParameter(limit + ',' + from + ',' + to + ',' + fraction + ',' + party);
        const result = await $.ajax({
            url: "/api/DashboardController/GetSpeaker/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets a speaker by id
async function getSpeakerById(speakerId) {
    try {
        const result = await $.ajax({
            url: "/api/DashboardController/GetSpeakerById/" + speakerId,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets the portrait of a speaker
async function getSpeakerPortrait(speakerId) {
    try {
        const result = await $.ajax({
            url: "/api/DashboardController/GetDeputyPortrait/" + speakerId,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
    return "https://picsum.photos/536/354";
    var img = baseURL + '/speakerportait?speakerid=' + speakerId;
    return img;
}

// Gets all fractions
async function getFractions() {
    try {
        const result = await $.ajax({
            url: "/api/DashboardController/GetFractions",
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all parties
async function getParties() {
    try {
        const result = await $.ajax({
            url: "/api/DashboardController/GetParties",
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all protocols
async function getProtocols() {
    try {
        const result = await $.ajax({
            url: "/api/DashboardController/GetProtocols",
            type: "GET",
            crossDomain: true,
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all polls of a protocol
async function getBundestagUrlOfPoll(pollId) {
    try {
        const result = await $.ajax({
            url: "/api/DashboardController/GetBundestagUrlOfPoll/" + pollId,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all polls of a protocol
async function getPollsOfProtocol(period, protocolNumber) {
    try {
        var param = cleanParameter(period + ',' + protocolNumber);
        const result = await $.ajax({
            url: "/api/DashboardController/GetPollsOfProtocol/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all angedaitems of an protocol
async function getAgendaItemsOfProtocol(protocolId) {
    try {
        const result = await $.ajax({
            url: "/api/DashboardController/GetAgendaItemsOfProtocol/" + protocolId,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all speeches of an agendaItem
async function getSpeechesOfAgendaItem(period, protocol, number) {
    try {
        var param = cleanParameter(period + ',' + protocol + ',' + number + ',');
        const result = await $.ajax({
            url: "/api/DashboardController/GetSpeechesOfAgendaItem/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets one NLP speech by id
async function getNLPSpeechById(id) {
    try {
        const result = await $.ajax({
            url: "/api/DashboardController/GetNLPSpeechById/" + id,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

//============================================== Networks ===============================================
async function getCommentNetworkData() {
    // Else fetch it from api
    try {
        const result = await $.ajax({
            url: "/api/DashboardController/GetCommentNetworkData",
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

//============================================== Networks end ===============================================

//============================================== Bigger Charts ===============================================
async function getTopicMapChartData(year) {
    // Else fetch it from api
    try {
        const result = await $.ajax({
            url: "/api/DashboardController/GetTopicMapChartData/" + year,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

async function getTopicBarRaceChartData() {
    // Else fetch it from api
    try {
        const result = await $.ajax({
            url: "/api/DashboardController/GetTopicBarRaceChartData/",
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

//============================================== Networks end ===============================================


//============================================== Tokens ===============================================
// Gets all Tokens
async function getTokens(minimum, from, to) {
    try {
        var param = cleanParameter(minimum + ',' + from + ',' + to + ',' + ',' + ',');
        const result = await $.ajax({
            url: "/api/DashboardController/GetTokens/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all Tokens of a fraction
async function getTokensOfFraction(minimum, fraction, from, to) {
    try {
        var param = cleanParameter(minimum + ',' + from + ',' + to + ',' + fraction + ',' + ',');
        const result = await $.ajax({
            url: "/api/DashboardController/GetTokens/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all Tokens of a speaker
async function getTokensOfSpeaker(minimum, speakerId, from, to) {
    try {
        var param = cleanParameter(minimum + ',' + from + ',' + to + ',' + ',' + ',' + speakerId);
        const result = await $.ajax({
            url: "/api/DashboardController/GetTokens/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all Tokens of a party
async function getTokensOfParty(minimum, party, from, to) {
    try {
        var param = cleanParameter(minimum + ',' + from + ',' + to + ',' + ',' + party + ',');
        const result = await $.ajax({
            url: "/api/DashboardController/GetTokens/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

//============================================== Tokens End ===============================================

//============================================== POS ===============================================
// Gets all POS
async function getPOS(minimum, from, to) {
    try {
        var param = cleanParameter(minimum + ',' + from + ',' + to + ',' + ',' + ',');
        const result = await $.ajax({
            url: "/api/DashboardController/GetPOS/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all POS of a fraction
async function getPOSOfFraction(minimum, fraction, from, to) {
    try {
        var param = cleanParameter(minimum + ',' + from + ',' + to + ',' + fraction + ',' + ',');
        const result = await $.ajax({
            url: "/api/DashboardController/GetPOS/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all POS of a speaker
async function getPOSOfSpeaker(minimum, speakerId, from, to) {
    try {
        var param = cleanParameter(minimum + ',' + from + ',' + to + ',' + ',' + ',' + speakerId);
        const result = await $.ajax({
            url: "/api/DashboardController/GetPOS/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all POS of a party
async function getPOSOfParty(minimum, party, from, to) {
    try {
        var param = cleanParameter(minimum + ',' + from + ',' + to + ',' + ',' + party + ',');
        const result = await $.ajax({
            url: "/api/DashboardController/GetPOS/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

//============================================== POS End ===============================================

//============================================== Sentiment ===============================================
// Gets all Sentiments
async function getSentiment(from, to) {
    try {
        var param = cleanParameter(from + ',' + to + ',' + ',' + ',');
        const result = await $.ajax({
            url: "/api/DashboardController/GetSentiments/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all Sentiments of a fraction
async function getSentimentOfFraction(fraction, from, to) {
    try {
        var param = cleanParameter(from + ',' + to + ',' + fraction + ',' + ',');
        const result = await $.ajax({
            url: "/api/DashboardController/GetSentiments/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all Sentiments of a speaker
async function getSentimentOfSpeaker(speakerId, from, to) {
    try {
        var param = cleanParameter(from + ',' + to + ',' + ',' + ',' + speakerId);
        const result = await $.ajax({
            url: "/api/DashboardController/GetSentiments/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all Sentiments of a party
async function getSentimentOfParty(party, from, to) {
    try {
        var param = cleanParameter(from + ',' + to + ',' + ',' + party + ',');
        const result = await $.ajax({
            url: "/api/DashboardController/GetSentiments/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all Sentiments of a party
async function getSentimentOfSpeakerNamedEntity(speakerId, namedEntity, from, to) {
    try {
        var param = cleanParameter(from + ',' + to + ',' + speakerId + ',' + namedEntity);
        const result = await $.ajax({
            url: "/api/DashboardController/GetSpeakerSentimentsAboutNamedEntity/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

//============================================== Sentiments End ===============================================

//============================================== Named Entity ===============================================
// Gets all NamedEntity
async function getNamedEntity(minimum, from, to) {
    try {
        var param = cleanParameter(minimum + ',' + from + ',' + to + ',' + ',' + ',');
        const result = await $.ajax({
            url: "/api/DashboardController/GetNamedEntitites/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all Named Entityt of a fraction
async function getNamedEntityOfFraction(minimum, fraction, from, to) {
    try {
        var param = cleanParameter(minimum + ',' + from + ',' + to + ',' + fraction + ',' + ',');
        const result = await $.ajax({
            url: "/api/DashboardController/GetNamedEntitites/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all Named Entity of a speaker
async function getNamedEntityOfSpeaker(minimum, speakerId, from, to) {
    try {
        var param = cleanParameter(minimum + ',' + from + ',' + to + ',' + ',' + ',' + speakerId);
        const result = await $.ajax({
            url: "/api/DashboardController/GetNamedEntitites/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all Named Entity of a party
async function getNamedEntityOfParty(minimum, party, from, to) {
    try {
        var param = cleanParameter(minimum + ',' + from + ',' + to + ',' + ',' + party + ',');
        const result = await $.ajax({
            url: "/api/DashboardController/GetNamedEntitites/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all Named Entity of a party
async function getNamedEntitiesWithSearchString(search) {
    try {
        var param = cleanParameter(search);
        const result = await $.ajax({
            url: "/api/DashboardController/SearchNamedEntities/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

// Gets all Named Entity with their sentiment value.
async function getNamedEntityWithSentimentView(searchTerm, minimum, from, to, fraction, party, speakerId) {
    try {
        var param = cleanParameter(searchTerm + ',' + minimum + ',' + from + ',' + to + ',' + fraction + ',' + party + ',' + speakerId);
        const result = await $.ajax({
            url: "/api/DashboardController/GetNamedEntititesWithSentimentView/" + param,
            type: "GET",
            dataType: "json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}

//============================================== NE End ===============================================

// ============================================== Topic analysis ======================================
// Gets all Named Entity with their sentiment value.
async function postNewTopicAnalysis(obj) {
    try {
        const result = await $.ajax({
            url: "/api/DashboardController/PostNewTopicAnalysis/",
            type: "POST",
            data: JSON.stringify(obj),
            dataType: "json",
            contentType: "application/json",
            accepts: {
                text: "application/json"
            },
        });
        return result.result;
    } catch (error) {
        console.error(error);
    }
}