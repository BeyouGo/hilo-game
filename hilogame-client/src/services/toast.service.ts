// toast.service.ts
import { Injectable, signal } from '@angular/core';

export type ToastType = 'info' | 'warning' | 'error' | 'success';
export interface Toast {
  type: ToastType;
  message: string;
  durationMs?: number;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  toast = signal<Toast | null>(null);
  private timer: any;

  show(message: string, type: ToastType = 'info', durationMs = 4000) {
    this.toast.set({ message, type, durationMs });
    clearTimeout(this.timer);
    if (durationMs > 0) {
      this.timer = setTimeout(() => this.clear(), durationMs);
    }
  }

  info(msg: string, durationMs = 4000)   { this.show(msg, 'info', durationMs); }
  warning(msg: string, durationMs = 5000){ this.show(msg, 'warning', durationMs); }
  error(msg: string, durationMs = 6000)  { this.show(msg, 'error', durationMs); }
  success(msg: string, durationMs = 3000){ this.show(msg, 'success', durationMs); } // optional

  clear() { this.toast.set(null); }
}
