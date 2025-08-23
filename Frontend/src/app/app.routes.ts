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
import { UserEditComponent } from './pages/user/user-edit/user-edit.component';
import { NotFoundComponent } from './pages/not-found/not-found.component';
import { roleGuard } from './guards/role.guard';
import { UnauthorizedComponent } from './pages/unauthorized/unauthorized.component';
import { OrderlineComponent } from './pages/orderline/orderline/orderline.component';
import { MovementComponent } from './pages/movement/movement.component';
import { MovementDetailComponent } from './pages/movement/movement-detail/movement-detail.component';
import { PurchaseComponent } from './pages/purchase/purchase.component';
import { PurchaseDetailComponent } from './pages/purchase/purchase-detail/purchase-detail.component';
import { DistributionComponent } from './pages/purchase/distribution/distribution.component';
import { PurchaseAddComponent } from './pages/purchase/purchase-add/purchase-add.component';
import { ProductEditComponent } from './pages/product/product-edit/product-edit.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/login',
    pathMatch: 'full',
  },
  {
    path: 'login',
    component: LoginComponent,
    data: { hideNavbar: true, hideFooter: true, hideBreadcrumbs: true },
  },
  {
    path: 'signup',
    component: SignupComponent,
    data: { hideNavbar: true, hideFooter: true, hideBreadcrumbs: true },
  },
  {
    path: 'admin',
    component: AdminComponent,
    canActivate: [authGuard],
    children: [],
  },
  {
    path: 'home',
    component: HomeComponent,
    canActivate: [authGuard],
  },
  {
    path: 'requests',
    data: { breadcrumb: 'Solicitudes' },
    canActivate: [authGuard],
    children: [
      { path: '', component: RequestComponent },
      { path: 'add', component: RequestAddComponent, data: { breadcrumb: 'Agregar solicitudes' }, },
      { path: ':id', component: RequestDetailComponent, data: { breadcrumb: 'Detalle solicitud' }, },
      { path: ':id/assign/:idorderline', component: RequestAssignComponent, data: { breadcrumb: 'Asignar solicitud' } },
    ],
  },
  {
    path: 'orderline/:id',
    component: OrderlineComponent,
    canActivate: [authGuard],
  },
  {
    path: 'center/:id',
    component: CenterDetailComponent,
    data: { breadcrumb: 'Detalle de Centro' },
  },
  {
    path: 'centers',
    data: { breadcrumb: 'Centros' },
    canActivate: [authGuard],
    children: [
      { path: '', component: CenterComponent },
      { path: 'add', component: CenterAddComponent, data: { breadcrumb: 'Agregar Centro' } },
      { path: 'edit/:id', component: CenterEditComponent, data: { breadcrumb: 'Editar Centro' } },
      //{path: 'center/:id', component: CenterDetailComponent, data: { breadcrumb: 'Detalle de Centro' }}
    ],
  },
  {
    path: 'categories',
    data: { breadcrumb: 'Categorías' },
    canActivate: [authGuard],
    children: [
      { path: '', component: CategoryComponent },
      { path: 'add', component: AddCategoryComponent, data: { breadcrumb: 'Agregar Categoría' } },
      {
        path: 'edit/:id',
        component: EditCategoryComponent,
        data: { breadcrumb: 'Editar Categoría' },
      },
      {
        path: 'detail/:id',
        component: CategoryDetailComponent,
        data: { breadcrumb: 'Detalle de Categoría' },
      },
    ],
  },
  {
    path: 'movements',
    data: { breadcrumb: 'Movimientos' },
    canActivate: [authGuard],
    children: [
      { path: '', component: MovementComponent },
      { path: ':id', component: MovementDetailComponent, data: { breadcrumb: 'Detalle de movimiento' } }
    ]
  },
  {
    path: 'products',
    data: { breadcrumb: 'Productos' },
    canActivate: [authGuard],
    children: [
      { path: '', component: ProductComponent },
      { path: 'add', component: ProductAddComponent, data: { breadcrumb: 'Agregar Producto' } },
      { path: 'edit/:id', component: ProductEditComponent, data: { breadcrumb: 'Editar Producto' } },
      {
        path: 'detail/:id',
        component: ProductDetailComponent,
        data: { breadcrumb: 'Detalle de Producto' },
      },
    ],
  },
  {
    path: 'purchases',
    data: { breadcrumb: 'Compras y bolsones' },
    canActivate: [authGuard],
    children: [
      { path: '', component: PurchaseComponent },
      { path: 'add', component: PurchaseAddComponent, data: { breadcrumb: 'Nueva compra' }  },
      { path: ':id', component: PurchaseDetailComponent, data: { breadcrumb: 'Detalle de compra' }  },
      { path: ':id/distribute', component: DistributionComponent, data: { breadcrumb: 'Nueva salida' }  }
    ]
  },
  {
    path: 'storage',
    data: { breadcrumb: 'Almacén' },
    canActivate: [authGuard],
    children: [
      { path: '', component: StorageComponent },
      { path: 'add', component: StorageAddComponent, data: { breadcrumb: 'Agregar Stock' } },
      {
        path: 'detail/:id',
        component: StorageDetailComponent,
        data: { breadcrumb: 'Detalle de Stock' },
      },
    ],
  },
  {
    path: 'users',
    data: { breadcrumb: 'Usuarios' },
    canActivate: [authGuard],
    children: [
      { path: '', component: UserComponent },
      { path: 'add', component: UserAddComponent, data: { breadcrumb: 'Agregar Usuario' } },
      {
        path: 'detail/:id',
        component: UserDetailComponent,
        data: { breadcrumb: 'Detalle de Usuario' },
      },
      { path: 'edit', component: UserEditComponent, data: { breadcrumb: 'Editar Usuario' } },
    ],
  },
  {
    path: 'unauthorized',
    component: UnauthorizedComponent,
  },
  //Esta ruta tiene que estar siempre ultima para capturar cualquier ruta no definida
  //y redirigir a la página de error 404
  {
    path: '**',
    component: NotFoundComponent,
    data: { hideNavbar: true },
  },
];
