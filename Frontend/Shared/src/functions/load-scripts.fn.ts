export interface DynamicScript {
    src: string;
    attrs?: { name: string; value?: string }[];
}

export function loadScripts(...dynamicScripts: DynamicScript[]): Promise<void[]> {
    const promises = [];

    const scripts = document.getElementsByTagName('script');
    for (let i = 0; i < dynamicScripts.length; i++) {
        const promise = new Promise<void>((res) => {
            let isFound = false;
            for (let j = 0; j < scripts.length; ++j) {
                if (scripts[j].getAttribute('src') != null && scripts[j].getAttribute('src').startsWith(dynamicScripts[i].src)) {
                    isFound = true;
                    break;
                }
            }
            if (isFound) {
                res();
            } else {
                const node = document.createElement('script');
                node.src = dynamicScripts[i].src;
                node.type = 'text/javascript';
                node.async = false;
                node.onload = () => res();
                node.onerror = () => res();
                (dynamicScripts[i].attrs || []).forEach((attr) => node.setAttribute(attr.name, attr.value));
                document.getElementsByTagName('head')[0].appendChild(node);
            }
        });

        promises.push(promise);
    }

    return Promise.all(promises);
}

export function unloadScript(name: string) {
    const scripts = document.getElementsByTagName('script');
    for (let j = 0; j < scripts.length; ++j) {
        if (scripts[j].getAttribute('src') != null && scripts[j].getAttribute('src').startsWith(name)) {
            document.getElementsByTagName('head')[0].removeChild(scripts[j]);
            return;
        }
    }
}
