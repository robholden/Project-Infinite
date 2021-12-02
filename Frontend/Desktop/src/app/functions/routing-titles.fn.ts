import { Meta, Title } from '@angular/platform-browser';
import { NavigationEnd, Router } from '@angular/router';

import { uniqueArray } from '@shared/functions';

export const LoadingTitlePlaceholder = 'Loading...';

export function replaceLoadingTitle(title: Title, text: string) {
    title.setTitle(title.getTitle().replace(LoadingTitlePlaceholder, text));
}

export function routingTitles(title: Title, router: Router, meta: Meta) {
    let trackingUrl = '';
    router.events.subscribe((event) => {
        if (event instanceof NavigationEnd) {
            // Compare urls and only change when different
            let url = event.url;
            const qi = url.indexOf('?');
            if (qi >= 0) url = url.split(/[?#]/)[0];

            if (url === trackingUrl) return;
            trackingUrl = url;

            // Get title from route data
            title.setTitle(`${routeData(router, 'title', ' / ', true)} / Snow Capture`);

            // Update meta tags
            const desc = routeData(router, 'description') || 'Snow Capture';
            const keywords = routeData(router, 'keywords', ', ');

            meta.updateTag({ name: 'description', content: desc });
            meta.updateTag({ name: 'keywords', content: `${keywords}` });

            window.scrollTo(0, 0);
        }
    });
}

function routeData(router: Router, prop: string, separator?: string, reverse?: boolean) {
    let data = getRouteData(router, prop);
    if (reverse) data = data.reverse();

    return data.join(separator || ' | ');
}

function getRouteData(router: Router, prop: string, parent: any = '') {
    const data = [];
    const state: any = router.routerState;

    if (parent === '') {
        parent = state.root;
    }

    if (parent && parent.snapshot.data && parent.snapshot.data[prop]) {
        data.push(parent.snapshot.data[prop]);
    }

    if (state && parent) {
        data.push(...getRouteData(router, prop, state.firstChild(parent)));
    }

    return uniqueArray(data);
}
