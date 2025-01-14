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
  displayedColumns = ['name', 'description'];
  categories: any[] = []

  constructor(private categoryService: CategoryService) {}
  ngOnInit() {
    this.categoryService.getCategories().subscribe(categories => {
      this.categories = categories;
    });
  }

}
