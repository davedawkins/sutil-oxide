

export function trapConsoleLog( f ) {
    const _log = window.console.log
    window.console.log = function() {
        _log.apply(window.console, arguments)
        f(arguments);
    };
}

function getErrorObject(){
    try { throw Error('') } catch(err) { return err; }
}

export function getCallStack() {
    var err = getErrorObject();
    var caller_line = err.stack.split("\n")[4];
    var index = caller_line.indexOf("at ");
    var clean = caller_line.slice(index+2, caller_line.length);
    return err.stack.split("\n")
}