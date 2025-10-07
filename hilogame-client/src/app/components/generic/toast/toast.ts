// app-toast.component.ts
import { Component, inject } from '@angular/core';
import { ToastService, ToastType } from '../../../../services/toast.service';
import {NgClass} from '@angular/common';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [
    NgClass
  ],
  template: `
    <div class="position-fixed bottom-0 end-0 p-3" style="z-index:1080;">
      @if (toastService.toast()) {
        @let t = toastService.toast()!;
        <div class="toast show border-0 shadow-lg"
             [ngClass]="variantClasses[t.type]"
             role="alert"
             aria-atomic="true">
          <div class="toast-body d-flex align-items-center gap-2">
            <span aria-hidden="true">{{ icon(t.type) }}</span>
            <div class="me-auto">{{ t.message }}</div>
            <button type="button"
                    class="btn-close ms-2"
                    [ngClass]="closeBtnClasses[t.type]"
                    aria-label="Close"
                    (click)="toastService.clear()"></button>
          </div>
        </div>
      }
    </div>
  `
})
export class AppToastComponent {
  toastService = inject(ToastService);

  variantClasses: Record<ToastType, string> = {
    info:    'text-white bg-info',
    success: 'text-white bg-success',
    warning: 'text-dark  bg-warning',
    error:   'text-white bg-danger',
  };

  closeBtnClasses: Record<ToastType, string> = {
    info: 'btn-close-white',
    success: 'btn-close-white',
    warning: '',
    error: 'btn-close-white',
  };

  icon(type: ToastType) {
    switch (type) {
      case 'info':    return 'ℹ️';
      case 'success': return '✅';
      case 'warning': return '⚠️';
      case 'error':   return '⛔️';
    }
  }
}
