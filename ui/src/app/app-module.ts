import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { AppRoutingModule } from './app-routing-module';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { App } from './app';
import { injecTokenInterceptor } from './shared/interceptors/injec-token-interceptor';
import { SharedPopup } from './shared/components/shared-popup/shared-popup';

@NgModule({
  declarations: [
    App,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    NgbModule,
    SharedPopup
  ],
  providers: [
    {provide: HTTP_INTERCEPTORS, useClass: injecTokenInterceptor, multi: true },
    provideBrowserGlobalErrorListeners(),
  ],
  bootstrap: [App]
})
export class AppModule { }
