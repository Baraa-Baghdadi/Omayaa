import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnonymousRoutingModule } from './anonymous-routing-module';
import { Anonymous } from './components/anonymous/anonymous';
import { Homepage } from './components/homepage/homepage';
import { Login } from './components/login/login';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Register } from './components/register/register';
@NgModule({
  declarations: [
    Anonymous,
    Homepage,
    Login,
    Register
  ],
  imports: [
    CommonModule,
    AnonymousRoutingModule,
    FormsModule, 
    ReactiveFormsModule
]
})
export class AnonymousModule { }
