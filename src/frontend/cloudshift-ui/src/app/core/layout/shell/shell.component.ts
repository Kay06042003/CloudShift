import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopHeaderComponent } from '../top-header/top-header.component';

const PAGE_TITLES: Record<string, string> = {
  '/dashboard':       'Dashboard',
  '/app-profiles':    'App Profiles',
  '/project-mapping': 'Project Mapping',
  '/migration-jobs':  'Migration Jobs',
  '/settings':        'Settings',
};

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, SidebarComponent, TopHeaderComponent],
  template: `
    <div class="shell" [class.sidebar-collapsed]="sidebarCollapsed()">
      <app-sidebar [collapsed]="sidebarCollapsed()" />

      <div class="shell-main">
        <app-top-header
          [currentPageTitle]="currentPageTitle()"
          (toggleSidebar)="toggleSidebar()"
        />
        <main class="shell-content">
          <router-outlet />
        </main>
      </div>
    </div>

    <!-- Mobile sidebar overlay -->
    <div
      class="sidebar-overlay"
      *ngIf="mobileMenuOpen()"
      (click)="mobileMenuOpen.set(false)"
    ></div>
  `,
  styles: [`
    .shell {
      display: flex;
      height: 100vh;
      background: var(--color-background);
      overflow: hidden;
    }

    .shell-main {
      flex: 1;
      display: flex;
      flex-direction: column;
      overflow: hidden;
      min-width: 0;
    }

    .shell-content {
      flex: 1;
      overflow-y: auto;
      overflow-x: hidden;
      background: var(--color-background);
    }

    .sidebar-overlay {
      position: fixed;
      inset: 0;
      background: rgba(0,0,0,0.4);
      z-index: 200;
      display: none;
    }

    @media (max-width: 768px) {
      .sidebar-overlay { display: block; }
    }
  `]
})
export class ShellComponent {
  sidebarCollapsed = signal(false);
  mobileMenuOpen = signal(false);
  currentPageTitle = signal('Dashboard');

  constructor(private router: Router) {
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(e => {
        const nav = e as NavigationEnd;
        const path = '/' + nav.urlAfterRedirects.split('/')[1];
        this.currentPageTitle.set(PAGE_TITLES[path] ?? 'CloudMigrator');
      });
  }

  toggleSidebar() {
    this.sidebarCollapsed.update(v => !v);
  }
}
