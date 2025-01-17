import { Component, OnInit } from '@angular/core';
import { UiTableComponent } from '../../shared/components/ui-table/ui-table.component';
import { CategoryService } from '../../services/category/category.service';
import { MatDialog } from '@angular/material/dialog';
import { GenericFormModalComponent } from '../../shared/components/generic-form-modal/generic-form-modal.component';
import { Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BreadcrumbComponent } from '../../shared/components/breadcrumbs/breadcrumbs.component';

@Component({
  selector: 'app-category',
  standalone: true,
  imports: [ UiTableComponent, BreadcrumbComponent ],
  templateUrl: './category.component.html',
  styleUrl: './category.component.css'
})
export class CategoryComponent implements OnInit {
  title = 'Categorías';
  displayedColumns = ['name', 'description'];
  categories: any[] = [];
  columnHeaders: { [key: string]: string } = {
    name: 'Nombre',
    description: 'Descripción',
  };

  constructor(private categoryService: CategoryService, private router: Router,private dialog: MatDialog,) {}
  ngOnInit() {
    this.categoryService.getCategories().subscribe(categories => {
      this.categories = categories;
    });
  }

  onAddCategory(): void {
    this.router.navigate(['/categories/add']);
  }
    // console.log('onAddCategory called in CategoryComponent');
    // const dialogRef = this.dialog.open(GenericFormModalComponent, {
    //   width: '500px',
    //   data: {
    //     title: 'Agregar Categoría',
    //     fields: [
    //       {
    //         name: 'name',
    //         label: 'Nombre',
    //         type: 'text',
    //         value: '',
    //         validators: [Validators.required],
    //         errorMessage: 'El nombre es requerido',
    //       },
    //       {
    //         name: 'description',
    //         label: 'Descripción',
    //         type: 'text',
    //         value: '',
    //       },
    //     ],
    //   },
    // });

    // dialogRef.afterClosed().subscribe((result) => {
    //   if (result) {
    //     this.categoryService.createCategory(result).subscribe((category) => {
    //       this.categories.push(category);
    //     });
    //   }
    // });

  onEditCategory(category: any) {
    this.router.navigate(['/categories/edit', category.id]);
  }
  onDeleteCategory(category: any) {
    console.log('Delete category', category);
  }

}
