import { Component, Input, Output, EventEmitter, TemplateRef } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';


@Component({
  selector: 'ui-table',
  standalone: true,
  imports: [MatTableModule, CommonModule, FormsModule],
  templateUrl: './ui-table.component.html',
  styleUrl: './ui-table.component.css'
})
export class UiTableComponent<T extends Record<string, any>>{
  @Input() title: string = '';
  @Input() displayedColumns: string[] = []; 
  @Input() dataSource: T[] = [];
  @Input() columnHeaders: { [key: string]: string } = {};

  @Input() customActions?: TemplateRef<any>;

  @Input() showAddButton = false;
  @Input() showEditButton = false;
  @Input() showDeleteButton = false;
  @Input() showSelectButton = true;
  @Input() showSearchBar = false;
  @Input() searchColumns: string[] = [];

  @Output() addElement = new EventEmitter<void>();
  @Output() editElement = new EventEmitter<T>();
  @Output() deleteElement = new EventEmitter<T>();
  @Output() selectElement = new EventEmitter<T>();

  searchTerm: string = '';
  filteredDataSource: T[] = [];

  ngOnChanges() {
    this.filteredDataSource = [...this.dataSource];
  }

  filterData() {
    const searchTermLower = this.searchTerm.toLowerCase().trim();
    if (!searchTermLower) {
      this.filteredDataSource = [...this.dataSource];
    } else {
      this.filteredDataSource = this.dataSource.filter(item =>
        this.searchColumns.some(col =>
          String(item[col] || '').toLowerCase().includes(searchTermLower)
        )
      );
    }
  }

  get columnsToDisplay(): string[] {
    return [...this.displayedColumns, 'actions'];
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
}
