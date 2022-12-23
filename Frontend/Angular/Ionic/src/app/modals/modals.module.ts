import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { ComponentsModule } from '@app/components/components.module';

import { IonicModule } from '@ionic/angular';

import { PipesModule } from '@shared/pipes/pipes.module';

import { LoginModal } from './login-modal/login.modal';
import { PrivacyPolicyModal } from './privacy-policy-modal/privacy-policy.modal';
import { RegisterExternalModal } from './register-external-modal/register-external.modal';
import { RegisterModal } from './register-modal/register.modal';
import { TermsAndConditionsModal } from './terms-and-conditions-modal/terms-and-conditions.modal';
import { TwoFactorModal } from './two-factor-modal/two-factor.modal';

import { MarkdownModule } from 'ngx-markdown';

@NgModule({
    declarations: [LoginModal, RegisterModal, TwoFactorModal, TermsAndConditionsModal, PrivacyPolicyModal, RegisterExternalModal],
    imports: [CommonModule, FormsModule, ReactiveFormsModule, IonicModule, MarkdownModule.forRoot(), PipesModule, ComponentsModule],
    exports: [LoginModal, RegisterModal, TwoFactorModal, TermsAndConditionsModal, PrivacyPolicyModal, RegisterExternalModal],
    entryComponents: [],
})
export class ModalsModule {}
