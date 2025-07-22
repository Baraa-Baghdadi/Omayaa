import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
   private readonly MODE_KEY = 'user-theme-mode';

  getMode(): 'dark' | 'light' {
    const mode = localStorage.getItem(this.MODE_KEY);
    return (mode === 'dark' || mode === 'light') ? mode : 'dark';
  }

  setMode(mode: 'dark' | 'light'): void {
    localStorage.setItem(this.MODE_KEY, mode);
    console.log("Here",localStorage.getItem(this.MODE_KEY));
    
    this.applyMode(mode);
  }

  applyMode(mode: 'dark' | 'light'){
    const body = document.body;
    if (mode === 'dark') {
      body.classList.add('dark-mode');
    } else {
      body.classList.remove('dark-mode');
    }
  }
}
