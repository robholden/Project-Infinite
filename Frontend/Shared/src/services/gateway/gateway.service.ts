import { Injectable, isDevMode } from '@angular/core';

import { CustomError } from '@shared/models';

import { HttpApiService } from '../';

interface Health {
    status: string;
}

@Injectable({
    providedIn: 'root',
})
export class GatewayService {
    constructor(private api: HttpApiService) {}

    /**
     * Checks the health of our api gateway
     */
    async healthy(): Promise<boolean> {
        const resp = await this.api.get<Health>('/health', {
            nonApi: true,
            skipRefresh: true,
            toastError: isDevMode(),
        });

        return !(resp instanceof CustomError) && resp.status === 'Healthy';
    }
}
