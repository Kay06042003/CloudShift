import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd } from '@angular/router';
import { filter, map } from 'rxjs/operators';

@Component({
  selector: 'app-top-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    <header class="top-header">
      <div class="header-left">
        <button class="toggle-btn" (click)="toggleSidebar.emit()" id="sidebar-toggle-btn" aria-label="Toggle sidebar">
          <span class="material-symbols-outlined">menu</span>
        </button>
        <nav class="breadcrumb" aria-label="Breadcrumb">
          <span class="breadcrumb-parent">CloudMigrator</span>
          <span class="breadcrumb-sep">/</span>
          <span class="breadcrumb-current">{{ currentPageTitle }}</span>
        </nav>
      </div>

      <div class="header-center">
        <div class="search-box">
          <span class="material-symbols-outlined search-icon icon-sm">search</span>
          <input
            type="text"
            class="search-input"
            placeholder="Search jobs, profiles, mappings..."
            id="global-search"
            aria-label="Global search"
          />
          <kbd class="search-kbd">⌘K</kbd>
        </div>
      </div>

      <div class="header-right">
        <button class="header-icon-btn" id="notifications-btn" aria-label="Notifications">
          <span class="material-symbols-outlined">notifications</span>
          <span class="notification-dot"></span>
        </button>
        <button class="header-icon-btn" id="refresh-btn" aria-label="Refresh data">
          <span class="material-symbols-outlined">refresh</span>
        </button>
        <div class="header-divider"></div>
        <button class="user-btn" id="user-menu-btn" aria-label="User menu">
          <div class="user-avatar">JD</div>
          <span class="user-name-header">John Doe</span>
          <span class="material-symbols-outlined icon-sm" style="color:var(--color-outline)">expand_more</span>
        </button>
      </div>
    </header>
  `,
  styles: [`
    .top-header {
      height: var(--header-height);
      background: var(--color-surface-container-lowest);
      border-bottom: 1px solid var(--color-outline-variant);
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 0 20px;
      position: sticky;
      top: 0;
      z-index: 100;
      flex-shrink: 0;
    }

    .header-left {
      display: flex;
      align-items: center;
      gap: 12px;
      flex-shrink: 0;
    }

    .toggle-btn {
      width: 36px;
      height: 36px;
      border-radius: var(--radius-md);
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--color-on-surface-variant);
      transition: background var(--transition-fast);

      &:hover { background: var(--color-surface-container); color: var(--color-on-surface); }
    }

    .breadcrumb {
      display: flex;
      align-items: center;
      gap: 6px;
      font-size: 14px;
    }

    .breadcrumb-parent { color: var(--color-outline); font-weight: 400; }
    .breadcrumb-sep    { color: var(--color-outline-variant); }
    .breadcrumb-current { color: var(--color-on-surface); font-weight: 600; }

    // Search
    .header-center { flex: 1; max-width: 480px; margin: 0 auto; }

    .search-box {
      display: flex;
      align-items: center;
      gap: 8px;
      background: var(--color-surface-container-low);
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius-md);
      padding: 0 12px;
      height: 36px;
      transition: border-color var(--transition-fast), box-shadow var(--transition-fast);

      &:focus-within {
        border-color: var(--color-primary);
        box-shadow: 0 0 0 2px rgba(0, 87, 194, 0.12);
      }
    }

    .search-icon { color: var(--color-outline); }

    .search-input {
      flex: 1;
      border: none;
      background: transparent;
      font-size: 14px;
      color: var(--color-on-surface);
      outline: none;

      &::placeholder { color: var(--color-outline); }
    }

    .search-kbd {
      font-size: 11px;
      color: var(--color-outline);
      background: var(--color-surface-container);
      border: 1px solid var(--color-outline-variant);
      border-radius: 4px;
      padding: 1px 5px;
      font-family: var(--font-sans);
    }

    // Right
    .header-right {
      display: flex;
      align-items: center;
      gap: 4px;
      flex-shrink: 0;
      margin-left: auto;
    }

    .header-icon-btn {
      position: relative;
      width: 36px;
      height: 36px;
      border-radius: var(--radius-md);
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--color-on-surface-variant);
      transition: background var(--transition-fast);

      &:hover { background: var(--color-surface-container); color: var(--color-on-surface); }
    }

    .notification-dot {
      position: absolute;
      top: 6px;
      right: 6px;
      width: 7px;
      height: 7px;
      background: var(--color-error);
      border-radius: 50%;
      border: 1.5px solid var(--color-surface-container-lowest);
    }

    .header-divider {
      width: 1px;
      height: 24px;
      background: var(--color-outline-variant);
      margin: 0 4px;
    }

    .user-btn {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 4px 8px;
      border-radius: var(--radius-md);
      transition: background var(--transition-fast);

      &:hover { background: var(--color-surface-container); }
    }

    .user-avatar {
      width: 28px;
      height: 28px;
      border-radius: 50%;
      background: var(--color-primary);
      color: #fff;
      font-size: 11px;
      font-weight: 700;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .user-name-header {
      font-size: 13px;
      font-weight: 500;
      color: var(--color-on-surface);
    }

    @media (max-width: 768px) {
      .header-center { display: none; }
      .user-name-header { display: none; }
    }
  `]
})
export class TopHeaderComponent {
  @Input() currentPageTitle: string = 'Dashboard';
  @Output() toggleSidebar = new EventEmitter<void>();
}
