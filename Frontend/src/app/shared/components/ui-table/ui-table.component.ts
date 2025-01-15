import { Component, Input } from '@angular/core';
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
  @Input() displayedColumns: string[] = []; 
  @Input() dataSource: T[] = [];
}
