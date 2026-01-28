import { Component, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './pages/shared/navbar/navbar';
import { AuthService } from './services/auth';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, NavbarComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  constructor(private auth: AuthService) {

  }
  ngOnInit(): void {
    this.auth.loadRolesIfEmpty().subscribe({
      error: () => {
        this.auth.roles = []
      }
    });
  }
}
