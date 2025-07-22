import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Anonymous } from './components/anonymous/anonymous';
import { Homepage } from './components/homepage/homepage';
import { Login } from './components/login/login';
import { Register } from './components/register/register';

const routes: Routes = [
  { 
    path: '',
    component: Anonymous,
    children: [
      {
        path: '',
        component: Homepage,
          resolve: {
          }
      },
      {
        path: 'login',
        component: Login,
      },
            {
        path: 'register',
        component: Register,
      },
    ],
  },
  {
    path: '*',
    redirectTo: '',
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AnonymousRoutingModule { }
