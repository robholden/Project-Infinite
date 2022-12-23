import { HttpClient, HttpErrorResponse, HttpEvent, HttpEventType, HttpHeaders, HttpRequest, HttpResponse } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';

import { Observable, Subject } from 'rxjs';
import { catchError, last, map, tap, timeout } from 'rxjs/operators';

import { CustomEvent } from '@shared/enums';
import { executeReCaptcha, isNull, parseEnumValue } from '@shared/functions';
import { INJ_DEVICE, INJ_ENV, INJ_PLATFORM, INJ_TOAST } from '@shared/injectors';
import { Device, Environment, Platform, Toast } from '@shared/interfaces';
import { CustomError, ErrorCode, LoginResponse, SMap } from '@shared/models';
import { AuthState } from '@shared/storage';

import { ReCaptchaService } from './';
import { EventService } from './event.service';

export interface ApiOptions<T> {
    toastError?: boolean;
    recaptchaAction?: string;
    skipRefresh?: boolean;
    isText?: boolean;
    withCredentials?: boolean;
    withResponse?: (response: HttpResponse<T>) => void;
}

@Injectable({
    providedIn: 'root',
})
export class HttpApiService {
    private _refreshing: Subject<CustomError>;
    private _calls: SMap<Subject<any | CustomError>> = {};

    constructor(
        @Inject(INJ_DEVICE) private device: Device,
        @Inject(INJ_PLATFORM) private platform: Platform,
        @Inject(INJ_ENV) private env: Environment,
        @Inject(INJ_TOAST) private toast: Toast,
        private http: HttpClient,
        private recaptcha: ReCaptchaService,
        private events: EventService,
        private authState: AuthState
    ) {}

    /**
     * Constructs a `GET` request that interprets the body as a JSON object and returns
     * the response body in a given type.
     *
     * @param url The endpoint url
     * @param options The http options to send with the request
     */
    async get<T>(url: string, options?: ApiOptions<T>): Promise<T | CustomError> {
        const meta = await this.callMeta(url, options);
        const req = this.http.get<HttpResponse<T>>(meta.url, meta.options);
        return this.handleResponse(url, req, options);
    }

    /**
     * Constructs a `PUT` request that interprets the body as a JSON object and returns
     * the response body in a given type.
     *
     * @param url The endpoint url
     * @param body The resources to add/update.
     * @param options The http options to send with the request
     */
    async put<T>(url: string, body?: any, options?: ApiOptions<T>): Promise<T | CustomError> {
        const meta = await this.callMeta(url, options);
        const req = this.http.put<HttpResponse<T>>(meta.url, body, meta.options);

        return this.handleResponse(url, req, options);
    }

    /**
     * Constructs a `POST` request that interprets the body as a JSON object and returns
     * the response body in a given type.
     *
     * @param url The endpoint url
     * @param body The content to replace with.
     * @param options The http options to send with the request
     */
    async post<T>(url: string, body?: any, options?: ApiOptions<T>): Promise<T | CustomError> {
        const meta = await this.callMeta(url, options);
        const req = this.http.post<HttpResponse<T>>(meta.url, body, meta.options);
        return this.handleResponse(url, req, options);
    }

    /**
     * Constructs a `DELETE` request that interprets the body as a JSON object and returns
     * the response body in a given type.
     *
     * @param url The endpoint url
     * @param options The http options to send with the request
     */
    async delete<T>(url: string, options?: ApiOptions<T>): Promise<T | CustomError> {
        const meta = await this.callMeta(url, options);
        const req = this.http.delete<HttpResponse<T>>(meta.url, meta.options);
        return this.handleResponse(url, req, options);
    }

    /**
     * Uploads a file to an endpoint
     *
     * @param url The endpoint url
     * @param file The file to upload
     * @param progress A callback function to track upload progress
     */
    async upload<T>(url: string, file: File, options?: ApiOptions<T>, progress?: (percent: number) => void): Promise<T | CustomError> {
        const meta = await this.callMeta(url, options);
        const headers = meta.options.headers.append('enctype', 'multipart/form-data');

        const formData: any = new FormData();
        formData.append('files', file, file.name);

        const req = new HttpRequest('POST', meta.url, formData, { ...meta.options, headers, reportProgress: true });

        const getEventMessage = (event: HttpEvent<any>) => {
            switch (event.type) {
                case HttpEventType.UploadProgress:
                    const percent = Math.round((100 * event.loaded) / (event.total ?? 0));
                    return percent;

                default:
                    return event;
            }
        };

        options = options || {};
        options.toastError = options.toastError !== false;

        if (!options.skipRefresh) {
            const refreshed = await this.refreshToken(url);
            if (refreshed !== null) return refreshed;
        }

        const obs = this.http.request<HttpResponse<T | number>>(req).pipe(
            map((event) => getEventMessage(event)),
            tap((percent) => {
                if (typeof percent === 'number') progress(percent);
            }),
            last()
        );

        const resError = async (resolve: any, httpError: HttpErrorResponse) => {
            const error = await this.parseError(httpError, options.toastError);
            resolve(error);
        };

        return new Promise<T | CustomError>((resolve) => {
            obs.pipe(
                timeout(this.env.httpTimeout),
                catchError(async (httpError: HttpErrorResponse) => {
                    await resError(resolve, httpError);
                    if (typeof httpError === 'string') console.error(httpError);
                }),
                tap(() => (this._calls[url] = null))
            ).subscribe(
                (response: HttpResponse<T>) => {
                    resolve(response?.body);
                    if (options.withResponse) options.withResponse(response);
                },
                async (httpError: HttpErrorResponse) => await resError(resolve, httpError)
            );
        });
    }

