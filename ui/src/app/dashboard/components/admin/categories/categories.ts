import { Component, OnInit, ViewChild } from '@angular/core';
import { ModalConfig, ModalResult, SharedModalComponent } from '../../../../shared/components/shared-modal-component/shared-modal-component';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { SharedModalService } from '../../../../shared/services/shared-modal-service';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './categories.html',
  styleUrl: './categories.scss'
})
export class Categories {
}
