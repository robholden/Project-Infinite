import { Component, HostBinding, HostListener, Input as NgInput, OnInit } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ValidatorFn } from '@angular/forms';

import { SMap, Trx } from '@shared/models';
import { AppColour } from '@shared/types';

import { ModalComponent } from '../../modal/modal.component';

export interface DismissAction {
    clearInput(name: string): void;
}

export interface Button {
    text: string | Trx;
    role?: 'cancel' | 'submit';
    colour?: AppColour;
    className?: string;
}

export interface DisplayMessage {
    colour: AppColour;
    text: string;
}

export interface Input {
    name: string;
    type: 'text' | 'textarea' | 'number' | 'email' | 'password' | 'checkbox' | 'radio';
    autocomplete?: string;
    mode?: 'none' | 'text' | 'decimal' | 'numeric' | 'tel' | 'search' | 'email' | 'url';
    placeholder?: string | Trx;
    value?: any | Trx;
    validators?: ValidatorFn[];
    displayMessages?: (value: any) => DisplayMessage[];
    className?: string;
    label?: string | Trx;
    text?: string | Trx;
    errorMap?: SMap<string | Trx>;
    onlyUseErrorMap?: boolean;
}

@Component({
    selector: 'pi-alert-component',
    templateUrl: 'alert.component.html',
    styleUrls: ['alert.component.scss'],
})
export class AlertComponent extends ModalComponent<any> implements OnInit, DismissAction {
    @NgInput() title: string | Trx;
    @NgInput() message: string | Trx;
    @NgInput() buttons: Button[] = [];
    @NgInput() inputs: Input[] = [];
    @NgInput() focusFirst: boolean = false;

    displayMessages: SMap<DisplayMessage[]> = {};
    form: FormGroup;

    @HostBinding('class.alert-content') _ = true;

    constructor() {
        super();
    }

    ngOnInit() {
        super.ngOnInit();

        this.inputs = (this.inputs || []).filter((i) => !!i);
        this.form = new FormGroup(
            this.inputs.reduce((acc, curr) => {
                const validators = curr.validators || [];
                if (curr.displayMessages) {
                    const displayFn = (control: AbstractControl) => {
                        this.displayMessages[curr.name] = curr.displayMessages(control.value);
                        return {};
                    };
                    validators.push(displayFn);
                }
                acc[curr.name] = new FormControl(curr.value, validators);

                return acc;
            }, {} as SMap<FormControl<any>>)
        );

        setTimeout(() => {
            const parent = document.getElementById(this.id);
            const hasInputs = this.inputs.length > 0;
            const el = this.findTag(parent, hasInputs ? 'INPUT' : 'BUTTON', this.focusFirst || hasInputs);

            if (el) el.focus();
        }, 0);
    }

    @HostListener('document:keydown.escape')
    cancel() {
        this.result.next(null);
    }

    submit() {
        this.result.next(this.form.value);
    }

    errors(field: string): Trx[] {
        const errorMap = this.form.get(field).errors || {};
        const errorKeys = Object.keys(errorMap);
        if (errorKeys.length === 0) return [];

        // If field is required, only return that error
        if (errorMap['required']) return [new Trx('form_errors.required', null, 'Required')];

        return Object.keys(errorMap).reduce((acc, key) => {
            const inp = this.inputs.find((i) => i.name === field);
            const errorMap = !inp ? {} : inp.errorMap || {};
            const defaultErrorMap = {
                minlength: new Trx('form_errors.minlength'),
                maxlength: new Trx('form_errors.maxlength'),
                email: new Trx('form_errors.email'),
            };

            if (errorMap[key]) acc.push(errorMap[key]);
            else if (!inp.onlyUseErrorMap && defaultErrorMap[key]) acc.push(defaultErrorMap[key]);

            return acc;
        }, []);
    }

    clearInput(name: string) {
        const field = this.form.get(name);
        if (field) field.patchValue('');
    }

    private findTag(startTag: Element, tagName: string, breakOnFirst: boolean): any {
        const nodes = startTag.children;

        let tag = null;
        for (let i = 0; i < nodes.length; i++) {
            if (nodes[i].tagName === tagName) {
                tag = nodes[i];
                if (breakOnFirst) break;
            }

            const t = this.findTag(nodes[i], tagName, breakOnFirst);
            if (t) {
                tag = t;
                if (breakOnFirst) break;
            }
        }

        return tag;
    }
}
