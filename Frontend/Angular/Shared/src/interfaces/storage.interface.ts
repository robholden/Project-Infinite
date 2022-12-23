export type StorageTypes = 'local' | 'session' | 'cookie';

export type StorageOptions<TValue = {}> = {
    keyName?: string;
    store?: StorageTypes[];
    expiresIn?: Date | number;
    defaultValue?: TValue;
};

export interface Storage {
    find<TValue>(key: string, options: StorageOptions<TValue>): Promise<TValue>;

    set<TValue>(key: string, value: TValue, options: StorageOptions<TValue>): Promise<void>;

    remove(key: string, opts?: StorageOptions): Promise<any>;

    keys(): Promise<string[]>;
}
