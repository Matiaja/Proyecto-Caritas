import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule, Location } from '@angular/common';
import { first, switchMap } from 'rxjs';
import { UserService } from '../../../services/user/user.service';
import { CenterService } from '../../../services/center/center.service';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-detail.component.html',
  styleUrl: './user-detail.component.css',
})
export class UserDetailComponent implements OnInit {
  user: any = {
    id: 0,
    userName: '',
    firstName: '',
    lastName: '',
    phoneNumber: '',
    email: '',
    role: '',
    centerId: 0,
    centerName: '',
  };

  // center: any = {
  //   name: '',
  // };

  title = 'Detalle de usuario';
  columnHeaders: Record<string, string> = {
    userName: 'Nombre de usuario',
    firstName: 'Nombre',
    lastName: 'Apellido',
    phoneNumber: 'Teléfono',
    email: 'Correo electrónico',
    role: 'Rol',
    centerName: 'Centro',
  };

  constructor(
    private userService: UserService,
    private centerService: CenterService,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location
  ) {}

  ngOnInit(): void {
    this.route.params
      .pipe(
        switchMap((params) => {
          const userId = params['id'];
          return this.userService.getUserById(userId).pipe(first());
        })
      )
      .subscribe({
        next: (user: any) => {
          this.user = user;
        },
        error: (error) => {
          console.error('Error obteniendo usuario:', error);
        },
      });
  }

  goBack(): void {
    this.location.back();
  }

  onAddElement = null;
  onEditElement = null;
  onDeleteElement = null;
}
