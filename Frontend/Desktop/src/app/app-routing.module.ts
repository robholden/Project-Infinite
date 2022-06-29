import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';

import { AuthGuard } from '@shared/middleware';

const routes: Routes = [
    { path: '', loadChildren: () => import('./pages/everyone/home-page/home.module').then((m) => m.HomePageModule) },
    { path: 'faq', loadChildren: () => import('./pages/everyone/faq-page/faq.module').then((m) => m.FAQPageModule) },
    { path: 'policies', loadChildren: () => import('./pages/everyone/policies-page/policies.module').then((m) => m.PoliciesPageModule) },
    {
        path: 'reset-password',
        loadChildren: () => import('./pages/everyone/reset-password-page/reset-password.module').then((m) => m.ResetPasswordPageModule),
    },
    {
        path: 'confirm-email',
        loadChildren: () => import('./pages/everyone/confirm-email-page/confirm-email.module').then((m) => m.ConfirmEmailPageModule),
    },
    { path: 'unsubscribe', loadChildren: () => import('./pages/everyone/unsubscribe-page/unsubscribe.module').then((m) => m.UnsubscribePageModule) },
    { path: 'picture', loadChildren: () => import('./pages/everyone/picture-page/picture.module').then((m) => m.PicturePageModule) },
    { path: 'collection', loadChildren: () => import('./pages/everyone/collection-page/collection.module').then((m) => m.CollectionPageModule) },
    { path: 'user', loadChildren: () => import('./pages/everyone/user-page/user.module').then((m) => m.UserPageModule) },

    {
        path: 'settings',
        loadChildren: () => import('./pages/users/settings-page/settings.module').then((m) => m.SettingsPageModule),
        canActivate: [AuthGuard],
    },

    {
        path: 'mod',
        loadChildren: () => import('./pages/moderators/moderator.module').then((m) => m.ModeratorPageModule),
        data: { roles: ['Moderator'] },
        canActivate: [AuthGuard],
    },

    {
        path: 'testing',
        loadChildren: () => import('./pages/everyone/testing-page/testing.module').then((m) => m.TestingPageModule),
    },

    { path: '**', redirectTo: '', pathMatch: 'full' },
];

@NgModule({
    imports: [RouterModule.forRoot(routes, { preloadingStrategy: PreloadAllModules })],
    exports: [RouterModule],
})
export class AppRoutingModule {}
