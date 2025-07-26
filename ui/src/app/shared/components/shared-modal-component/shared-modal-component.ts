
import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

// Define interfaces for modal configuration
export interface ModalField {
  key: string;
  label: string;
  type: 'text' | 'email' | 'password' | 'number' | 'textarea' | 'select' | 'checkbox' | 'date' | 'tel' | 'file';
  placeholder?: string;
  required?: boolean;
  options?: { value: any; label: string }[];
  rows?: number; // For textarea
  disabled?: boolean;
  accept?: string; // For file input - e.g., "image/*", ".jpg,.png"
  multiple?: boolean; // For file input - allow multiple files
  validation?: {
    minLength?: number;
    maxLength?: number;
    min?: number;
    max?: number;
    pattern?: string;
    maxFileSize?: number; // In bytes
    allowedFileTypes?: string[]; // e.g., ['jpg', 'png', 'gif']
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
  @Output() fileChanged = new EventEmitter<{ fieldKey: string; file: File | null }>();

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

  /**
   * Handle file input change
   */
  onFileChange(event: any, fieldKey: string): void {
    const file = event.target.files?.[0] || null;
    
    if (file) {
      const field = this.config.fields.find(f => f.key === fieldKey);
      
      // Validate file if validation rules are provided
      if (field?.validation) {
        const validationResult = this.validateFile(file, field.validation);
        if (!validationResult.isValid) {
          this.error = validationResult.errorMessage;
          event.target.value = ''; // Clear the input
          return;
        }
      }
    }

    // Update form control
    const control = this.form.get(fieldKey);
    if (control) {
      control.setValue(file);
      control.markAsTouched();
    }

    // Emit file change event
    this.fileChanged.emit({ fieldKey, file });
  }

  /**
   * Validate file against validation rules
   */
  private validateFile(file: File, validation: ModalField['validation']): { isValid: boolean; errorMessage: string } {
    if (!validation) return { isValid: true, errorMessage: '' };

    // Check file size
    if (validation.maxFileSize && file.size > validation.maxFileSize) {
      const maxSizeMB = validation.maxFileSize / (1024 * 1024);
      return {
        isValid: false,
        errorMessage: `حجم الملف يجب أن يكون أقل من ${maxSizeMB} ميجابايت`
      };
    }

    // Check file type
    if (validation.allowedFileTypes) {
      const fileExtension = file.name.split('.').pop()?.toLowerCase();
      if (!fileExtension || !validation.allowedFileTypes.includes(fileExtension)) {
        return {
          isValid: false,
          errorMessage: `نوع الملف غير مسموح. الأنواع المسموحة: ${validation.allowedFileTypes.join(', ')}`
        };
      }
    }

    return { isValid: true, errorMessage: '' };
  }

  /**
   * Get file name for display
   */
  getFileName(fieldKey: string): string {
    const control = this.form.get(fieldKey);
    const file = control?.value;
    return file instanceof File ? file.name : '';
  }

  /**
   * Clear file input
   */
  clearFile(fieldKey: string, fileInput: HTMLInputElement): void {
    fileInput.value = '';
    const control = this.form.get(fieldKey);
    if (control) {
      control.setValue(null);
    }
    this.fileChanged.emit({ fieldKey, file: null });
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
      case 'file':
        return 'col-12';
      default:
        return 'col-md-6';
    }
  }

  /**
   * Check if field has validation error
   */
  hasFieldError(fieldKey: string): boolean {
    const control = this.form.get(fieldKey);
    return !!(control && control.invalid && (control.dirty || control.touched));
  }

  /**
   * Get field error message
   */
  getFieldErrorMessage(field: ModalField): string {
    const control = this.form.get(field.key);
    if (!control || !control.errors) return '';

    if (control.errors['required']) {
      return `${field.label} مطلوب`;
    }
    if (control.errors['minlength']) {
      return `${field.label} يجب أن يكون على الأقل ${control.errors['minlength'].requiredLength} أحرف`;
    }
    if (control.errors['maxlength']) {
      return `${field.label} يجب أن لا يتجاوز ${control.errors['maxlength'].requiredLength} حرف`;
    }
    if (control.errors['min']) {
      return `${field.label} يجب أن يكون على الأقل ${control.errors['min'].min}`;
    }
    if (control.errors['max']) {
      return `${field.label} يجب أن لا يتجاوز ${control.errors['max'].max}`;
    }
    if (control.errors['email']) {
      return 'يرجى إدخال بريد إلكتروني صحيح';
    }
    if (control.errors['pattern']) {
      return `${field.label} غير صحيح`;
    }

    return 'خطأ في التحقق';
  }
}
