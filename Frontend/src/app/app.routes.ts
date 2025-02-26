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
import { RequestAddComponent } from './pages/request/request-add/request-add.component';
import { ProductComponent } from './pages/product/product.component';
import { ProductAddComponent } from './pages/product/product-add/product-add.component';
import { CenterAddComponent } from './pages/center/center-add/center-add.component';
import { CenterEditComponent } from './pages/center/center-edit/center-edit.component';
import { CenterDetailComponent } from './pages/center/center-detail/center-detail.component';
import { CategoryDetailComponent } from './pages/category/category-detail/category-detail.component';
import { ProductDetailComponent } from './pages/product/product-detail/product-detail.component';
import { StorageComponent } from './pages/storage/storage.component';
import { StorageAddComponent } from './pages/storage/storage-add/storage-add.component';
import { StorageDetailComponent } from './pages/storage/storage-detail/storage-detail.component';
import { UserComponent } from './pages/user/user.component';
import { UserAddComponent } from './pages/user/user-add/user-add.component';
import { UserDetailComponent } from './pages/user/user-detail/user-detail.component';

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
        path: 'requests', canActivate: [authGuard],
        children: [
            { path: '', component: RequestComponent },
            { path: 'add', component: RequestAddComponent },
            { path: ':id', component: RequestDetailComponent },
            { path: ':id/assign', component: RequestAssignComponent }
        ]
    },
    {path: 'center/:id', component: CenterDetailComponent, data: { breadcrumb: 'Detalle de Centro' }} ,
    { 
        path: 'centers',
        data: { breadcrumb: 'Centros' },  
        canActivate: [authGuard],
         children: [
            {path: '', component: CenterComponent},
            {path: 'add', component: CenterAddComponent, data: { breadcrumb: 'Agregar Centro' } },
            {path: 'edit/:id', component: CenterEditComponent, data: { breadcrumb: 'Editar Centro' } },
            //{path: 'center/:id', component: CenterDetailComponent, data: { breadcrumb: 'Detalle de Centro' }} 
        ]
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
    },
    {
        path: 'users',
        data: { breadcrumb: 'Usuarios' },
        canActivate: [authGuard],
        children: [
            {path: '', component: UserComponent},
            {path: 'add', component: UserAddComponent, data: { breadcrumb: 'Agregar Usuario' }},
            {path: 'detail/:id', component: UserDetailComponent, data: { breadcrumb: 'Detalle de Usuario' }},
            ]
    }
];
