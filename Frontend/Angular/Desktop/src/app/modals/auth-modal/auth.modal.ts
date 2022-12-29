import { Component, Injector, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { LoginProvidersService } from '@app/services/login-providers.service';
import { ModalComponent } from '@app/shared/controllers/modal';

import { CustomEvent } from '@shared/enums';
import { CustomError, Provider } from '@shared/models';
import { EventService } from '@shared/services';
import { AuthService } from '@shared/services/identity';

@Component({
    selector: 'pi-auth',
    templateUrl: './auth.modal.html',
    styleUrls: ['./auth.modal.scss'],
})
export class AuthModal extends ModalComponent<boolean> implements OnInit {
    @Input() type: 'login' | 'register' | 'two-factor';
    @Input() ref: string = location.pathname;

    providers: Provider[] = ['google', 'facebook', 'apple'];
    loading: boolean;

    constructor(
        injector: Injector,
        private router: Router,
        private events: EventService,
        private authService: AuthService,
        private loginProviders: LoginProvidersService
    ) {
        super(injector);
        this.loginProviders.init();
    }

    reload() {
        if (this.ref) {
            this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => this.router.navigate([this.ref]));
        }

        this.result.next(true);
    }

    async loginWithProvider(provider: Provider) {
        this.loading = true;

        // Trigger provider sign in
        const providerResult = await this.loginProviders.login(provider);
        if (!providerResult) {
            this.loading = false;
            return;
        }

        // Verify token with our api
        const resp = await this.authService.loginWithExternalProvider(providerResult.provider, providerResult.user.token, providerResult.user.name);
        this.loading = false;

        if (resp instanceof CustomError) {
            return;
        }

        if (!resp.authToken) await this.events.trigger(CustomEvent.RegisterExternal, { providerResult, user: resp.user });
        else this.reload();
    }

    ngOnInit() {}
}
