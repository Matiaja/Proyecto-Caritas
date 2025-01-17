import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { SignupComponent } from './pages/signup/signup.component';
import { AdminComponent } from './pages/admin/admin.component';
import { authGuard } from './auth/auth.guard';
import { HomeComponent } from './pages/home/home.component';
import { RequestComponent } from './pages/request/request.component';
import { CenterComponent } from './pages/center/center.component';
import { CategoryComponent } from './pages/category/category.component';
import { AddCategoryComponent } from './pages/category/add-category/add-category.component';

export const routes: Routes = [
    {
        path: '', redirectTo: '/login', pathMatch: 'full'
    },
    {
        path: 'login', component: LoginComponent
    },
    {
        path: 'signup', component: SignupComponent
    },
    {
        path: 'admin', component: AdminComponent, canActivate: [authGuard]
    },
    { 
        path: 'home', component: HomeComponent, canActivate: [authGuard]
    },
    { 
        path: 'requests', component: RequestComponent, canActivate: [authGuard] 
    },
    { 
        path: 'centers', component: CenterComponent, canActivate: [authGuard] 
    },
    { 
        path: 'categories', component: CategoryComponent, canActivate: [authGuard]
    },
    {
        path: 'categories/add', component: AddCategoryComponent, canActivate: [authGuard]
    }
];
