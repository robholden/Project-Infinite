export function RefresherAwaitDo(prom: Promise<any>, doCallback: () => void) {
    return new Promise<void>((res) => {
        const startTime = new Date();
        const completed = () => {
            const endTime = new Date();
            const ms = Math.abs(endTime.getTime() - startTime.getTime());

            setTimeout(() => {
                doCallback();
                res();
            }, 500 - ms);
        };

        prom.then(() => completed()).catch(() => completed());
    });
}
