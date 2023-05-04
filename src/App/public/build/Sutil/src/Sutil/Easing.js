
export function linear() {
    return (x) => x;
}

export function backInOut(t) {
    const s = 1.70158 * 1.525;
    if (t < 0.5) {
        const tin = t * 2;
        return 0.5 * ((tin * tin) * (((s + 1) * tin) - s));
    }
    else {
        const tout = t - 1;
        return 0.5 * (((tout * tout) * (((s + 1) * tout) + s)) + 2);
    }
}

export function backIn(t) {
    const s = 1.70158;
    return (t * t) * (((s + 1) * t) - s);
}

export function backOut(t) {
    const s = 1.70158;
    const t$0027 = t - 1;
    return ((t$0027 * t$0027) * (((s + 1) * t$0027) + s)) + 1;
}

export function cubicIn(t) {
    return (t * t) * t;
}

export function cubicOut(t) {
    const f = t - 1;
    return ((f * f) * f) + 1;
}

export function cubicInOut(t) {
    if (t < 0.5) {
        return ((4 * t) * t) * t;
    }
    else {
        return (0.5 * Math.pow((2 * t) - 2, 3)) + 1;
    }
}

export function quadInOut(t) {
    const tin = t / 0.5;
    if (tin < 1) {
        return (0.5 * tin) * tin;
    }
    else {
        const tout = tin - 1;
        return -0.5 * ((tout * (tout - 2)) - 1);
    }
}

export function quadIn(t) {
    return t * t;
}

export function quadOut(t) {
    return (-t) * (t - 2);
}

export function quartIn(t) {
    return Math.pow(t, 4);
}

export function quartOut(t) {
    return (Math.pow(t - 1, 3) * (1 - t)) + 1;
}

export function quartInOut(t) {
    if (t < 0.5) {
        return (((8 * t) * t) * t) * t;
    }
    else {
        return (-8 * Math.pow(t - 1, 4)) + 1;
    }
}

export function elasticIn(t) {
    return Math.sin(((13 * t) * 3.141592653589793) / 2) * Math.pow(2, 10 * (t - 1));
}

export function elasticOut(t) {
    return (Math.sin(((-13 * (t + 1)) * 3.141592653589793) / 2) * Math.pow(2, -10 * t)) + 1;
}

export function quintIn(t) {
    return (((t * t) * t) * t) * t;
}

export function quintOut(t) {
    const t$0027 = t - 1;
    return ((((t$0027 * t$0027) * t$0027) * t$0027) * t$0027) + 1;
}

export function expoInOut(t) {
    if ((t === 0) ? true : (t === 1)) {
        return t;
    }
    else if (t < 0.5) {
        return 0.5 * Math.pow(2, (20 * t) - 10);
    }
    else {
        return (-0.5 * Math.pow(2, 10 - (t * 20))) + 1;
    }
}

export function expoIn(t) {
    if (t === 0) {
        return t;
    }
    else {
        return Math.pow(2, 10 * (t - 1));
    }
}

export function expoOut(t) {
    if (t === 1) {
        return t;
    }
    else {
        return 1 - Math.pow(2, -10 * t);
    }
}

