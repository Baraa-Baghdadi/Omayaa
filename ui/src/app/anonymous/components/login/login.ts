import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Auth } from '../../../shared/services/auth';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {  
  form!:FormGroup;
  currentUser:any;

  constructor(
    private router:Router,
    private fb:FormBuilder,
    private authService : Auth
  ) {}

  ngOnInit() {
    if (this.authService.isLogin$()) {
      this.buildForm();
      this.router.navigate(['app']);
    }
    else{
      this.buildForm();
    }
  }
  
  logIn(){ 
    this.authService.logIn(this.form.value).subscribe({
      next:(data:any) =>{
        this.authService.removeAllToken();
        this.authService.AccessToken.next(data.accessToken);
        this.authService.refreshToken.next(data.refreshToken)
        this.authService.setTokenInLocal(data);
        this.getCurrentUser();
      }
    }) 
  }

  buildForm(){
    this.form = this.fb.group({
      providerName:['',[Validators.minLength(5),Validators.required]],
      password:['',[Validators.required,Validators.minLength(6)]]
    });
  }

  get providerName() {
    return this.form.get('providerName');
  }

  get password() {
    return this.form.get('password');
  }

  getCurrentUser(){
    this.authService.getCurrentUser().subscribe({
      next : (data:any) => {
      this.authService.currentUser.next(data);
      console.log(data.role);
      if (data.role !="Admin") {
        this.router.navigate(['/provider']);
      }
      else{
        // this.router.navigate(['/app']);
      }
      },
    })
  }


  onSubmit(): void {
        this.authService.logIn(this.form.value).subscribe({
      next:(data:any) =>{
        this.authService.removeAllToken();
        this.authService.AccessToken.next(data.accessToken);
        this.authService.refreshToken.next(data.refreshToken)
        this.authService.setTokenInLocal(data);
        this.getCurrentUser();
        this.authService.getCurrentUser().subscribe({
        next : (data:any) => {
          this.authService.currentUser.next(data);
          if (data.role !="Admin") {
            this.router.navigate(['/provider']);
          }
          else{
            this.router.navigate(['/app']);
          }
          },
        })
      }
    }) 
  }
}
