import { Component } from '@angular/core';
import { Navigation } from '../navigation/navigation';
import { RouterModule } from '@angular/router';
import { RightSideNav } from '../right-side-nav/right-side-nav';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports : [Navigation,RouterModule,RightSideNav],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard {

}
