import {inject, Injectable} from '@angular/core';
import {HttpClient, HttpErrorResponse, HttpParams} from '@angular/common/http';
import {catchError, Observable, throwError} from 'rxjs';
import {ToastService} from '../toast.service';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  //Injections
  private http = inject(HttpClient);
  private toastService = inject(ToastService);

  public readonly baseUrl = "http://localhost:5047";

  get<T>(path: string, query?: Record<string, unknown>): Observable<T> {
    return this.http
      .get<T>(this.url(path), { params: this.toParams(query) })
      .pipe(catchError(this.handleError));
  }

  post<T>(path: string, body?: unknown): Observable<T> {
    return this.http
      .post<T>(this.url(path), body)
      .pipe(catchError(this.handleError));
  }

  put<T>(path: string, body?: unknown): Observable<T> {
    return this.http
      .put<T>(this.url(path), body)
      .pipe(catchError(this.handleError));
  }

  delete<T>(path: string): Observable<T> {
    return this.http
      .delete<T>(this.url(path))
      .pipe(catchError(this.handleError));
  }


  // ---------- Utils ----------

  private toParams(obj?: Record<string, unknown>): HttpParams {
    let params = new HttpParams();
    if (!obj) return params;

    Object.entries(obj).forEach(([key, value]) => {
      if (value === null || value === undefined) return;

      // Gère tableaux / scalaires proprement
      if (Array.isArray(value)) {
        value.forEach(v => {
          if (v !== null && v !== undefined) {
            params = params.append(key, String(v));
          }
        });
      } else {
        params = params.set(key, String(value));
      }
    });

    return params;
  }

  private handleError = (error: HttpErrorResponse) => {
    let message = 'Unexpected error';
    if (error.error?.error) message = error.error.error;
    else if (typeof error.error === 'string') message = error.error;
    else if (error.message) message = error.message;
    this.toastService.error(message);
    return throwError(() => new Error(message));
  };

  private url(path: string): string {
    const p = path.startsWith('/') ? path : `/${path}`;
    return `${this.baseUrl}${p}`;
  }
}
