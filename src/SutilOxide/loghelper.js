

export function trapConsoleLog( f ) {
    const _log = window.console.log
    window.console.log = function() {
        _log.apply(window.console, arguments)
        f(arguments);
    };
}