import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
    {
    path: 'app',
    loadChildren: () => import('./dashboard/dashboard-module').then((m) => m.DashboardModule),
  },
  {
    path: '',
    loadChildren: () =>
      import('./anonymous/anonymous-module').then((m) => m.AnonymousModule),
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
