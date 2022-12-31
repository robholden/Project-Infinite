export type Post = {
    title: string;
    body: string;
    author: string;
};

export type PostSearch = {
    username?: string;
    filter?: string;
    filterBy?: PostFilterParams;
};

export enum PostOrderByEnum {
    None = 0,
    CreatedDate,
    UpdatedDate,
    Title,
    Username,
}

export enum PostFilterParams {
    None = 0,
    Title = 1 << 0,
    Body = 1 << 1,
}
