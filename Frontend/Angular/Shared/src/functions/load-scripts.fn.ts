export interface DynamicScript {
    src: string;
    attrs?: { name: string; value?: string }[];
}

export function loadScripts(...dynamicScripts: DynamicScript[]): Promise<void[]> {
    const scripts = document.getElementsByTagName('script');
    const promises = dynamicScripts.map(
        (script) =>
            new Promise<void>((res) => {
                for (let j = 0; j < scripts.length; ++j) {
                    if (scripts[j].getAttribute('src') != null && scripts[j].getAttribute('src').startsWith(script.src)) {
                        return;
                    }
                }

                const node = document.createElement('script');
                node.src = script.src;
                node.type = 'text/javascript';
                node.async = false;
                node.onload = () => res();
                node.onerror = () => res();
                (script.attrs || []).forEach((attr) => node.setAttribute(attr.name, attr.value));
                document.getElementsByTagName('head')[0].appendChild(node);
            })
    );

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
