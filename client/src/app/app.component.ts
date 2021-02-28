import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'dupa';
  users: any;

  constructor(private http: HttpClient){
    this.getClients()
  }

  private getClients(){
    this.http.get("https://localhost:5001/api/users").subscribe(
      response=>{
        console.log(response)
        this.users = response
      },error=>{
        console.log(error)
      })
  }

}
 