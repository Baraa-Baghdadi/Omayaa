import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Auth } from '../../../shared/services/auth';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class Register {
  form: FormGroup;

  constructor(private fb: FormBuilder,private authService:Auth, private router:Router) {
    this.form = this.fb.group({
      providerName: ['', [Validators.required, Validators.minLength(5)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required,Validators.minLength(6)]],
      mobile: ['', [Validators.required]],
      telephone: [''],
      address: ['',[Validators.required]]
    });
  }

  onRegister() {
    if (this.form.valid) {
      const requestBody = this.form.value;
      this.authService.register(requestBody).subscribe(
        {next: (response:any) => {
          this.router.navigate(['login']);
        }}
      );
    }
  }
}
