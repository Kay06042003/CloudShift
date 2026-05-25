import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  AddProfileFormValue,
  CloudShiftApiService,
  CreateOAuthProviderAppFormValue,
  IOAuthProviderApp,
  UpdateOAuthProviderAppFormValue
} from '../../services/cloudshift-api.service';
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

      @if (oauthStatus() === 'success') {
        <div class="oauth-banner success">
          <span class="material-symbols-outlined icon-sm">check_circle</span>
          Provider connected. The new app profile is available below.
        </div>
      }

      @if (oauthStatus() === 'error') {
        <div class="oauth-banner error">
          <span class="material-symbols-outlined icon-sm">error</span>
          {{ oauthErrorMessage() }}
        </div>
      }

      <div class="provider-apps-panel">
        <div class="panel-header">
          <div>
            <h2>OAuth Provider Apps</h2>
            <p>Configure customer-owned Google or Microsoft OAuth clients before connecting cloud accounts.</p>
          </div>
          <button class="btn-secondary" type="button" (click)="toggleProviderAppForm()">
            <span class="material-symbols-outlined icon-sm">{{ showProviderAppForm ? 'close' : 'add' }}</span>
            {{ showProviderAppForm ? 'Cancel' : 'Add OAuth App' }}
          </button>
        </div>

        @if (showProviderAppForm) {
          <form class="provider-app-form" (ngSubmit)="saveProviderApp()" #providerAppFormRef="ngForm">
            <div class="form-row">
              <label>
                Provider
                <select
                  class="cs-select"
                  name="provider"
                  [(ngModel)]="providerAppForm.provider"
                  (ngModelChange)="onProviderAppProviderChanged($event)"
                  [disabled]="isProviderChangeLocked"
                  required
                >
                  <option value="google-drive">Google Drive</option>
                  <option value="onedrive">OneDrive</option>
                </select>
              </label>
              <label>
                Name
                <input class="cs-input" name="name" [(ngModel)]="providerAppForm.name" required placeholder="Company Google OAuth App" />
              </label>
            </div>

            <div class="form-row">
              <label>
                Client ID
                <input class="cs-input" name="clientId" [(ngModel)]="providerAppForm.clientId" required />
              </label>
              <label>
                Client Secret Value
                <input
                  class="cs-input"
                  name="clientSecret"
                  type="password"
                  [(ngModel)]="providerAppForm.clientSecret"
                  [required]="!editingProviderAppId"
                  [placeholder]="editingProviderAppId ? 'Leave blank to keep current secret value' : 'Paste the secret value, not the secret ID'"
                />
              </label>
            </div>

            <div class="form-row">
              <label>
                Redirect URI
                <input class="cs-input code-input" name="redirectUri" [(ngModel)]="providerAppForm.redirectUri" required />
              </label>
              @if (providerAppForm.provider === 'onedrive') {
                <label>
                  Tenant
                  <input class="cs-input" name="tenantId" [(ngModel)]="providerAppForm.tenantId" placeholder="common" />
                </label>
              }
            </div>

            <label>
              Scopes
              <input class="cs-input code-input" name="scopes" [(ngModel)]="providerAppForm.scopes" required />
            </label>

            <label class="checkbox-row">
              <input name="isActive" type="checkbox" [(ngModel)]="providerAppForm.isActive" />
              Active
            </label>

            <div class="form-actions">
              <button class="btn-primary" type="submit" [disabled]="providerAppFormRef.invalid || isSavingProviderApp">
                <span class="material-symbols-outlined icon-sm">save</span>
                {{ editingProviderAppId ? 'Update OAuth App' : 'Save OAuth App' }}
              </button>
            </div>
          </form>
        }

        <div class="provider-app-list">
          @for (app of providerApps; track app.id) {
            <div class="provider-app-row" [class.inactive]="!app.isActive">
              <span class="material-symbols-outlined icon-sm">{{ app.provider === 'onedrive' ? 'cloud' : 'add_to_drive' }}</span>
              <div>
                <strong>{{ app.name }}</strong>
                <p>{{ providerLabel(app.provider) }} · {{ app.clientId }}</p>
                <p>{{ app.linkedProfileCount }} linked profile{{ app.linkedProfileCount === 1 ? '' : 's' }} · {{ app.isActive ? 'Active' : 'Inactive' }}</p>
              </div>
              <code>{{ app.redirectUri }}</code>
              <div class="provider-app-actions">
                <button class="btn-icon-sm" type="button" title="Edit OAuth app" (click)="editProviderApp(app)">
                  <span class="material-symbols-outlined icon-sm">edit</span>
                </button>
                <button class="btn-icon-sm danger" type="button" title="Delete OAuth app" (click)="deleteProviderApp(app)">
                  <span class="material-symbols-outlined icon-sm">delete</span>
                </button>
              </div>
            </div>
          } @empty {
            <div class="provider-app-empty">
              Add at least one OAuth app to connect Google Drive or OneDrive accounts.
            </div>
          }
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
      [providerApps]="providerApps"
      (close)="showAddModal.set(false)"
      (profileAdded)="onProfileAdded($event)"
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

    .oauth-banner {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 10px 12px;
      border-radius: var(--radius);
      border: 1px solid var(--color-outline-variant);
      font-size: 13px;
      font-weight: 500;
      margin-bottom: 16px;

      &.success {
        background: #eaf7ef;
        border-color: #b6dfc4;
        color: #1a7a4a;
      }

      &.error {
        background: #fdecec;
        border-color: #f4b8b8;
        color: var(--color-error);
      }
    }

    .provider-apps-panel {
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius-md);
      background: var(--color-surface-container-lowest);
      padding: 14px;
      margin-bottom: 18px;
    }

    .panel-header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 16px;
      margin-bottom: 12px;

      h2 {
        font-size: 16px;
        font-weight: 600;
        margin-bottom: 4px;
      }

      p {
        color: var(--color-outline);
        font-size: 13px;
      }
    }

    .provider-app-form {
      display: grid;
      gap: 12px;
      padding: 12px;
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius);
      background: var(--color-surface-container-low);
      margin-bottom: 12px;

      label {
        display: grid;
        gap: 6px;
        font-size: 12px;
        font-weight: 600;
        color: var(--color-on-surface-variant);
      }
    }

    .form-row {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 12px;
    }

    .code-input {
      font-family: ui-monospace, SFMono-Regular, Consolas, monospace;
      font-size: 12px;
    }

    .form-actions {
      display: flex;
      justify-content: flex-end;
    }

    .provider-app-list {
      display: grid;
      gap: 8px;
    }

    .provider-app-row {
      display: grid;
      grid-template-columns: 24px minmax(180px, 1fr) minmax(240px, 1.5fr) auto;
      gap: 10px;
      align-items: center;
      padding: 10px;
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius);
      background: var(--color-surface-container-low);

      p {
        color: var(--color-outline);
        font-size: 12px;
        margin-top: 2px;
      }

      code {
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
        font-size: 12px;
        color: var(--color-on-surface-variant);
      }

      &.inactive {
        opacity: 0.72;
      }
    }

    .provider-app-actions {
      display: flex;
      gap: 4px;
    }

    .checkbox-row {
      display: flex !important;
      grid-template-columns: none !important;
      align-items: center;
      gap: 8px !important;
    }

    .provider-app-empty {
      padding: 14px;
      border: 1px dashed var(--color-outline-variant);
      border-radius: var(--radius);
      color: var(--color-outline);
      font-size: 13px;
      text-align: center;
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

      &.danger:hover { color: var(--color-error); }
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
  providerApps: IOAuthProviderApp[] = [];
  searchQuery = '';
  activeFilter = 'all';
  viewMode: 'grid' | 'list' = 'grid';
  showAddModal = signal(false);
  oauthStatus = signal<'success' | 'error' | null>(null);
  oauthReason = signal<string | null>(null);
  showProviderAppForm = false;
  isSavingProviderApp = false;
  editingProviderAppId: string | null = null;
  editingProviderLinkedProfileCount = 0;
  providerAppForm: UpdateOAuthProviderAppFormValue = this.createDefaultProviderAppForm('google-drive');

  statusFilters = [
    { value: 'all',    label: 'All' },
    { value: 'active', label: 'Active' },
    { value: 'idle',   label: 'Idle' },
    { value: 'error',  label: 'Error' },
  ];

  constructor(
    private api: CloudShiftApiService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    this.handleOAuthRedirect();
    this.refresh();
    this.refreshProviderApps();
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

  get isProviderChangeLocked(): boolean {
    return this.editingProviderAppId !== null && this.editingProviderLinkedProfileCount > 0;
  }

  getFilterCount(filter: string): number {
    if (filter === 'all') return this.profiles.length;
    return this.profiles.filter(p => p.status === filter).length;
  }

  refresh() {
    this.api.getAppProfiles().subscribe({
      next: profiles => this.profiles = profiles,
      error: error => console.error('Failed to load app profiles', error)
    });
  }

  refreshProviderApps() {
    this.api.getOAuthProviderApps().subscribe({
      next: apps => this.providerApps = apps,
      error: error => console.error('Failed to load OAuth provider apps', error)
    });
  }

  toggleProviderAppForm() {
    this.showProviderAppForm = !this.showProviderAppForm;
    if (this.showProviderAppForm) {
      this.editingProviderAppId = null;
      this.editingProviderLinkedProfileCount = 0;
      this.providerAppForm = this.createDefaultProviderAppForm('google-drive');
    }
  }

  onProviderAppProviderChanged(provider: CreateOAuthProviderAppFormValue['provider']) {
    this.providerAppForm = {
      ...this.providerAppForm,
      provider,
      tenantId: provider === 'onedrive' ? 'common' : '',
      redirectUri: this.api.getSuggestedOAuthRedirectUri(provider),
      scopes: this.defaultScopes(provider)
    };
  }

  saveProviderApp() {
    this.isSavingProviderApp = true;
    const request = this.editingProviderAppId
      ? this.api.updateOAuthProviderApp(this.editingProviderAppId, this.providerAppForm)
      : this.api.createOAuthProviderApp(this.providerAppForm as CreateOAuthProviderAppFormValue);

    request.subscribe({
      next: () => {
        this.isSavingProviderApp = false;
        this.showProviderAppForm = false;
        this.editingProviderAppId = null;
        this.editingProviderLinkedProfileCount = 0;
        this.providerAppForm = this.createDefaultProviderAppForm('google-drive');
        this.refreshProviderApps();
      },
      error: error => {
        this.isSavingProviderApp = false;
        console.error('Failed to save OAuth provider app', error);
      }
    });
  }

  editProviderApp(app: IOAuthProviderApp) {
    this.editingProviderAppId = app.id;
    this.editingProviderLinkedProfileCount = app.linkedProfileCount;
    this.providerAppForm = {
      provider: app.provider,
      name: app.name,
      clientId: app.clientId,
      clientSecret: '',
      tenantId: app.provider === 'onedrive' ? app.tenantId || 'common' : '',
      redirectUri: app.redirectUri,
      scopes: app.scopes,
      isActive: app.isActive
    };
    this.showProviderAppForm = true;
  }

  deleteProviderApp(app: IOAuthProviderApp) {
    const message = app.linkedProfileCount > 0
      ? `This OAuth app has ${app.linkedProfileCount} linked profile(s). It will be deactivated, not deleted. Continue?`
      : 'Delete this OAuth app?';

    if (!window.confirm(message)) {
      return;
    }

    this.api.deleteOAuthProviderApp(app.id).subscribe({
      next: () => this.refreshProviderApps(),
      error: error => console.error('Failed to delete OAuth provider app', error)
    });
  }

  onProfileAdded(profile: AddProfileFormValue) {
    this.api.startAppProfileOAuth(profile);
  }

  providerLabel(provider: IOAuthProviderApp['provider']): string {
    return provider === 'onedrive' ? 'OneDrive' : 'Google Drive';
  }

  private handleOAuthRedirect() {
    const oauth = this.route.snapshot.queryParamMap.get('oauth');
    const oauthReason = this.route.snapshot.queryParamMap.get('oauthReason');
    if (oauth !== 'success' && oauth !== 'error') {
      return;
    }

    this.oauthStatus.set(oauth);
    this.oauthReason.set(oauthReason);
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { oauth: null, oauthReason: null },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  oauthErrorMessage(): string {
    if (this.oauthReason() === 'invalid_client') {
      return 'Provider authorization failed: invalid client secret value.';
    }

    return 'Provider authorization failed or was cancelled.';
  }

  private createDefaultProviderAppForm(provider: CreateOAuthProviderAppFormValue['provider']): UpdateOAuthProviderAppFormValue {
    return {
      provider,
      name: provider === 'onedrive' ? 'Microsoft OneDrive OAuth App' : 'Google Drive OAuth App',
      clientId: '',
      clientSecret: '',
      tenantId: provider === 'onedrive' ? 'common' : '',
      redirectUri: this.api.getSuggestedOAuthRedirectUri(provider),
      scopes: this.defaultScopes(provider),
      isActive: true
    };
  }

  private defaultScopes(provider: CreateOAuthProviderAppFormValue['provider']): string {
    return provider === 'onedrive'
      ? 'offline_access User.Read Files.ReadWrite'
      : 'openid email profile https://www.googleapis.com/auth/drive';
  }
}
