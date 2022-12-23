import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

import { PageChange } from '@app/features/paged-component.helper';

@Component({
    selector: 'sc-paging',
    templateUrl: './paging.component.html',
    styleUrls: ['./paging.component.scss'],
})
export class PagingComponent implements OnInit {
    pages: number[] = [];
    pageCount: number = 0;
    rowIndex: { start: number; end: number } = { start: 0, end: 0 };
    showEnds: boolean = false;

    @Input() page: number = 1;
    @Input() pageSize: number = 25;
    @Input() pageViewLimit: number = 4;
    @Input() rows: number = 0;
    @Output() pageChange = new EventEmitter<PageChange>();

    constructor() {}

    ngOnInit() {
        this.pageTurn();
    }

    get pageResult() {
        return { start: this.rowIndex.start + 1, end: this.rowIndex.end >= this.rows ? this.rows : this.rowIndex.end + 1, total: this.rows };
    }

    pageTurn(page: number = null, pageSize: number = null) {
        // Work out page restrictions
        if (pageSize === null) pageSize = this.pageSize;
        if (page === null) page = this.page;

        this.pageCount = Math.ceil(this.rows / pageSize);
        page = page > this.pageCount ? this.pageCount : page;

        // If page size <= 0 show all results
        if (pageSize <= 0) {
            this.pages = [];
            this.rowIndex.start = 0;
            this.rowIndex.end = this.rows;
            return;
        }

        const startI = page === 1 ? 0 : (page - 1) * pageSize;
        const endI = startI + pageSize;

        // If there are over n pages limit the amount shown
        const limit = this.pageViewLimit;
        let start = 1;
        let end = 1 + limit * 2;

        // Make sure end is not spilling over
        const pageCount = this.pageCount;
        if (end > pageCount) end = pageCount;

        // Work out in between pages
        if (pageCount > limit * 2) {
            if (page > limit && page <= pageCount - limit) {
                start = page - limit;
                end = page + limit;
            } else if (page > pageCount - limit) {
                start = page - (limit * 2 - (pageCount - page));
                end = pageCount;
            }
        }

        // Store pages
        const pages = [];
        for (let i = start; i <= end; i++) pages.push(i);
        this.pages = pages;
        this.rowIndex.start = startI;
        this.rowIndex.end = endI;
        this.showEnds = pageCount > limit * 2 + 1;

        // Emit event for calling to page data set
        if (page !== this.page || this.pageSize !== pageSize) {
            this.pageChange.emit({ pageNumber: page, pageSize });
            this.page = page;
            this.pageSize = pageSize;
        }
    }
}
