import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'client';
  user: any;
  constructor(private http: HttpClient) {
  }

  ngOnInit(): void {
    this.user = this.getUsers();
  }


  getUsers() {
    return this.http.get('https://localhost:5001/api/users').subscribe({
      next: (resposne) => {
        this.user = resposne;
      },
      error: (error) => {
        console.log(error);
      },
    });
  }

}
