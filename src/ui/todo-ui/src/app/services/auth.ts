import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private url = `${environment.apiBaseUrl}/api/auth`;
  roles: string[] = []; // in-memory store

  constructor(private http: HttpClient) { }

  getAuthenticatedUser(): Observable<string[]> {
    return this.http.get<string[]>(this.url); // your url
  }

  loadRolesIfEmpty(): Observable<string[]> {
    if (this.roles.length > 0) {
      // already have roles -> no api call
      return new Observable<string[]>(observer => {
        observer.next(this.roles);
        observer.complete();
      });
    }

    // call api once, then store
    return this.getAuthenticatedUser().pipe(
      tap(r => (this.roles = r ?? []))
    );
  }

  hasRole(role: string): boolean {
    return this.roles.includes(role);
  }
}
