import { Component, Input, Output, EventEmitter, TemplateRef } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgxPaginationModule } from 'ngx-pagination';
import { ResponsiveService } from '../../../services/responsive/responsive.service';

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
  @Input() mobileHeaders: { [key: string]: string } = {};
  @Input() showProductsFilters: boolean = false;
  @Input() categories: { id: number; name: string }[] = [];
  @Input() centers: { id: number; name: string }[] = [];
  @Input() sortOptions: { key: string; label: string }[] = [];
  @Input() showCommonFilters: boolean = false;

  @Input() customActions?: TemplateRef<any>;

  @Input() mobileColumns: string[] = [];

  @Input() showAddButton = false;
  @Input() showEditButton = false;
  @Input() showDeleteButton = false;
  @Input() showSelectButton = true;
  @Input() showSearchBar = false;
  @Input() showCenterSelect = false;
  @Input() searchColumns: string[] = [];

  @Input() canEdit = true;
  @Input() canDelete = true;
  @Input() canAdd = true;


  @Output() filterChange = new EventEmitter<{ categoryId?: number; sortBy?: string; order?: string; centerId?: number }>();
  @Output() addElement = new EventEmitter<void>();
  @Output() editElement = new EventEmitter<T>();
  @Output() deleteElement = new EventEmitter<T>();
  @Output() selectElement = new EventEmitter<T>();

  @Output() pageChange = new EventEmitter<number>();
  
  constructor(private responsiveService: ResponsiveService) {
    this.responsiveService.isMobile$.subscribe(isMobile => {
      this.isMobileView = isMobile;
    });
  }
  
  filtersVisible: boolean = false;
  isMobileView = false;
  searchTerm: string = '';
  selectedCategory: number | null = null;
  selectedCenter: number | null = null;
  selectedSortBy: string | null = null;
  selectedOrder: string = 'asc';
  filteredDataSource: T[] = [];

  p: number = 1;
  itemsPerPage: number = 10;
  totalItems: number = 0


  
  ngOnChanges() {
    this.filteredDataSource = [...this.dataSource];
    this.totalItems = this.filteredDataSource.length;
    this.updatePagedData();
    this.applySearchFilter();
  }

  updatePagedData() {
    this.totalItems = this.filteredDataSource.length;
  }

  filterData(emitRemoteFilter: boolean = true) {
    this.applySearchFilter();
    
    if (emitRemoteFilter) {
      this.filterChange.emit({
        categoryId: this.selectedCategory || undefined,
        centerId: this.selectedCenter || undefined,
        sortBy: this.selectedSortBy || undefined,
        order: this.selectedOrder
      });
    }
  
    this.updatePagedData();
  }

  applySearchFilter() {
    if (!this.searchTerm || this.searchTerm.trim() === '') {
      this.filteredDataSource = [...this.dataSource];
      return;
    }

    const searchTermLower = this.searchTerm.toLowerCase();
  
    this.filteredDataSource = this.dataSource.filter(item => {
      const columnsToSearch = this.searchColumns.length > 0 
        ? this.searchColumns 
        : this.displayedColumns;
      
      return columnsToSearch.some(column => {
        const value = item[column];
        if (value === undefined || value === null) return false;
        
        return value.toString().toLowerCase().includes(searchTermLower);
      });
    });
  }

  get columnsToDisplay(): string[] {
    const baseColumns = this.isMobileView && this.mobileColumns.length > 0 
      ? this.mobileColumns 
      : this.displayedColumns;
    
    return [...baseColumns, 'actions'];
  }

  get headersToDisplay(): string[] {
    const baseHeaders = this.isMobileView
      ? this.mobileHeaders 
      : this.columnHeaders;
    
    return Object.keys(baseHeaders).concat('Acciones');
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
