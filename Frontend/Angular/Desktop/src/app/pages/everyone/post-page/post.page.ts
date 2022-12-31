import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { ActivatedRoute } from '@angular/router';

import { CustomError, Post } from '@shared/models';
import { PostService } from '@shared/services/content';
import { AuthState } from '@shared/storage';

import { fade } from '@app/functions/animations.fn';
import { replaceLoadingTitle } from '@app/functions/routing-titles.fn';

@Component({
    selector: 'pi-post-page',
    templateUrl: './post.page.html',
    styleUrls: ['./post.page.scss'],
    animations: [fade],
})
export class PostPage implements OnInit {
    loading: boolean = true;
    postId: string;
    post: Post;
    author: boolean;
    reported: boolean;

    constructor(private activatedRoute: ActivatedRoute, private title: Title, private postService: PostService, public authState: AuthState) {}

    ngOnInit(): void {
        this.activatedRoute.params.subscribe((params) => {
            const postId: string = params['id'];
            if (this.postId !== postId) {
                this.post = null;
                this.postId = postId;
                this.fetchPost();
            }
        });
    }

    private async fetchPost() {
        const postResp = await this.postService.getById(this.postId);

        // Stop if response is an exception
        if (postResp instanceof CustomError) {
            this.loading = false;
            this.setPostTitle();
            return;
        }

        this.post = postResp;

        this.loading = false;
        this.setPostTitle();
    }

    private async setPostTitle() {
        setTimeout(() => replaceLoadingTitle(this.title, this.post ? this.post.title : ':('), 0);
    }
}
