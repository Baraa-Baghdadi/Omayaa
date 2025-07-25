
import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

// Define interfaces for modal configuration
export interface ModalField {
  key: string;
  label: string;
  type: 'text' | 'email' | 'password' | 'number' | 'textarea' | 'select' | 'checkbox' | 'date' | 'tel';
  placeholder?: string;
  required?: boolean;
  options?: { value: any; label: string }[];
  rows?: number; // For textarea
  disabled?: boolean;
  validation?: {
    minLength?: number;
    maxLength?: number;
    min?: number;
    max?: number;
    pattern?: string;
  };
}

export interface ModalConfig {
  title: string;
  subtitle?: string;
  fields: ModalField[];
  saveButtonText?: string;
  cancelButtonText?: string;
  saveButtonClass?: string;
  cancelButtonClass?: string;
  size?: 'sm' | 'lg' | 'xl';
  centered?: boolean;
  backdrop?: boolean | 'static';
  keyboard?: boolean;
  showCloseButton?: boolean;
}

export interface ModalResult {
  action: 'save' | 'cancel' | 'close';
  data?: any;
}

// Declare Bootstrap for TypeScript
declare var bootstrap: any;

@Component({
  selector: 'app-shared-modal-component',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './shared-modal-component.html',
  styleUrl: './shared-modal-component.scss'
})
export class SharedModalComponent implements OnInit, OnDestroy {
 @ViewChild('modalElement', { static: true }) modalElement!: ElementRef;

  @Input() config: ModalConfig = {
    title: '',
    fields: [],
    saveButtonText: 'حفظ',
    cancelButtonText: 'إلغاء',
    showCloseButton: true,
    centered: true,
    backdrop: true,
    keyboard: true
  };

  @Input() form!: FormGroup;
  @Input() loading = false;
  @Input() saving = false;
  @Input() error: string | null = null;

  @Output() modalResult = new EventEmitter<ModalResult>();
  @Output() saveClicked = new EventEmitter<any>();
  @Output() cancelClicked = new EventEmitter<void>();
  @Output() closeClicked = new EventEmitter<void>();

  private modal: any;

  ngOnInit(): void {
    this.initializeModal();
  }

  ngOnDestroy(): void {
    if (this.modal) {
      this.modal.dispose();
    }
  }

  private initializeModal(): void {
    setTimeout(() => {
      if (this.modalElement?.nativeElement) {
        this.modal = new bootstrap.Modal(this.modalElement.nativeElement, {
          backdrop: this.config.backdrop !== false ? (this.config.backdrop === 'static' ? 'static' : true) : false,
          keyboard: this.config.keyboard !== false,
          focus: true
        });

        // Listen for modal events
        this.modalElement.nativeElement.addEventListener('hidden.bs.modal', () => {
          this.onModalHidden();
        });
      }
    }, 100);
  }

  public show(): void {
    if (this.modal) {
      this.modal.show();
    }
  }

  public hide(): void {
    if (this.modal) {
      this.modal.hide();
    }
  }

  onSave(): void {
    if (this.form.valid) {
      const formData = this.form.value;
      this.saveClicked.emit(formData);
      this.modalResult.emit({
        action: 'save',
        data: formData
      });
    } else {
      // Mark all fields as touched to show validation errors
      Object.keys(this.form.controls).forEach(key => {
        this.form.get(key)?.markAsTouched();
      });
    }
  }

  onCancel(): void {
    this.cancelClicked.emit();
    this.modalResult.emit({
      action: 'cancel'
    });
    this.hide();
  }

  onClose(): void {
    this.closeClicked.emit();
    this.modalResult.emit({
      action: 'close'
    });
    this.hide();
  }

  private onModalHidden(): void {
    // Reset form and clear error when modal is hidden
    if (this.form) {
      this.form.reset();
    }
    this.error = null;
  }

  getFieldColumnClass(field: ModalField): string {
    // You can customize column classes based on field type or add custom logic
    switch (field.type) {
      case 'checkbox':
        return 'col-12';
      case 'textarea':
        return 'col-12';
      default:
        return 'col-md-6';
    }
  }
}
