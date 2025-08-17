import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Category } from '../../../models/category.model';
import { CategoryService } from '../../../services/category/category.service';
import { CommonModule, Location } from '@angular/common';
import { UiTableComponent } from "../../../shared/components/ui-table/ui-table.component";

@Component({
  selector: 'app-category-detail',
  standalone: true,
  imports: [CommonModule, UiTableComponent],
  templateUrl: './category-detail.component.html',
  styleUrl: './category-detail.component.css',
})
export class CategoryDetailComponent implements OnInit {
  category: Category = {
    id: 0,
    name: '',
    description: '',
  } as Category;

  // Datos de la tabla
  products: any[] = [];
  displayedColumns = ['id', 'code', 'name'];
  columnHeaders = {
    id: 'ID',
    code: 'Código',
    name: 'Nombre'
  };
  title = 'Productos de la categoría';
  mobileColumns = ['code', 'name'];
  mobileHeaders = {
    code: 'Código',
    name: 'Nombre'
  };

  constructor(
    private categoryService: CategoryService,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      const categoryId = params['id'];
      this.loadCategoryDetails(categoryId);
    });
  }

  loadCategoryDetails(categoryId: any): void {
    this.categoryService.getCategory(categoryId).subscribe({
      next: (category: Category) => {
        this.category = category;
        this.products = category.products || [];
      },
    });
  }

  goBack() {
    this.location.back();
  }

  onAddElement = null;
  onEditElement = null;
  onDeleteElement = null;
}
