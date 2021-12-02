import { Injectable } from '@angular/core';

import { CustomError, UserStats } from '@shared/models';

import { HttpApiService } from '../';

@Injectable({
    providedIn: 'root',
})
export class StatsService {
    constructor(private api: HttpApiService) {}

    /**
     * Returns content stats for a username
     *
     * @param username The username
     */
    async get(username: string): Promise<UserStats> {
        const stats = await this.api.get<UserStats>('/content/stats/' + username, { toastError: false });
        return stats instanceof CustomError ? new UserStats() : stats;
    }
}
