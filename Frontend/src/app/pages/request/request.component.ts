import { Component } from '@angular/core';
import { Request } from '../../models/request';
import { RequestService } from '../../services/request.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-request',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './request.component.html',
  styleUrl: './request.component.css'
})
export class RequestComponent {
  
  requests: Request[] = [];

  constructor(private reqService: RequestService) { }
  
  ngOnInit() {
    this.reqService.getRequests().subscribe({
      next: (reqs: Request[]) => {
        this.requests = reqs;
        console.log(this.requests);
    },
    error: (err) => {
      console.log(err);
    }
    });
  } 
  

}
