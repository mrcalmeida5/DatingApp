import { Component, OnInit } from '@angular/core';
import {  Router } from '@angular/router';

@Component({
  selector: 'app-server-error',
  templateUrl: './server-error.component.html',
  styleUrls: ['./server-error.component.css']
})
export class ServerErrorComponent implements OnInit {

  error: any;

  constructor(private router: Router) {
    var error = router.getCurrentNavigation()?.extras?.state;
    this.error = error?.error; // router.getCurrentNavigation?.arguments?.extras?.state?.error;
   }

  ngOnInit(): void {
  }

}
