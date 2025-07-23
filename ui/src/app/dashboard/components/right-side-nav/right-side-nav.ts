import { Component } from '@angular/core';
import { ThemeService } from '../../../shared/services/theme-service';
import { Auth } from '../../../shared/services/auth';

@Component({
  selector: 'app-right-side-nav',
  imports: [],
  templateUrl: './right-side-nav.html',
  styleUrl: './right-side-nav.scss'
})
export class RightSideNav {
  darkMode = false;

  constructor(private themeService: ThemeService,private authService:Auth) {
    this.darkMode = this.themeService.getMode() === 'dark';
  }

  toggleMode() {
    this.darkMode = !this.darkMode;
    const mode = this.darkMode ? 'dark' : 'light';
    this.themeService.setMode(mode);
  }

  logOut(){
    this.authService.logOut();
  }
}
