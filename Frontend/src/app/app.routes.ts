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
import { EditCategoryComponent } from './pages/category/edit-category/edit-category.component';
import { RequestDetailComponent } from './pages/request/request-detail/request-detail.component';
import { RequestAssignComponent } from './pages/request/request-assign/request-assign.component';
import { ProductComponent } from './pages/product/product.component';
import { ProductAddComponent } from './pages/product/product-add/product-add.component';
import { CategoryDetailComponent } from './pages/category/category-detail/category-detail.component';
import { ProductDetailComponent } from './pages/product/product-detail/product-detail.component';
import { StorageComponent } from './pages/storage/storage.component';
import { StorageAddComponent } from './pages/storage/storage-add/storage-add.component';
import { StorageDetailComponent } from './pages/storage/storage-detail/storage-detail.component';

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
        path: 'admin', 
        component: AdminComponent, 
        canActivate: [authGuard],
        children: [
            
        ]
    },
    { 
        path: 'home', component: HomeComponent, canActivate: [authGuard]
    },
    { 
        path: 'requests', component: RequestComponent, canActivate: [authGuard] 
    },
    { 
        path: 'requests/:id', component: RequestDetailComponent 
    },
    { 
        path: 'requests/:id/assign', component: RequestAssignComponent 
    },
    { 
        path: 'centers', component: CenterComponent, canActivate: [authGuard] 
    },
    {
        path: 'categories',
        data: { breadcrumb: 'Categorías' },
        canActivate: [authGuard],
        children: [
          { path: '', component: CategoryComponent },
          { path: 'add', component: AddCategoryComponent, data: { breadcrumb: 'Agregar Categoría' } },
          { path: 'edit/:id', component: EditCategoryComponent, data: { breadcrumb: 'Editar Categoría' } },
          { path: 'detail/:id', component: CategoryDetailComponent, data: { breadcrumb: 'Detalle de Categoría' } }
        ]
    },
    {
        path: 'products',
        data: { breadcrumb: 'Productos' },
        canActivate: [authGuard],
        children: [
            { path: '', component: ProductComponent },
            { path: 'add', component: ProductAddComponent, data: { breadcrumb: 'Agregar Producto' } },
            { path: 'detail/:id', component: ProductDetailComponent, data: { breadcrumb: 'Detalle de Producto' } }
        ]
    },
    {
        path: 'storage',
        data: { breadcrumb: 'Almacén' },
        canActivate: [authGuard],
        children: [
            {path: '', component: StorageComponent},
            {path: 'add', component: StorageAddComponent, data: { breadcrumb: 'Agregar Stock' }},
            {path: 'detail/:id', component: StorageDetailComponent, data: { breadcrumb: 'Detalle de Stock' }}
        ]
    }
];
