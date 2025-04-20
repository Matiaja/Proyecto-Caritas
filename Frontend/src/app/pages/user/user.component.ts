import { Component, OnInit } from '@angular/core';
import { UiTableComponent } from '../../shared/components/ui-table/ui-table.component';
import { Router } from '@angular/router';
import { BreadcrumbComponent } from '../../shared/components/breadcrumbs/breadcrumbs.component';
import { ConfirmModalService } from '../../services/confirmModal/confirm-modal.service';
import { UserService } from '../../services/user/user.service';
import { ToastrService } from 'ngx-toastr';
import { CenterService } from '../../services/center/center.service';

@Component({
  selector: 'app-user',
  standalone: true,
  imports: [UiTableComponent, BreadcrumbComponent],
  templateUrl: './user.component.html',
  styleUrl: './user.component.css',
})
export class UserComponent implements OnInit {
  title = 'Usuarios';
  displayedColumns = ['userName', 'email', 'centerName'];
  users: any[] = [];
  selectedCenter: number | null = null;
  sortBy = '';
  order = 'asc';
  columnHeaders: Record<string, string> = {
    userName: 'Nombre de usuario',
    email: 'Correo electrónico',
    centerName: 'Centro',
  };
  centers: any[] = [];
  showSearchBar = true;
  searchColumns = ['userName', 'email', 'centerName'];
  sortOptions = [{ key: 'userName', label: 'Nombre de usuario' }];
  mobileHeaders: Record<string, string> = {
    userName: 'Nombre de usuario',
  };
  mobileColumns = ['userName'];

  constructor(
    private userService: UserService,
    private router: Router,
    private modalService: ConfirmModalService,
    private toastr: ToastrService,
    private centerService: CenterService
  ) {}

  ngOnInit() {
    this.loadCenters();
    // this.userService.users$.subscribe(users => {
    //   this.users = users.map(user => ({
    //     ...user,
    //     centerName: user.centerName ? user.centerName : "Usuario sin centro"
    //   }));
    // });
    // this.userService.getUsersNoAdmin();
    // console.log(this.users);
    this.loadUsers();
  }

  loadCenters() {
    this.centerService.getCenters().subscribe((centers) => {
      this.centers = centers.map((center) => ({
        ...center,
        name: center.name,
      }));
    });
  }

  loadUsers() {
    this.userService.getFilteredUsers(this.selectedCenter ?? undefined, this.sortBy, this.order);
    this.userService.users$.subscribe((users) => {
      this.users = users.map((user) => ({
        ...user,
        centerName: user.centerId
          ? user.centerName
            ? user.centerName
            : 'Centro número ' + user.centerId
          : user.centerName
            ? user.centerName
            : 'Usuario sin centro',
      }));
    });
  }

  onFilterChange(filter: { centerId?: number; sortBy?: string; order?: string }) {
    this.selectedCenter = filter.centerId ?? null;
    this.sortBy = filter.sortBy ?? '';
    this.order = filter.order ?? 'asc';
    this.loadUsers();
  }

  onAddUser(): void {
    this.router.navigate(['/users/add']);
  }

  onSelectUser(user: any) {
    this.router.navigate(['/users/detail', user.id]);
  }

  async onDeleteUser(user: any) {
    const confirmed = await this.modalService.confirm(
      'Eliminar usuario',
      '¿Estás seguro de que quieres eliminar este usuario?'
    );

    if (confirmed) {
      this.userService.deleteUser(user.id).subscribe(() => {
        this.users = this.users.filter((u) => u.id !== user.id);
      });
      this.toastr.success('Usuario eliminado correctamente');
    }
  }
}
