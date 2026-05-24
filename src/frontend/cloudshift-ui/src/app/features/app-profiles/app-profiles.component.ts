import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MockDataService } from '../../services/mock-data.service';
import { IAppProfile } from '../../models/app-profile.model';
import { ProfileCardComponent } from './profile-card/profile-card.component';
import { AddProfileModalComponent } from './add-profile-modal/add-profile-modal.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-app-profiles',
  standalone: true,
  imports: [CommonModule, FormsModule, ProfileCardComponent, AddProfileModalComponent, StatusBadgeComponent],
  template: `
    <div class="page-container">
      <!-- Page Header -->
      <div class="page-header">
        <div>
          <h1 class="page-title">App Profiles</h1>
          <p class="page-subtitle">Manage connected cloud storage accounts for migration sources and destinations.</p>
        </div>
        <div class="page-actions">
          <button class="btn-secondary" id="refresh-profiles-btn" (click)="refresh()">
            <span class="material-symbols-outlined icon-sm">refresh</span>
            Refresh
          </button>
          <button class="btn-primary" id="add-profile-btn" (click)="showAddModal.set(true)">
            <span class="material-symbols-outlined icon-sm">add</span>
            Add Profile
          </button>
        </div>
      </div>

      <!-- Filter / Search Bar -->
      <div class="filter-bar">
        <div class="search-input-wrap">
          <span class="material-symbols-outlined icon-sm search-icon-inner">search</span>
          <input
            type="text"
            class="cs-input search-field"
            placeholder="Search profiles..."
            [(ngModel)]="searchQuery"
            id="profiles-search"
            aria-label="Search profiles"
          />
        </div>

        <div class="filter-chips">
          @for (f of statusFilters; track f.value) {
            <button
              class="filter-chip"
              [class.active]="activeFilter === f.value"
              (click)="activeFilter = f.value"
              [id]="'filter-' + f.value"
            >
              {{ f.label }}
              <span class="chip-count">{{ getFilterCount(f.value) }}</span>
            </button>
          }
        </div>

        <div class="view-toggle">
          <button class="view-btn" [class.active]="viewMode === 'grid'" (click)="viewMode = 'grid'" id="grid-view-btn" aria-label="Grid view">
            <span class="material-symbols-outlined icon-sm">grid_view</span>
          </button>
          <button class="view-btn" [class.active]="viewMode === 'list'" (click)="viewMode = 'list'" id="list-view-btn" aria-label="List view">
            <span class="material-symbols-outlined icon-sm">list</span>
          </button>
        </div>
      </div>

      <!-- Summary Stats -->
      <div class="summary-bar">
        <div class="summary-item">
          <span class="material-symbols-outlined icon-sm" style="color:var(--color-primary)">storage</span>
          <span class="label-sm">Total Storage:</span>
          <strong>{{ totalStorageGB | number:'1.0-0' }} GB</strong>
        </div>
        <div class="summary-item">
          <span class="material-symbols-outlined icon-sm" style="color:#1a7a4a">check_circle</span>
          <span class="label-sm">Active:</span>
          <strong>{{ activeCount }}</strong>
        </div>
        <div class="summary-item">
          <span class="material-symbols-outlined icon-sm" style="color:var(--color-error)">error</span>
          <span class="label-sm">Errors:</span>
          <strong>{{ errorCount }}</strong>
        </div>
      </div>

      <!-- Profile Grid -->
      <div class="profiles-grid" *ngIf="viewMode === 'grid'">
        @for (profile of filteredProfiles; track profile.id) {
          <app-profile-card [profile]="profile" class="animate-fade-in" />
        } @empty {
          <div class="empty-state">
            <span class="material-symbols-outlined icon-xl" style="color:var(--color-outline);font-size:48px">cloud_off</span>
            <h3>No profiles found</h3>
            <p>Try adjusting your search or filter, or add a new profile.</p>
            <button class="btn-primary" (click)="showAddModal.set(true)">
              <span class="material-symbols-outlined icon-sm">add</span>
              Add Your First Profile
            </button>
          </div>
        }
      </div>

      <!-- Profile List -->
      <div class="cs-table-wrapper" *ngIf="viewMode === 'list'">
        <table class="cs-table">
          <thead>
            <tr>
              <th>Profile Name</th>
              <th>Provider</th>
              <th>Account</th>
              <th>Storage</th>
              <th>Files</th>
              <th>Last Sync</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            @for (p of filteredProfiles; track p.id) {
              <tr class="animate-fade-in">
                <td><strong>{{ p.name }}</strong></td>
                <td class="provider-label">{{ p.provider }}</td>
                <td class="code-text">{{ p.email }}</td>
                <td>{{ p.storageUsedGB }}/{{ p.storageTotalGB }} GB</td>
                <td>{{ p.filesCount | number }}</td>
                <td>{{ p.lastSync | date:'MMM d, HH:mm' }}</td>
                <td><app-status-badge [status]="p.status" /></td>
                <td class="actions-cell">
                  <button class="btn-icon-sm" title="Edit"><span class="material-symbols-outlined icon-sm">edit</span></button>
                  <button class="btn-icon-sm" title="Sync"><span class="material-symbols-outlined icon-sm">sync</span></button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </div>

    <!-- Add Profile Modal -->
    <app-add-profile-modal
      [isOpen]="showAddModal()"
      (close)="showAddModal.set(false)"
      (profileAdded)="onProfileAdded()"
    />
  `,
  styles: [`
    .filter-bar {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 16px;
      flex-wrap: wrap;
    }

    .search-input-wrap {
      position: relative;
      flex: 1;
      min-width: 200px;
      max-width: 320px;

      .search-icon-inner {
        position: absolute;
        left: 10px;
        top: 50%;
        transform: translateY(-50%);
        color: var(--color-outline);
      }

      .search-field { padding-left: 32px; }
    }

    .filter-chips {
      display: flex;
      gap: 6px;
      flex-wrap: wrap;
    }

    .filter-chip {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 4px 12px;
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius-full);
      font-size: 13px;
      font-weight: 500;
      color: var(--color-on-surface-variant);
      background: var(--color-surface-container-lowest);
      transition: all 0.15s;
      cursor: pointer;

      &:hover { border-color: var(--color-primary-fixed-dim); }
      &.active { background: var(--color-primary-fixed); border-color: var(--color-primary); color: var(--color-primary); }
    }

    .chip-count {
      background: var(--color-surface-container-high);
      border-radius: var(--radius-full);
      font-size: 11px;
      padding: 0 5px;
      min-width: 18px;
      text-align: center;
    }

    .view-toggle {
      display: flex;
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius);
      overflow: hidden;
      margin-left: auto;
    }

    .view-btn {
      padding: 6px 10px;
      color: var(--color-on-surface-variant);
      transition: background 0.15s;

      &:hover { background: var(--color-surface-container); }
      &.active { background: var(--color-primary-fixed); color: var(--color-primary); }
    }

    .summary-bar {
      display: flex;
      gap: 20px;
      padding: 10px 16px;
      background: var(--color-surface-container-low);
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius-md);
      margin-bottom: 20px;
    }

    .summary-item {
      display: flex;
      align-items: center;
      gap: 6px;
      font-size: 13px;
      color: var(--color-on-surface-variant);

      strong { color: var(--color-on-surface); font-weight: 600; }
    }

    .profiles-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: 16px;
    }

    .empty-state {
      grid-column: 1 / -1;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 12px;
      padding: 64px 24px;
      text-align: center;

      h3 { font-size: 16px; font-weight: 600; }
      p  { font-size: 14px; color: var(--color-outline); }
    }

    .actions-cell {
      display: flex;
      gap: 4px;
    }

    .btn-icon-sm {
      width: 26px;
      height: 26px;
      border-radius: var(--radius);
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--color-on-surface-variant);
      transition: background 0.15s;

      &:hover { background: var(--color-surface-container); color: var(--color-on-surface); }
    }

    .provider-label {
      font-size: 12px;
      color: var(--color-outline);
      text-transform: capitalize;
    }

    .btn-primary {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 8px 16px;
      background: var(--color-primary);
      color: #fff;
      border-radius: var(--radius);
      font-size: 14px;
      font-weight: 500;
      transition: background 0.15s;

      &:hover { background: var(--color-primary-dark); }
    }

    .btn-secondary {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 8px 14px;
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius);
      font-size: 14px;
      font-weight: 500;
      color: var(--color-on-surface-variant);
      background: var(--color-surface-container-lowest);
      transition: background 0.15s;

      &:hover { background: var(--color-surface-container); }
    }
  `]
})
export class AppProfilesComponent implements OnInit {
  profiles: IAppProfile[] = [];
  searchQuery = '';
  activeFilter = 'all';
  viewMode: 'grid' | 'list' = 'grid';
  showAddModal = signal(false);

