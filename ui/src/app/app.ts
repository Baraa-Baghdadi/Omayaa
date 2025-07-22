import { Component, OnInit } from '@angular/core';
import * as AOS from 'aos';
@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected title = 'ui';

    ngOnInit(): void {
      AOS.init({
        duration: 1000,
        easing: 'ease-in-out'
    });
  }
}
