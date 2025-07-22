import { NgClass, NgIf, NgStyle } from '@angular/common';
import { Component } from '@angular/core';
import { RightSideNav } from '../right-side-nav/right-side-nav';

@Component({
  selector: 'app-navigation',
  imports: [NgStyle,NgClass,RightSideNav,NgIf],
  templateUrl: './navigation.html',
  styleUrl: './navigation.scss'
})
export class Navigation {
  darkMode = false;
  sidebarExpanded = true;
  
  toggleMode() {
    this.darkMode = !this.darkMode;
    document.body.classList.toggle('bg-dark', this.darkMode);
    document.body.classList.toggle('text-white', this.darkMode);
  }
}
