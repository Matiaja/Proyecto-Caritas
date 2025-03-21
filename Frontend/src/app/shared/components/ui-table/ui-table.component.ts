import { Component, Input, Output, EventEmitter, TemplateRef } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgxPaginationModule } from 'ngx-pagination';

@Component({
  selector: 'ui-table',
  standalone: true,
  imports: [NgxPaginationModule, MatTableModule, CommonModule, FormsModule],
  templateUrl: './ui-table.component.html',
  styleUrl: './ui-table.component.css'
})
export class UiTableComponent<T extends Record<string, any>>{
  paginationId = 'tablePagination';
  @Input() title: string = '';
  @Input() displayedColumns: string[] = []; 
  @Input() dataSource: T[] = [];
  @Input() columnHeaders: { [key: string]: string } = {};
  @Input() showFilters: boolean = false;
  @Input() categories: { id: number; name: string }[] = [];
  @Input() sortOptions: { key: string; label: string }[] = [];

  @Input() customActions?: TemplateRef<any>;

  @Input() showAddButton = false;
  @Input() showEditButton = false;
  @Input() showDeleteButton = false;
  @Input() showSelectButton = true;
  @Input() showSearchBar = false;
  @Input() searchColumns: string[] = [];

  @Output() filterChange = new EventEmitter<{ categoryId?: number; sortBy?: string; order?: string }>();
  @Output() addElement = new EventEmitter<void>();
  @Output() editElement = new EventEmitter<T>();
  @Output() deleteElement = new EventEmitter<T>();
  @Output() selectElement = new EventEmitter<T>();

  @Output() pageChange = new EventEmitter<number>();
  filtersVisible: boolean = false;
  
  searchTerm: string = '';
  selectedCategory: number | null = null;
  selectedSortBy: string | null = null;
  selectedOrder: string = 'asc';
  filteredDataSource: T[] = [];

  p: number = 1;
  itemsPerPage: number = 2;
  totalItems: number = 0

  ngOnChanges() {
    this.filteredDataSource = [...this.dataSource];
    this.totalItems = this.dataSource.length;
    this.updatePagedData();
    this.filterData();
  }

  updatePagedData() {
    this.totalItems = this.filteredDataSource.length;
  }

  filterData() {
    this.filterChange.emit({
      categoryId: this.selectedCategory || undefined,
      sortBy: this.selectedSortBy || undefined,
      order: this.selectedOrder
    });

    this.updatePagedData();
  }

  get columnsToDisplay(): string[] {
    return [...this.displayedColumns, 'actions'];
  }

  onPageChange(newPage: number) {
    this.p = newPage;
    this.pageChange.emit(newPage);
  }

  onAddElement(): void {
    this.addElement.emit();
  }

  onEditElement(element: T): void {
    this.editElement.emit(element);
  }

  onDeleteElement(element: T): void {
    this.deleteElement.emit(element);
  }

  onSelectElement(element: T): void {
    this.selectElement.emit(element);
  }

  toggleFilters() {
    this.filtersVisible = !this.filtersVisible;
  }
}
