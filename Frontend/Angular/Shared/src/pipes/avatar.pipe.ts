import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
    name: 'avatar',
    pure: false,
})
export class AvatarPipe implements PipeTransform {
    constructor() {}

    transform(name: string): string {
        return `https://icotar.com/avatar/${name}`;
    }
}
