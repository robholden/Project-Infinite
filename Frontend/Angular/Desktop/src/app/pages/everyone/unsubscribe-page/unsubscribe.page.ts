import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { CustomError, Trx } from '@shared/models';
import { UserService } from '@shared/services/identity';
import { AuthState } from '@shared/storage';

@Component({
    selector: 'pi-unsubscribe',
    templateUrl: './unsubscribe.page.html',
    styleUrls: ['./unsubscribe.page.scss'],
})
export class UnsubscribePage implements OnInit {
    valid: boolean = null;
    error: Trx;

    constructor(private activatedRoute: ActivatedRoute, private userService: UserService, private authState: AuthState) {
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
        const resp = await this.userService.unsubscribe(key);

        // Stop if response is an exception
        if (resp instanceof CustomError) {
            this.error = resp;
            this.valid = false;
            return;
        }

        this.authState.update('user', (user) => ({ ...user, preferences: { ...user.preferences, marketingEmails: false } }));

        this.valid = true;
    }
}