    private async callMeta(url: string, options?: ApiOptions<any>): Promise<{ url: string; options: { headers: HttpHeaders } }> {
        options = options || {};

        let headers: HttpHeaders = new HttpHeaders();
        if (options.recaptchaAction && this.env.recaptchaKey) {
            const token = await executeReCaptcha(this.env.recaptchaKey, options.recaptchaAction);
            if (token) headers = headers.append('x-recaptcha-token', token);
        }

        const httpOptions = { headers, observe: 'response' };
        if (options.isText) httpOptions['responseType'] = 'text';

        httpOptions['withCredentials'] = options.withCredentials !== false;

        return { url: (url || '').startsWith('http') ? url : this.env.gateway + url, options: httpOptions };
    }

    private async handleResponse<T>(url: string, obs: Observable<HttpResponse<T>>, options?: ApiOptions<T>): Promise<T | CustomError> {
        options = options || {};
        options.toastError = options.toastError !== false;

        if (!options.skipRefresh) {
            const refreshed = await this.refreshToken(url);
            if (refreshed !== null) return refreshed;
        }

        if (this._calls[url]) {
            return await new Promise<T | CustomError>((res) => this._calls[url].subscribe((result) => res(result)));
        }

        const subject = new Subject<T | CustomError>();
        this._calls[url] = subject;

        const resError = async (resolve: any, httpError: HttpErrorResponse) => {
            const error = await this.parseError(httpError, options.toastError);
            resolve(error);
            subject.next(error);
        };

        return new Promise<T | CustomError>((resolve) => {
            obs.pipe(
                timeout(this.env.httpTimeout),
                catchError(async (httpError: HttpErrorResponse) => {
                    await resError(resolve, httpError);
                    if (typeof httpError === 'string') console.error(httpError);
                }),
                tap(() => (this._calls[url] = null))
            ).subscribe(
                (response: HttpResponse<T>) => {
                    resolve(response?.body);
                    subject.next(response?.body);

                    if (options.withResponse) options.withResponse(response);
                },
                async (httpError: HttpErrorResponse) => await resError(resolve, httpError)
            );
        });
    }

    private async parseError(httpError: HttpErrorResponse, toastError: boolean): Promise<CustomError> {
        const errorHttp = this.findError(httpError);
        const error = errorHttp instanceof CustomError ? errorHttp : new CustomError(ErrorCode.HttpError, [errorHttp.status], errorHttp.message);

        if (toastError) this.showError(error);
        return error;
    }

    private findError(httpError: HttpErrorResponse): HttpErrorResponse | CustomError {
        if (httpError instanceof CustomError) return httpError;
        if (!httpError) return new CustomError(ErrorCode.Default, [httpError.error]);

        if (!httpError.error) return this.customErrorFromCode(httpError.status);
        else if (!('code' in httpError.error)) return httpError;

        return new CustomError(httpError.error.code, httpError.error.params, httpError.error.message);
    }

    private showError(exc: CustomError = null) {
        this.toast.showError(exc || new CustomError(ErrorCode.Default));
    }

    private customErrorFromCode(statusCode: number) {
        let errorCode = parseEnumValue(-statusCode, ErrorCode);
        if (isNull(errorCode)) errorCode = ErrorCode.HttpError;

        return new CustomError(errorCode, [statusCode || 0]);
    }

    async refreshToken(url?: string, force?: boolean): Promise<CustomError> {
        const [token, refreshToken, user] = await Promise.all([
            this.authState.retrieve('token'),
            this.authState.retrieve('refreshToken'),
            this.authState.retrieve('user'),
        ]);

        if (url === this.env.refresh || !token || !refreshToken) {
            return null;
        }

        // Check if our refresh token has expired
        const date: Date = await this.authState.retrieve('expires');
        if (!force && user && date && new Date(date) >= new Date()) {
            return null;
        }

        if (this._refreshing) {
            const ref = await new Promise<CustomError>((res) => this._refreshing.subscribe((ex) => res(ex)));
            return ref;
        }

        // Track inital token
        this._refreshing = new Subject<CustomError>();

        const meta = await this.callMeta(this.env.refresh);
        const req = this.http.post<HttpResponse<LoginResponse>>(
            meta.url,
            {
                bearerToken: token,
                refreshToken: refreshToken,
            },
            meta.options
        );
        const resp = await this.handleResponse<LoginResponse>(this.env.refresh, req);
        const return_result = resp instanceof CustomError && refreshToken === (await this.authState.retrieve('refreshToken')) ? resp : null;

        if (!(resp instanceof CustomError)) {
            this.authState.setLoginToken(resp);
        }

        this._refreshing.next(return_result);
        this._refreshing = null;

        if (return_result instanceof CustomError) {
            switch (return_result.code) {
                case ErrorCode.SessionHasExpired:
                case ErrorCode.TokenInvalid:
                case ErrorCode.HttpUnauthorized:
                    this.events.trigger(CustomEvent.Revoked);
                    break;

                case ErrorCode.ReCaptchaFailed:
                    await this.recaptcha.load();
                    break;
            }
        }

        return return_result;
    }
}
