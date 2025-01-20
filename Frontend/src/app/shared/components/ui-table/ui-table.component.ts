import { Component, Input, Output, EventEmitter  } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'ui-table',
  standalone: true,
  imports: [MatTableModule, CommonModule],
  templateUrl: './ui-table.component.html',
  styleUrl: './ui-table.component.css'
})
export class UiTableComponent<T> {
  @Input() title: string = '';
  @Input() displayedColumns: string[] = []; 
  @Input() dataSource: T[] = [];
  @Input() columnHeaders: { [key: string]: string } = {};
  @Output() addElement = new EventEmitter<void>();
  @Output() editElement = new EventEmitter<T>();
  @Output() deleteElement = new EventEmitter<T>();
  
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
}
