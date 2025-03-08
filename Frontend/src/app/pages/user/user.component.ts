import { Component, OnInit } from '@angular/core';
import { UiTableComponent } from '../../shared/components/ui-table/ui-table.component';
import { MatDialog } from '@angular/material/dialog';
import { GenericFormModalComponent } from '../../shared/components/generic-form-modal/generic-form-modal.component';
import { Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BreadcrumbComponent } from '../../shared/components/breadcrumbs/breadcrumbs.component';
import { ConfirmModalService } from '../../services/confirmModal/confirm-modal.service';
import { UserService } from '../../services/user/user.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-user',
  standalone: true,
  imports: [ UiTableComponent, BreadcrumbComponent ],
  templateUrl: './user.component.html',
  styleUrl: './user.component.css'
})
export class UserComponent implements OnInit{
  title = 'Usuarios';
  displayedColumns = ['userName', 'email', 'centerName'];
  users: any[] = [];
  columnHeaders: { [key: string]: string } = {
    userName: 'Nombre de usuario',
    email: 'Correo electrónico',
    centerName: 'Centro',
  };

  showSearchBar = true;
  searchColumns = ['userName', 'email', 'centerName'];

  constructor(private userService: UserService, 
    private router: Router, 
    private modalService: ConfirmModalService,
    private toastr: ToastrService
  ) { }

  ngOnInit() {
    this.userService.users$.subscribe(users => {
      this.users = users.map(user => ({
        ...user,
        centerName: user.centerName ? user.centerName : "Usuario sin centro"
      }));
    });
    this.userService.getUsersNoAdmin();
    console.log(this.users);
  }

  onAddUser(): void {
    this.router.navigate(['/users/add']);
  }

  onSelectUser(user: any) {
    this.router.navigate(['/users/detail', user.id]);
  }

  async onDeleteUser(user: any) {
    const confirmed = await this.modalService.confirm('Eliminar usuario', 
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
