import { Injectable } from '@angular/core';

import { obj2QueryString } from '@shared/functions';
import { CustomError, PagedList, PageRequest, Report, ReportedAction, ReportSearch } from '@shared/models';

import { HttpApiService } from '../http-api.service';

@Injectable({
    providedIn: 'root',
})
export class ReportService<T extends Report<any>> {
    constructor(private api: HttpApiService) {}

    /**
     * Actions a user report
     *
     * @param id the report id
     * @param action the action to perform
     * @param notes any notes about the report
     * @param sendEmail whether to email the user
     */
    async actionUser(id: string, action: ReportedAction, notes: string, sendEmail: boolean = false): Promise<void | CustomError> {
        return await this.api.post(`/reports/report/user/${id}/action`, { action, notes, sendEmail });
    }

    /**
     * Actions a picture report
     *
     * @param id the report id
     * @param action the action to perform
     * @param notes any notes about the report
     * @param sendEmail whether to email the user
     */
    async actionPicture(id: string, action: ReportedAction, notes: string, sendEmail: boolean = false): Promise<void | CustomError> {
        return await this.api.post(`/reports/report/picture/${id}/action`, { action, notes, sendEmail });
    }

    /**
     * Searches reports by given parameters
     *
     * @param route The api route to use for the endpoint
     * @param search The search criteria
     * @param pageRequest Page options
     */
    async lookup(route: string, search: ReportSearch, pageRequest: PageRequest): Promise<PagedList<T> | CustomError> {
        return await this.api.get(`/reports/report${route || ''}?` + obj2QueryString(search, pageRequest), {
            toastError: false,
        });
    }
}
