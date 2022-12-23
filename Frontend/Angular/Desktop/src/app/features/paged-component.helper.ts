import { Directive, EventEmitter, Injector, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { buildQueryString, deepCopy, obj2QueryString, QueryType, valueHasChanged, wait, writeQueryString } from '@shared/functions';
import { CustomError, PagedList, PageRequest, pageRequestFromQueryString, SMap } from '@shared/models';
import { LoadingService } from '@shared/services';

export interface PageChange {
    pageNumber: number;
    pageSize: number;
}

export interface PagedOptions<S extends Object> {
    queryConfig?: SMap<QueryType>;
    def?: S;
    filterParams?: string[];
    pager: PageRequest;
    queryString?: boolean;
    onResult?: () => void;
}

export interface LookupService<T, S> {
    lookup(route: string, search: S, pageRequest: PageRequest): Promise<PagedList<T> | CustomError>;
}

export interface PagedComponentOptions {
    route?: string;
    defer?: () => Promise<any>;
    cache?: boolean;
    routeChanged?: (route?: string) => any;
}

export class PagedStore {
    static cache: SMap<any> = {};
}

@Directive()
export class PagedComponent<T, S extends Object> implements OnChanges {
    private _initialised: boolean;
    private _loading: LoadingService;

    pagerDefaults: PageRequest = this.pageOptions.pager || new PageRequest();
    pager: PageRequest = this.pageOptions.pager;
    result: PagedList<T>;
    @Output() resultChange = new EventEmitter<PagedList<T>>();

    loaded: boolean = false;
    reloading: boolean = true;
    getting: boolean = true;
    @Output() gettingChange = new EventEmitter<boolean>();

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
            if (componentOptions?.defer) await componentOptions?.defer();

            this.setParams(params);
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

    async nextPage() {
        await this.search({ page: this.pager.page + 1 });
    }

    async prevPage() {
        await this.search({ page: this.pager.page - 1 });
    }

    async changePage(data: PageChange) {
        await this.search({ page: data.pageNumber, pageSize: data.pageSize });
    }

    async reload() {
        await wait(0);

        this.setParams(this.params);
        this.search({ reload: true });
    }

    async search(options: { page?: number; pageSize?: number; reload?: boolean } = { page: 1, pageSize: null, reload: false }) {
        this.pager.page = options.page;

        if (!options.pageSize) options.pageSize = this.pager.pageSize;
        else this.pager.pageSize = options.pageSize;

        // Check for a cache response to avoid unnecessary api calls
        let result: PagedList<T>;
        const qstring = obj2QueryString({ ...this.params, ...this.pager });
        if (options.page > 1 && this.componentOptions?.cache) {
            const cached = PagedStore.cache[qstring];
            if (cached) result = cached;
        }

        // Added filter params to url
        this.writeParams();

        // If we have no result, call api
        if (!result) {
            // Show temporary loader
            if (options.reload) {
                this.reloading = true;
                this._loading.show();
            }

            this.getting = true;
            this.gettingChange.emit(true);

            const resp = await this.lookupService.lookup(this.route, this.params, this.pager);

            this.getting = false;
            this.gettingChange.emit(false);
            this.reloading = false;

            // Stop if response is an exception
            if (resp instanceof CustomError) {
                return;
            }

            result = resp;
            PagedStore.cache[qstring] = resp;
        }

        // Store result
        this.result = result;
        this.resultChange.emit(this.result);

        // Emit changes to child components
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