  statusFilters = [
    { value: 'all',    label: 'All' },
    { value: 'active', label: 'Active' },
    { value: 'idle',   label: 'Idle' },
    { value: 'error',  label: 'Error' },
  ];

  constructor(private mockData: MockDataService) {}

  ngOnInit() {
    this.profiles = this.mockData.getAppProfiles();
  }

  get filteredProfiles(): IAppProfile[] {
    return this.profiles.filter(p => {
      const matchSearch =
        !this.searchQuery ||
        p.name.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        p.email.toLowerCase().includes(this.searchQuery.toLowerCase());
      const matchFilter = this.activeFilter === 'all' || p.status === this.activeFilter;
      return matchSearch && matchFilter;
    });
  }

  get totalStorageGB(): number {
    return this.profiles.reduce((sum, p) => sum + p.storageTotalGB, 0);
  }

  get activeCount(): number {
    return this.profiles.filter(p => p.status === 'active').length;
  }

  get errorCount(): number {
    return this.profiles.filter(p => p.status === 'error').length;
  }

  getFilterCount(filter: string): number {
    if (filter === 'all') return this.profiles.length;
    return this.profiles.filter(p => p.status === filter).length;
  }

  refresh() {
    this.profiles = this.mockData.getAppProfiles();
  }

  onProfileAdded() {
    // In production: reload from API
    console.log('Profile added — would reload from API');
  }
}
