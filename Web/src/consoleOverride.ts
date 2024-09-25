type ConsoleLog = {
    type: 'log' | 'info' | 'warn' | 'error';
    args: any[];
    date: Date;
    page: string;
};

export const logs: ConsoleLog[] = [];

export function overrideConsole() {
    // define a new console
    const console = (function (oldCons: any) {
        return {
            log: function (...params: any[]) {
                oldCons.log(...params);
                logs.push({
                    type: 'log',
                    args: params,
                    date: new Date(),
                    page: window.location.href,
                });
            },
            info: function (...params: any[]) {
                oldCons.info(...params);
                logs.push({
                    type: 'info',
                    args: params,
                    date: new Date(),
                    page: window.location.href,
                });
            },
            warn: function (...params: any[]) {
                oldCons.warn(...params);
                logs.push({
                    type: 'warn',
                    args: params,
                    date: new Date(),
                    page: window.location.href,
                });
            },
            error: function (...params: any[]) {
                oldCons.error(...params);
                logs.push({
                    type: 'error',
                    args: params,
                    date: new Date(),
                    page: window.location.href,
                });
            },
        };
    })(window.console);

    //Then redefine the old console
    window.console = console as any;
}
