export interface StorageOptions {
    persisted?: boolean;
    cookie?: boolean;
    expiresIn?: Date | number;
}

export interface Storage {
    get<T>(key: string, opts?: StorageOptions): Promise<T>;

    set<T>(key: string, value: T, opts?: StorageOptions): Promise<void>;

    remove(key: string, opts?: StorageOptions): Promise<any>;

    keys(): Promise<string[]>;
}
