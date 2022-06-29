import { buildQueryString, pascalToScores, QueryType } from '@shared/functions';
import { SMap } from '@shared/models';

export type OrderByDirection = 'asc' | 'desc';

export class PageRequest {
    public page: number = 1;
    public pageSize: number = 25;

    constructor(public orderBy: string = '', public orderDir: OrderByDirection = 'desc') {
        this.orderBy = pascalToScores(orderBy);
    }

    static fromPageSize(size: number): PageRequest {
        const request = new PageRequest();
        request.pageSize = size;
        return request;
    }
}

export function pageRequestFromQueryString(qparams: SMap<string>, def: PageRequest, config?: SMap<QueryType>) {
    def = def || new PageRequest();

    const pagerConfig: SMap<QueryType> = { pageSize: 'number', page: 'number', orderDir: 'string', orderBy: 'string' };
    if (config)
        Object.keys(pagerConfig)
            .filter((key) => key in config)
            .forEach((key) => (pagerConfig[key] = config[key]));

    const pager = buildQueryString<PageRequest>(qparams, pagerConfig, def);
    if (!pager.orderBy) pager.orderDir = def.orderDir;

    return pager;
}
