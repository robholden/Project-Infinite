import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { CustomError, Trx } from '@shared/models';
import { CommsService } from '@shared/services/comms';
import { AuthState } from '@shared/storage';

@Component({
    selector: 'sc-unsubscribe',
    templateUrl: './unsubscribe.page.html',
    styleUrls: ['./unsubscribe.page.scss'],
})
export class UnsubscribePage implements OnInit {
    valid: boolean = null;
    error: Trx;

    constructor(private activatedRoute: ActivatedRoute, private commsService: CommsService, private authState: AuthState) {
        // If there's no key; prompt for one
        const key = this.activatedRoute.snapshot.params['key'];
        this.unsubscribe(key);
    }

    ngOnInit() {}

    // Unsubscribes the user with a given key
    //
    async unsubscribe(key: string) {
        // Quit if key is null
        if (!key) {
            this.valid = false;
            return;
        }

        // Verify the key with the server
        const resp = await this.commsService.unsubscribe(key);

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            this.error = resp;
            this.valid = false;
            return;
        }

        // Update comms setting state
        if (this.authState.commSettings) {
            this.authState.commSettings = { ...this.authState.commSettings, marketingEmail: false };
        }

        this.valid = true;
    }
}
