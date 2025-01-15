import { Component, OnInit } from '@angular/core';
import { UiTableComponent } from '../../shared/components/ui-table/ui-table.component';
import { CategoryService } from '../../services/category/category.service';
@Component({
  selector: 'app-category',
  standalone: true,
  imports: [ UiTableComponent ],
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

  constructor(private categoryService: CategoryService) {}
  ngOnInit() {
    this.categoryService.getCategories().subscribe(categories => {
      this.categories = categories;
    });
  }

  onAddCategory() {
    console.log('crear categoria');
  }
  onEditCategory(category: any) {
    console.log('Edit category', category);
  }
  onDeleteCategory(category: any) {
    console.log('Delete category', category);
  }

}
