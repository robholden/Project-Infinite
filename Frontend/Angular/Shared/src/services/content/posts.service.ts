import { Injectable } from '@angular/core';

import { CustomError, Post } from '@shared/models';

import { HttpApiService } from '../';

@Injectable({
    providedIn: 'root',
})
export class PostsService {
    constructor(private api: HttpApiService) {}

    /**
     * Returns all available posts
     */
    async getAll(): Promise<Post[] | CustomError> {
        return await this.api.get<Post[]>(`/content/posts`);
    }
}
