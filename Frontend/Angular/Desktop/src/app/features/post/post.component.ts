import { Component, Input } from '@angular/core';

import { Post } from '@shared/models';

@Component({
    selector: 'pi-post',
    templateUrl: './post.component.html',
    styleUrls: ['./post.component.scss'],
})
export class PostComponent {
    @Input() post: Post;
}
