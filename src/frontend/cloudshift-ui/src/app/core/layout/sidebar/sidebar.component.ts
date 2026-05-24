import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <aside class="sidebar" [class.collapsed]="collapsed">
      <!-- Logo -->
      <div class="sidebar-logo">
        <div class="logo-icon">
          <span class="material-symbols-outlined filled icon-lg" style="color:#fff">cloud_sync</span>
        </div>
        <div class="logo-text" *ngIf="!collapsed">
          <span class="logo-name">CloudMigrator</span>
          <span class="logo-edition">Enterprise Edition</span>
        </div>
      </div>

      <!-- Navigation -->
      <nav class="sidebar-nav">
        <ul>
          @for (item of navItems; track item.route) {
            <li>
              <a [routerLink]="item.route"
                 routerLinkActive="active"
                 class="nav-item"
                 [title]="collapsed ? item.label : ''"
                 [attr.aria-label]="item.label">
                <span class="material-symbols-outlined nav-icon">{{ item.icon }}</span>
                <span class="nav-label" *ngIf="!collapsed">{{ item.label }}</span>
              </a>
            </li>
          }
        </ul>
      </nav>

      <!-- Bottom section -->
      <div class="sidebar-bottom">
        <a routerLink="/settings" routerLinkActive="active" class="nav-item" [title]="collapsed ? 'Settings' : ''">
          <span class="material-symbols-outlined nav-icon">settings</span>
          <span class="nav-label" *ngIf="!collapsed">Settings</span>
        </a>
        <div class="user-profile" *ngIf="!collapsed">
          <div class="user-avatar">JD</div>
          <div class="user-info">
            <div class="user-name">John Doe</div>
            <div class="user-role">Administrator</div>
          </div>
          <button class="user-menu-btn" aria-label="User menu">
            <span class="material-symbols-outlined icon-sm">more_vert</span>
          </button>
        </div>
        <div class="user-avatar-mini" *ngIf="collapsed" title="John Doe">JD</div>
      </div>
    </aside>
  `,
  styles: [`
    .sidebar {
      width: var(--sidebar-width);
      height: 100vh;
      background: var(--color-surface-container-lowest);
      border-right: 1px solid var(--color-outline-variant);
      display: flex;
      flex-direction: column;
      flex-shrink: 0;
      transition: width var(--transition-base);
      overflow: hidden;
      position: sticky;
      top: 0;
    }

    .sidebar.collapsed { width: 64px; }

    // Logo
    .sidebar-logo {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 16px;
      height: var(--header-height);
      border-bottom: 1px solid var(--color-outline-variant);
      flex-shrink: 0;
    }

    .logo-icon {
      width: 36px;
      height: 36px;
      background: linear-gradient(135deg, var(--color-primary), var(--color-primary-container));
      border-radius: var(--radius-md);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      box-shadow: 0 2px 8px rgba(0, 87, 194, 0.3);
    }

    .logo-text {
      display: flex;
      flex-direction: column;
      overflow: hidden;
      white-space: nowrap;
    }

    .logo-name {
      font-size: 15px;
      font-weight: 700;
      color: var(--color-on-surface);
      letter-spacing: -0.3px;
    }

    .logo-edition {
      font-size: 10px;
      font-weight: 500;
      color: var(--color-outline);
      text-transform: uppercase;
      letter-spacing: 0.08em;
    }

    // Nav
    .sidebar-nav {
      flex: 1;
      overflow-y: auto;
      padding: 8px;

      ul { display: flex; flex-direction: column; gap: 2px; }
    }

    .nav-item {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 9px 12px;
      border-radius: var(--radius-md);
      color: var(--color-on-surface-variant);
      font-size: 14px;
      font-weight: 500;
      text-decoration: none;
      transition: background var(--transition-fast), color var(--transition-fast);
      white-space: nowrap;
      cursor: pointer;

      &:hover {
        background: var(--color-surface-container);
        color: var(--color-on-surface);
      }

      &.active {
        background: var(--color-primary-fixed);
        color: var(--color-primary-dark);

        .nav-icon { font-variation-settings: 'FILL' 1, 'wght' 400, 'GRAD' 0, 'opsz' 24; }
      }
    }

    .nav-icon {
      font-size: 20px;
      flex-shrink: 0;
    }

    .nav-label { overflow: hidden; text-overflow: ellipsis; }

    // Bottom
    .sidebar-bottom {
      border-top: 1px solid var(--color-outline-variant);
      padding: 8px;
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .user-profile {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 8px;
      border-radius: var(--radius-md);
      cursor: pointer;
      transition: background var(--transition-fast);

      &:hover { background: var(--color-surface-container); }
    }

    .user-avatar {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background: var(--color-primary);
      color: #fff;
      font-size: 12px;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .user-avatar-mini {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background: var(--color-primary);
      color: #fff;
      font-size: 12px;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 4px;
      cursor: pointer;
    }

    .user-info { flex: 1; overflow: hidden; }

    .user-name {
      font-size: 13px;
      font-weight: 600;
      color: var(--color-on-surface);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .user-role {
      font-size: 11px;
      color: var(--color-outline);
    }

    .user-menu-btn {
      color: var(--color-outline);
      border-radius: var(--radius);
      padding: 2px;

      &:hover { background: var(--color-surface-container-high); color: var(--color-on-surface); }
    }
  `]
})
export class SidebarComponent {
  @Input() collapsed = false;

  navItems: NavItem[] = [
    { label: 'Dashboard',       icon: 'dashboard',    route: '/dashboard' },
    { label: 'App Profiles',    icon: 'apps',         route: '/app-profiles' },
    { label: 'Project Mapping', icon: 'map',          route: '/project-mapping' },
    { label: 'Migration Jobs',  icon: 'sync',         route: '/migration-jobs' },
  ];
}
