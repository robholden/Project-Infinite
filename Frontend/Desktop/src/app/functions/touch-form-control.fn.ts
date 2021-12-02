import { FormGroup } from '@angular/forms';

export function TouchFormControls(form: FormGroup) {
    if (!form) return;

    Object.keys(form.controls).forEach((field) => {
        const control = form.get(field);
        control.markAsTouched({ onlySelf: true });
        control.updateValueAndValidity();
    });
}
