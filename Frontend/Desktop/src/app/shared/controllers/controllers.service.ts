import { ApplicationRef, ComponentFactoryResolver, ComponentRef, EmbeddedViewRef, Injectable, Injector, Type } from '@angular/core';

export interface IControllerComponent {
    id: string;
}

@Injectable()
export class ControllersService {
    private i: number = 0;
    private refs: { [key: string]: ComponentRef<any> } = {};

    constructor(private fac: ComponentFactoryResolver, private appRef: ApplicationRef, private injector: Injector) {}

    create<T extends IControllerComponent>(component: Type<T>, resolver?: (instance: T) => void, id?: string): T {
        const componentRef = this.fac.resolveComponentFactory<T>(component as any).create(this.injector);

        id = id || new Date().toString() + this.i;
        componentRef.instance.id = id;
        this.i++;

        if (resolver) resolver(componentRef.instance);

        this.refs[id] = componentRef;

        return componentRef.instance;
    }

    show(id: string, appendTo?: HTMLElement, onAppended?: (el: HTMLElement) => void, parentEl?: HTMLElement) {
        const componentRef = this.refs[id];
        if (!componentRef) return;

        // Attach component to the appRef so that it's inside the ng component tree
        this.appRef.attachView(componentRef.hostView);

        // Get DOM element from component
        const domElem = (componentRef.hostView as EmbeddedViewRef<any>).rootNodes[0] as HTMLElement;

        const append = (el: HTMLElement) => {
            if (onAppended) setTimeout(() => onAppended(el), 0);
            document.body.appendChild(el);
        };

        // Append to given element?
        if (appendTo) {
            if (parentEl) {
                appendTo.appendChild(domElem);
            } else {
                appendTo.id = id;
                appendTo.appendChild(domElem);
                append(appendTo);
                return;
            }
        }

        // Append DOM element to the body
        if (!parentEl) {
            domElem.id = id;
            append(domElem);
        } else {
            parentEl.id = id;
            append(parentEl);
        }
    }

    destroy(id: string) {
        const el = document.getElementById(id);
        if (!el) return;

        el.classList.add('controller-out');

        setTimeout(() => {
            el.remove();
            this.destroyComponent(id);
        }, 300);
    }

    private destroyComponent(id: string) {
        const componentRef = this.refs[id];
        if (!componentRef) return;

        delete this.refs[id];

        this.appRef.detachView(componentRef.hostView);
        componentRef.destroy();
    }
}
