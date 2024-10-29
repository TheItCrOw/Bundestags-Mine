// https://github.com/brainsum/cookieconsent
window.CookieConsent.init({
    // More link URL on bar.
    modalMainTextMoreLink: null,
    // How long to wait until bar comes up.
    barTimeout: 1000,
    // Look and feel.
    theme: {
        barColor: 'white',
        barTextColor: 'rgba(0,0,0,0.75)',
        barMainButtonColor: 'gold',
        barMainButtonTextColor: 'black',
        modalMainButtonColor: 'gold',
        modalMainButtonTextColor: 'black',
    },
    language: {
        // Current language.
        current: 'de',
        locale: {
            en: {
                barMainText: 'Die Bundestags-Mine nutzt Cookies um Besucherzahlen und Nutzerverhalten zu untersuchen.',
                closeAriaLabel: 'close',
                barLinkSetting: 'Einstellungen',
                barBtnAcceptAll: 'Alle akzeptieren',
                modalMainTitle: 'Cookie Einstellungen',
                modalMainText: 'Cookies sind kleine Datenpakete, die von einer Website gesendet und vom Webbrowser des Nutzers auf dessen Computer gespeichert werden, solange der Nutzer surft. Ihr Browser speichert jede Nachricht in einer kleinen Datei, die Cookie genannt wird. Wenn Sie eine andere Seite vom Server anfordern, sendet Ihr Browser den Cookie wieder an den Server. Cookies wurden als gewissenhafter Mechanismus entwickelt, um sich an Informationen zu erinnern oder die Browsing-Verhalten des Nutzers aufzuzeichnen.',
                modalBtnSave: 'Speichern',
                modalBtnAcceptAll: 'Alle akzeptieren',
                modalAffectedSolutions: 'Betroffene Funktionen',
                learnMore: 'Mehr dazu',
                on: 'An',
                off: 'Aus',
                enabled: 'ist an.',
                disabled: 'ist aus.',
                checked: 'checked',
                unchecked: 'unchecked',
            }
        }
    },
    // List all the categories you want to display.
    categories: {
        // Unique name.
        // This probably will be the default category.
        necessary: {
            // The cookies here are necessary and category can't be turned off.
            // Wanted config value will be ignored.
            needed: true,
            // The cookies in this category will be let trough.
            // This probably should be false if category not necessary.
            wanted: true,
            // If checkbox is on or off at first run.
            checked: true,
            // Language settings for categories.
            language: {
                locale: {
                    en: {
                        name: 'Notwendige Cookies',
                        description: 'Die Bundestags-Mine nutzt eigens erstellte Videos von Youtube als Hilfsanleitungen. Um diese zu "embedden" sind einige Youtube und Google cookies zu akzeptieren. Des Weiteren werden Site-Analytics wie Besucherzahlen, Besuchsdauer und Nutzerverhalten in den Cookies festgehalten und ausgewertet.',
                    }
                }
            }
        }
    },
    // List actual services here.
    services: {
        // Unique name.
        analytics: {
            // Existing category Unique name.
            // This example shows how to block Google Analytics.
            category: 'necessary',
            // Type of blocking to apply here.
            // This depends on the type of script we are trying to block.
            // Can be: dynamic-script, script-tag, wrapped, localcookie.
            type: 'dynamic-script',
            // Only needed if "type: dynamic-script".
            // The filter will look for this keyword in inserted scipt tags
            //  and block if match found.
            search: 'analytics',
            // List of known cookie names or regular expressions matching
            //  cookie names placed by this service.
            // These will be removed from current domain and .domain.
            cookies: [
                {
                    // Known cookie name.
                    name: '_gid',
                    // Expected cookie domain.
                    domain: `.${window.location.hostname}`
                },
                {
                    // Regex matching cookie name.
                    name: /^_ga/,
                    domain: `.${window.location.hostname}`
                }
            ],
            language: {
                locale: {
                    de: {
                        name: 'Google Analytics'
                    }
                }
            }
        }
    }
});