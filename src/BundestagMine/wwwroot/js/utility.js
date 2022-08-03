// Author: Kevin

// Creates a unique id
// https://stackoverflow.com/questions/105034/how-to-create-a-guid-uuid
function generateUUID() { // Public Domain/MIT
    var d = new Date().getTime();//Timestamp
    var d2 = ((typeof performance !== 'undefined') && performance.now && (performance.now() * 1000)) || 0;//Time in microseconds since page-load or 0 if unsupported
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16;//random number between 0 and 16
        if (d > 0) {//Use timestamp until depleted
            r = (d + r) % 16 | 0;
            d = Math.floor(d / 16);
        } else {//Use microseconds since page-load if supported
            r = (d2 + r) % 16 | 0;
            d2 = Math.floor(d2 / 16);
        }
        return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
}

// Parses a date into the given format
function parseToGermanDate(input) {
    var dmy = input.split("-");
    if (dmy.length == 1) return dmy[0];
    if (dmy[2].length > 2) dmy[2] = dmy[2].replace("T00:00:00", "");
    return dmy[2] + '.' + dmy[1] + '.' + dmy[0];
}

// Generates a random int
function randomIntFromInterval(min, max) { // min and max included 
    return Math.floor(Math.random() * (max - min + 1) + min)
}

// Delays the function by X milliseconds
function delay(delayInms) {
    return new Promise(resolve => {
        setTimeout(() => {
            resolve(2);
        }, delayInms);
    });
}

// Performs a small page animation
function doPageTransition() {
    var layerClass = '.left-layer';
    var layers = document.querySelectorAll(layerClass);
    for (const layer of layers) {
        layer.classList.toggle("active");
    }
}

// ENcodes a text to utf8
function utf8_from_str(s) {
    for (var i = 0, enc = encodeURIComponent(s), a = []; i < enc.length;) {
        if (enc[i] === '%') {
            a.push(parseInt(enc.substr(i + 1, 2), 16))
            i += 3
        } else {
            a.push(enc.charCodeAt(i++))
        }
    }
    return a
}

function stripHTMLfromString(s) {
    return s.replace(/<[^>]*>?/gm, '');
}

function sanitizeString(s) {
    return s.trim().replaceAll('\\n', ' ').replaceAll('&ndash;', '').replaceAll('&nbsp;', '');
}

String.prototype.insert_at = function (index, string) {
    return this.substr(0, index) + string + this.substr(index);
}

function IsValidImageUrl(url) {
    var image = new Image();
    image.src = url;
    if (image.width == 0) {
        return false;
    }
    return true;
}

// Shows an information toast
function showToast(title, message) {
    $('.toast').find('.title').html(title);
    $('.toast').find('.toast-body').html(message);
    $('.toast').get(0).style.zIndex = 99999;
    $('.toast').toast('show');
}

function countAnimation(obj, start, end, duration) {
    let startTimestamp = null;
    const step = (timestamp) => {
        if (!startTimestamp) startTimestamp = timestamp;
        const progress = Math.min((timestamp - startTimestamp) / duration, 1);
        obj.innerHTML = new Intl.NumberFormat('de-DE', { maximumSignificantDigits: 3 }).format((Math.floor(progress * (end - start) + start)));
        if (progress < 1) {
            window.requestAnimationFrame(step);
        }
    };
    window.requestAnimationFrame(step);
}

// Replaces german umlaute
function replaceUmlaute(str) {
    return str
        .replace(/\u00df/g, 'ss')
        .replace(/\u00e4/g, 'ae')
        .replace(/\u00f6/g, 'oe')
        .replace(/\u00fc/g, 'ue')
        .replace(/\u00c4/g, 'Ae')
        .replace(/\u00d6/g, 'Oe')
        .replace(/\u00dc/g, 'Ue');
}

