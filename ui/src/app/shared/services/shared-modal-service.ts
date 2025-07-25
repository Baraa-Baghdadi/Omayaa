import { Injectable } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BehaviorSubject, Observable } from 'rxjs';
import { ModalConfig, ModalField, ModalResult } from '../components/shared-modal-component/shared-modal-component';

@Injectable({
  providedIn: 'root'
})
export class SharedModalService {
  private configSubject = new BehaviorSubject<ModalConfig | null>(null);
  private formSubject = new BehaviorSubject<FormGroup | null>(null);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private savingSubject = new BehaviorSubject<boolean>(false);
  private errorSubject = new BehaviorSubject<string | null>(null);
  private resultSubject = new BehaviorSubject<ModalResult | null>(null);

  public config$ = this.configSubject.asObservable();
  public form$ = this.formSubject.asObservable();
  public loading$ = this.loadingSubject.asObservable();
  public saving$ = this.savingSubject.asObservable();
  public error$ = this.errorSubject.asObservable();
  public result$ = this.resultSubject.asObservable();

  constructor(private fb: FormBuilder) {}

  /**
   * Open modal with configuration
   * @param config Modal configuration
   * @param initialData Initial form data (optional)
   * @returns Observable of modal result
   */
  openModal(config: ModalConfig, initialData?: any): Observable<ModalResult | null> {
    // Create form based on fields
    const form = this.createFormFromFields(config.fields, initialData);
    
    // Set configuration and form
    this.configSubject.next(config);
    this.formSubject.next(form);
    this.errorSubject.next(null);
    this.loadingSubject.next(false);
    this.savingSubject.next(false);
    
    // Reset result
    this.resultSubject.next(null);
    
    return this.result$;
  }

  /**
   * Create FormGroup from field definitions
   * @param fields Array of modal fields
   * @param initialData Initial form data
   * @returns FormGroup
   */
  private createFormFromFields(fields: ModalField[], initialData?: any): FormGroup {
    const formControls: { [key: string]: any } = {};

    fields.forEach(field => {
      const validators = [];
      
      // Add required validator
      if (field.required) {
        validators.push(Validators.required);
      }

      // Add validation based on field type and validation rules
      switch (field.type) {
        case 'email':
          validators.push(Validators.email);
          break;
        case 'number':
          if (field.validation?.min !== undefined) {
            validators.push(Validators.min(field.validation.min));
          }
          if (field.validation?.max !== undefined) {
            validators.push(Validators.max(field.validation.max));
          }
          break;
        case 'text':
        case 'textarea':
        case 'password':
        case 'tel':
          if (field.validation?.minLength) {
            validators.push(Validators.minLength(field.validation.minLength));
          }
          if (field.validation?.maxLength) {
            validators.push(Validators.maxLength(field.validation.maxLength));
          }
          if (field.validation?.pattern) {
            validators.push(Validators.pattern(field.validation.pattern));
          }
          break;
      }

      // Get initial value
      const initialValue = initialData && initialData[field.key] !== undefined 
        ? initialData[field.key] 
        : (field.type === 'checkbox' ? false : '');

      formControls[field.key] = [initialValue, validators];
    });

    return this.fb.group(formControls);
  }

  /**
   * Set loading state
   * @param loading Loading state
   */
  setLoading(loading: boolean): void {
    this.loadingSubject.next(loading);
  }

  /**
   * Set saving state
   * @param saving Saving state
   */
  setSaving(saving: boolean): void {
    this.savingSubject.next(saving);
  }

  /**
   * Set error message
   * @param error Error message
   */
  setError(error: string | null): void {
    this.errorSubject.next(error);
  }

  /**
   * Set modal result
   * @param result Modal result
   */
  setResult(result: ModalResult): void {
    this.resultSubject.next(result);
  }

  /**
   * Close modal
   */
  closeModal(): void {
    this.configSubject.next(null);
    this.formSubject.next(null);
    this.loadingSubject.next(false);
    this.savingSubject.next(false);
    this.errorSubject.next(null);
  }

