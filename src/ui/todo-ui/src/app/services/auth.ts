import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, catchError, map, Observable, of, tap, throwError } from 'rxjs';
import { environment } from '../../environments/environment';

export interface AuthUser {
  id: number,
  name: string,
  roles: string[]
}

export interface LoginResponse {
  token: string
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private url = `${environment.apiBaseUrl}/api/auth`;
  // roles: string[] = []; // in-memory store
  roles$ = new BehaviorSubject<string[]>([]);
  user: string = '';
  token: string = '';

  constructor(private http: HttpClient) { }

  login(username: string, password: string): Observable<string> {
    return this.http.post<LoginResponse>(`${this.url}/login`, { username, password }).pipe(
      tap(e => this.token = e.token),
      map(e => e.token),
      catchError(err => {
        console.error('Error from service ', err.error);
        return throwError(() => err); // rethrow
      })
    )
  }

  isAuthenticated(): Observable<boolean> {
    return this.roles$.pipe(
      map(e => e.length > 0)
    )
  }

  getAuthenticatedUser(): Observable<string[]> {
    if (!this.token) return of([]);

    return this.http.get<AuthUser>(`${this.url}/authenticated-user?token=${this.token}`,
      {
        headers: {
          Authorization: `Bearer ${this.token}`
        }
      }
    ).pipe(
      tap(e => {
        this.roles$.next(e.roles)
        this.user = e.name;
      }),
      map(e => e.roles)
    );
  }

  getUserName() {
    return this.user;
  }

  loadRolesIfEmpty(): Observable<string[]> {
    if (this.roles$.value.length > 0) {
      return new Observable<string[]>(observer => {
        observer.next(this.roles$.value);
        observer.complete();
      });
    }

    // call api once, then store
    return this.getAuthenticatedUser();
  }

  hasRole(role: string): boolean {
    return this.roles$.value.includes(role);
  }

  getToken(): string | null {
    return this.token;
  }
}
