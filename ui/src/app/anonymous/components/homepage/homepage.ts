import { Component, OnDestroy, OnInit, Renderer2 } from '@angular/core';

@Component({
  selector: 'app-homepage',
  standalone: false,
  templateUrl: './homepage.html',
  styleUrl: './homepage.scss'
})
export class Homepage implements OnInit,OnDestroy {
    
  constructor(private renderer: Renderer2) {}

  ngOnInit(): void {
    // hide scroll from body
    this.renderer.setStyle(document.body, 'overflow', 'hidden');
  }

  ngOnDestroy(): void {
    // go back
    this.renderer.removeStyle(document.body, 'overflow');
  }
}