  /**
   * Get current form value
   * @returns Current form value or null
   */
  getCurrentFormValue(): any {
    const currentForm = this.formSubject.value;
    return currentForm ? currentForm.value : null;
  }

  /**
   * Get current form
   * @returns Current FormGroup or null
   */
  getCurrentForm(): FormGroup | null {
    return this.formSubject.value;
  }

  /**
   * Update form field value
   * @param fieldKey Field key
   * @param value New value
   */
  updateFieldValue(fieldKey: string, value: any): void {
    const currentForm = this.formSubject.value;
    if (currentForm && currentForm.get(fieldKey)) {
      currentForm.get(fieldKey)?.setValue(value);
    }
  }

  /**
   * Mark all fields as touched
   */
  markAllFieldsAsTouched(): void {
    const currentForm = this.formSubject.value;
    if (currentForm) {
      Object.keys(currentForm.controls).forEach(key => {
        currentForm.get(key)?.markAsTouched();
      });
    }
  }

  /**
   * Reset form
   */
  resetForm(): void {
    const currentForm = this.formSubject.value;
    if (currentForm) {
      currentForm.reset();
    }
  }

  /**
   * Validate form
   * @returns true if form is valid, false otherwise
   */
  validateForm(): boolean {
    const currentForm = this.formSubject.value;
    if (currentForm) {
      if (currentForm.valid) {
        return true;
      } else {
        this.markAllFieldsAsTouched();
        return false;
      }
    }
    return false;
  }

  /**
   * Helper method to create common field configurations
   */
  static createField(
    key: string,
    label: string,
    type: ModalField['type'],
    options?: Partial<ModalField>
  ): ModalField {
    return {
      key,
      label,
      type,
      required: false,
      ...options
    };
  }

  /**
   * Helper method to create text field
   */
  public static createTextField(
    key: string,
    label: string,
    options?: Partial<ModalField>
  ): ModalField {
    return this.createField(key, label, 'text', options);
  }

  /**
   * Helper method to create email field
   */
  static createEmailField(
    key: string,
    label: string,
    options?: Partial<ModalField>
  ): ModalField {
    return this.createField(key, label, 'email', options);
  }

  /**
   * Helper method to create password field
   */
  static createPasswordField(
    key: string,
    label: string,
    options?: Partial<ModalField>
  ): ModalField {
    return this.createField(key, label, 'password', options);
  }

  /**
   * Helper method to create number field
   */
  static createNumberField(
    key: string,
    label: string,
    options?: Partial<ModalField>
  ): ModalField {
    return this.createField(key, label, 'number', options);
  }

  /**
   * Helper method to create textarea field
   */
  static createTextareaField(
    key: string,
    label: string,
    rows: number = 3,
    options?: Partial<ModalField>
  ): ModalField {
    return this.createField(key, label, 'textarea', { rows, ...options });
  }

  /**
   * Helper method to create select field
   */
  static createSelectField(
    key: string,
    label: string,
    options: { value: any; label: string }[],
    fieldOptions?: Partial<ModalField>
  ): ModalField {
    return this.createField(key, label, 'select', { options, ...fieldOptions });
  }

  /**
   * Helper method to create checkbox field
   */
  static createCheckboxField(
    key: string,
    label: string,
    options?: Partial<ModalField>
  ): ModalField {
    return this.createField(key, label, 'checkbox', options);
  }

  /**
   * Helper method to create date field
   */
  static createDateField(
    key: string,
    label: string,
    options?: Partial<ModalField>
  ): ModalField {
    return this.createField(key, label, 'date', options);
  }

  /**
   * Helper method to create phone field
   */
  static createPhoneField(
    key: string,
    label: string,
    options?: Partial<ModalField>
  ): ModalField {
    return this.createField(key, label, 'tel', options);
  }
}
