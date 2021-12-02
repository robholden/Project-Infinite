import { Component } from '@angular/core';
import { wait } from '@shared/functions';

@Component({
    selector: 'sc-testing',
    templateUrl: 'testing.page.html',
    styleUrls: ['testing.page.scss'],
})
export class TestPage {
    show = false;
    array = [];
    sum = 100;
    throttle = 300;
    scrollDistance = 2;
    scrollUpDistance = 5;
    direction = '';

    constructor() {
        setTimeout(async () => {
            await wait(1000);
            this.show = true;
            this.appendItems(0, this.sum);
        });
    }

    addItems(startIndex, endIndex, _method) {
        for (let i = 0; i < this.sum; ++i) {
            this.array[_method]([i, ' ', this.generateWord()].join(''));
        }
    }

    appendItems(startIndex, endIndex) {
        this.addItems(startIndex, endIndex, 'push');
    }

    prependItems(startIndex, endIndex) {
        this.addItems(startIndex, endIndex, 'unshift');
    }

    onScrollDown(ev) {
        console.log('scrolled down!!', ev);

        // add another 20 items
        const start = this.sum;
        this.sum += 20;
        this.appendItems(start, this.sum);

        this.direction = 'down';
    }

    onUp(ev) {
        console.log('scrolled up!', ev);
        const start = this.sum;
        this.sum += 20;
        this.prependItems(start, this.sum);

        this.direction = 'up';
    }

    generateWord() {
        return 'Word';
    }
}
