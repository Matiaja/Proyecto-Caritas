import { Component, OnInit } from '@angular/core';
import { UiTableComponent } from '../../shared/components/ui-table/ui-table.component';
import { CategoryService } from '../../services/category/category.service';
import { MatDialog } from '@angular/material/dialog';
import { GenericFormModalComponent } from '../../shared/components/generic-form-modal/generic-form-modal.component';
import { Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BreadcrumbComponent } from '../../shared/components/breadcrumbs/breadcrumbs.component';
import { ConfirmModalService } from '../../services/confirmModal/confirm-modal.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-category',
  standalone: true,
  imports: [ UiTableComponent, BreadcrumbComponent ],
  templateUrl: './category.component.html',
  styleUrl: './category.component.css'
})
export class CategoryComponent implements OnInit {
  title = 'Categorías';
  sortBy: string = '';
  order: string = 'asc';
  displayedColumns = ['name', 'description'];
  categories: any[] = [];
  columnHeaders: { [key: string]: string } = {
    name: 'Nombre',
    description: 'Descripción',
  };
  sortOptions = [
    { key: 'name', label: 'Nombre' }
  ];
  mobileHeaders: { [key: string]: string } = {
    name: 'Nombre'
  };
  mobileColumns = ['name'];

  constructor(
    private categoryService: CategoryService, 
    private router: Router,
    private dialog: MatDialog,
    private modalService: ConfirmModalService,
    private toastr: ToastrService
    ) {}
  ngOnInit() {
    this.loadCategories();
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
    this.categoryService.getFilteredCategories(
      this.sortBy,
      this.order
    );

    this.categoryService.categories$
    .subscribe(categories => {
      this.categories = categories;
    });
  }

  onFilterChange(filters: { sortBy?: string; order?: string }) {
    this.sortBy = filters.sortBy || '';
    this.order = filters.order || 'asc';
    this.loadCategories();
  }

  async onDeleteCategory(category: any) {
    const confirmed = await this.modalService.confirm('Eliminar categoría', 
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
