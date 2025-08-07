import { Component } from '@angular/core';
import { Navigation } from '../navigation/navigation';
import { Router, RouterModule } from '@angular/router';
import { RightSideNav } from '../right-side-nav/right-side-nav';
import { Auth } from '../../../shared/services/auth';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports : [Navigation,RouterModule,RightSideNav],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss'
})
export class Dashboard {
  constructor(private authService:Auth,private router:Router) { 
    this.authService.getCurrentUser().subscribe({
      next : (data:any) => {
        this.authService.currentUser.next(data);
        if (data.role !="Admin") {
          this.router.navigate(['/provider']);
        }
        },
      })
   }
}
