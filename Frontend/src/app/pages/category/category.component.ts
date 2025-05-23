import { Component, OnInit } from '@angular/core';
import { UiTableComponent } from '../../shared/components/ui-table/ui-table.component';
import { CategoryService } from '../../services/category/category.service';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { ConfirmModalService } from '../../services/confirmModal/confirm-modal.service';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-category',
  standalone: true,
  imports: [UiTableComponent],
  templateUrl: './category.component.html',
  styleUrl: './category.component.css',
})
export class CategoryComponent implements OnInit {
  title = 'Categorías';
  sortBy = '';
  order = 'asc';
  displayedColumns = ['name', 'description'];
  categories: any[] = [];
  columnHeaders: Record<string, string> = {
    name: 'Nombre',
    description: 'Descripción',
  };
  sortOptions = [{ key: 'name', label: 'Nombre' }];
  mobileHeaders: Record<string, string> = {
    name: 'Nombre',
  };
  mobileColumns = ['name'];
  canEdit: boolean = false;
  canDelete: boolean = false;
  canAdd: boolean = false;

  constructor(
    private categoryService: CategoryService,
    private router: Router,
    private dialog: MatDialog,
    private modalService: ConfirmModalService,
    private toastr: ToastrService,
    private authService: AuthService
  ) {}
  ngOnInit() {
    this.checkRole();
    this.loadCategories();
  }

  checkRole() {
    const userRole = this.authService.getUserRole();
    this.canEdit = userRole === 'Admin';
    this.canDelete = userRole === 'Admin';
    this.canAdd = userRole === 'Admin';
  }

  onAddCategory(): void {
    this.router.navigate(['/categories/add']);
  }

  onEditCategory(category: any) {
    this.router.navigate(['/categories/edit', category.id]);
  }

  onSelectCategory(category: any) {
    this.router.navigate(['/categories/detail', category.id]);
  }

  loadCategories() {
    this.categoryService.getFilteredCategories(this.sortBy, this.order);

    this.categoryService.categories$.subscribe((categories) => {
      this.categories = categories;
    });
  }

  onFilterChange(filters: { sortBy?: string; order?: string }) {
    this.sortBy = filters.sortBy || '';
    this.order = filters.order || 'asc';
    this.loadCategories();
  }

  async onDeleteCategory(category: any) {
    const confirmed = await this.modalService.confirm(
      'Eliminar categoría',
      '¿Estás seguro de que quieres eliminar esta categoría?'
    );

    if (confirmed) {
      this.categoryService.deleteCategory(category.id).subscribe(() => {
        this.categories = this.categories.filter((c) => c.id !== category.id);
      });
      this.toastr.success('Categoría eliminada con éxito', 'Exito');
    }
  }
}
