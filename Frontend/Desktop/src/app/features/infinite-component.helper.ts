import { Directive, EventEmitter, Injector, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { buildQueryString, deepCopy, obj2QueryString, valueHasChanged, wait, writeQueryString } from '@shared/functions';
import { CustomError, PagedInfo, PagedList, PageRequest, pageRequestFromQueryString, SMap } from '@shared/models';
import { LoadingService } from '@shared/services';

import { LookupService, PagedComponentOptions, PagedOptions, PagedStore } from './paged-component.helper';

@Directive()
export class InfiniteComponent<T, S extends Object> implements OnChanges {
    private _initialised: boolean;
    private _loading: LoadingService;

    pagerDefaults: PageRequest = this.pageOptions.pager || new PageRequest();
    pager: PageRequest = this.pageOptions.pager;
    result: PagedList<T>;
    pagedInfo: PagedInfo;
    @Output() resultChange = new EventEmitter<PagedList<T>>();

    loaded: boolean = false;
    getting = {
        init: true,
        prev: false,
        next: false,
        reload: false,
    };

    pages: number[] = [];

    @Input() route: string;
    @Input() displayParams: {};
    @Input() ignoreParams: string[] = [];
    @Input() fixedParams: SMap<any> = {};

    params: S;
    filtered: boolean;

    constructor(
        injector: Injector,
        private lookupService: LookupService<T, S>,
        private pageOptions: PagedOptions<S>,
        private componentOptions?: PagedComponentOptions
    ) {
        const ar = injector.get<ActivatedRoute>(ActivatedRoute);
        this._loading = injector.get<LoadingService>(LoadingService);

        this.route = componentOptions?.route || '';
        this.pager = deepCopy(this.pagerDefaults);
        this.params = pageOptions.def;

        let params: S;
        if (pageOptions.queryString !== false) {
            const qparams = ar.snapshot.queryParams;
            params = buildQueryString<S>(qparams, pageOptions.queryConfig || {});
            this.pager = pageRequestFromQueryString(qparams, this.pager, pageOptions.queryConfig);
        }

        // We only have @inputs at OnInit
        setTimeout(async () => {
            this.setParams(params);
            if (componentOptions?.defer) await componentOptions?.defer();
            this.search();
            this._initialised = true;
        }, 0);
    }

    ngOnChanges(changes: SimpleChanges) {
        if (!valueHasChanged(changes, 'route')) return;

        if (this.componentOptions?.routeChanged) {
            this.componentOptions.routeChanged(this.route);
        }

        if (this._initialised) {
            this.reset();
            this.search();
        }
    }

    async reload() {
        await wait(0);
        await this.search({ reload: true });
    }

    async nextPage() {
        await this.search({ next: true });
    }

    async prevPage() {
        await this.search({ prev: true });
    }

    async search(options?: { reload?: boolean; next?: boolean; prev?: boolean; page?: number; pageSize?: number }) {
        options = options || {};
        let page = this.pager.page;
        let pageSize = options.pageSize;

        // Force reset when reloading
        if (options.reload) this.pages = [];
        // Incremement or set page number based on provided options
        else {
            if (options.next) page++;
            else if (options.prev) page--;
            else if (options.page) page = options.page;
        }

        // Stop if the page is out of bounds
        if (page <= 0 || (this.result && page > this.result.totalPages)) return;

        // Do not re-fetch pages we already have
        if (this.pages.includes(page)) {
            this.writeParams();
            return;
        }

        // Store page info
        this.pager.page = page;
        this.pages.push(page);
        this.pages.sort();
        if (!pageSize) pageSize = this.pager.pageSize;

        // Check for a cache response to avoid unnecessary api calls
        let result: PagedList<T>;
        const qstring = obj2QueryString({ ...this.params, ...this.pager });
        if (page > 1 && this.componentOptions?.cache) {
            const cached = PagedStore.cache[qstring];
            if (cached) result = cached;
        }

        // Added filter params to url
        this.writeParams();

        // If we have no result, call api
        if (!result) {
            // Show temporary loader
            if (options.reload) {
                this.getting.reload = true;
                this._loading.show();
            }

            // Set internal loaders
            else {
                if (page === 1) this.getting.init = true;
                else if (options.next) this.getting.next = true;
                else if (options.prev) this.getting.prev = true;
            }

            // Call injected lookup service
            const resp = await this.lookupService.lookup(this.route, this.params, { ...this.pager, page, pageSize });
            this.getting = { init: false, prev: false, next: false, reload: false };
            if (options.reload) this._loading.hide();

            // Stop if response is an exception
            if (resp instanceof CustomError) {
                return;
            }

            // Cache response
            PagedStore.cache[qstring] = resp;
            result = resp;
        }

        // Store result
        if (!this.result || options.reload) this.result = result;
        // Update or append result to existing set
        else {
            this.result.totalPages = result.totalPages;
            this.result.filtered = this.filtered;

            if (options.prev) {
                if (this.pages[0] === this.result.pageNumber) this.result.hasPreviousPage = result.hasPreviousPage;
                this.result.rows.unshift(...result.rows);
            } else if (options.next) {
                if (this.pages[this.pages.length - 1] === this.result.pageNumber) this.result.hasNextPage = result.hasNextPage;
                this.result.rows.push(...result.rows);
            }
        }

        // Emit changes to child components
        this.resultChange.emit(this.result);
        this.pagedInfo = this.result;
        if (this.pageOptions?.onResult) this.pageOptions.onResult();
    }

    sameOrderBy() {
        return this.pager.orderBy === this.pagerDefaults.orderBy;
    }

    removeOrderBy() {
        this.pager.orderDir = this.pagerDefaults.orderDir;
        this.pager.orderBy = this.pagerDefaults.orderBy;
    }

    private reset() {
        this.result = null;
        this.pager.page = 1;
        this.pages = [];
    }

    private setParams(params: S) {
        if (!params) params = this.pageOptions.def;
        if (!params) {
            this.params = null;
            return;
        }

        const fixedKeys = Object.keys(this.fixedParams);
        fixedKeys.forEach((key) => (params[key] = this.fixedParams[key]));

        (this.ignoreParams || []).forEach((key) => delete params[key]);

        this.params = params;
    }

    private writeParams() {
        const pageRequest = {};

        if (this.pager.page > 1) pageRequest['page'] = this.pager.page;
        if (this.pager.pageSize !== this.pagerDefaults.pageSize) pageRequest['pageSize'] = this.pager.pageSize;
        if (this.pager.orderDir !== this.pagerDefaults.orderDir) pageRequest['orderDir'] = this.pager.orderDir;
        if (this.pager.orderBy !== this.pagerDefaults.orderBy) pageRequest['orderBy'] = this.pager.orderBy;

        this.filtered = (this.pageOptions.filterParams || []).some((f) => this.params[f] && !this.fixedParams[f]);

        if (this.pageOptions.queryString !== false) {
            writeQueryString({ ...this.params, ...pageRequest }, Object.keys(this.fixedParams), this.pageOptions.def);
        }
    }
}
