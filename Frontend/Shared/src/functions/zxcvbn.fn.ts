import { AppColour } from '../types/colour.type';

import * as _zxcvbn from 'zxcvbn';

export class PasswordStrength {
    public constructor(public level: PasswordStrengthLevel, public colour: AppColour, public message: string) {}
}

export enum PasswordStrengthLevel {
    Short,
    Invalid,
    Weak,
    Ok,
    Good,
    Strong,
}

/**
 * Determines a given password its strength
 *
    0 # too guessable: risky password. (guesses < 10^3)
    1 # very guessable: protection from throttled online attacks. (guesses < 10^6)
    2 # somewhat guessable: protection from unthrottled online attacks. (guesses < 10^8)
    3 # safely unguessable: moderate protection from offline slow-hash scenario. (guesses < 10^10)
    4 # very unguessable: strong protection from offline slow-hash scenario. (guesses >= 10^10)
 *
 * @param value The password
 */
export function zxcvbn(value: string): PasswordStrength {
    if (!value) return null;

    const result = _zxcvbn(value);
    switch (result.score) {
        case 0:
        case 1:
            return new PasswordStrength(PasswordStrengthLevel.Weak, 'danger', 'Weak');

        case 2:
            return new PasswordStrength(PasswordStrengthLevel.Ok, 'danger', 'Ok');

        case 3:
            return new PasswordStrength(PasswordStrengthLevel.Good, 'warning', 'Good');

        case 4:
            return new PasswordStrength(PasswordStrengthLevel.Strong, 'success', 'Strong');

        default:
            return null;
    }
}
