export interface PagedInfo {
    hasNextPage: boolean;
    hasPreviousPage: boolean;
    pageNumber: number;
    totalPages: number;
    totalRows: number;
    timestamp: string;
    filtered?: boolean;
}

export interface PagedList<T> extends PagedInfo {
    rows: T[];
}
