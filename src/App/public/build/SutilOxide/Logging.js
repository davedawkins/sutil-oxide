import { some } from "../fable_modules/fable-library.3.7.20/Option.js";

function jslog(s) {
    console.log(some(s));
}

let loggingHandler = (value) => {
};

export function log(s) {
    loggingHandler(s);
}

export function prependHandler(log_1) {
    const currentHandler = loggingHandler;
    loggingHandler = ((s) => {
        log_1(s);
        currentHandler(s);
    });
}

export function appendHandler(log_1) {
    const currentHandler = loggingHandler;
    loggingHandler = ((s) => {
        currentHandler(s);
        log_1(s);
    });
}

export function setHandler(log_1) {
    loggingHandler = log_1;
}

