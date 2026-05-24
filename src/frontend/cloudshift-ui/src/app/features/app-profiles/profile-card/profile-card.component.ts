import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IAppProfile, CloudProvider } from '../../../models/app-profile.model';
import { StatusBadgeComponent } from '../../../shared/components/status-badge/status-badge.component';
import { ProgressBarComponent } from '../../../shared/components/progress-bar/progress-bar.component';

@Component({
  selector: 'app-profile-card',
  standalone: true,
  imports: [CommonModule, StatusBadgeComponent, ProgressBarComponent],
  template: `
    <div class="profile-card" [class.error]="profile.status === 'error'">
      <div class="card-header">
        <div class="provider-icon" [ngClass]="profile.provider">
          <span class="material-symbols-outlined icon-lg">{{ providerIcon }}</span>
        </div>
        <div class="card-title-group">
          <div class="profile-name">{{ profile.name }}</div>
          <div class="profile-email">{{ profile.email }}</div>
        </div>
        <app-status-badge [status]="profile.status" />
      </div>

      <div class="card-stats">
        <div class="stat-item">
          <div class="stat-val">{{ profile.filesCount | number }}</div>
          <div class="stat-lbl">Files</div>
        </div>
        <div class="stat-divider"></div>
        <div class="stat-item">
          <div class="stat-val">{{ profile.storageUsedGB }} GB</div>
          <div class="stat-lbl">Used</div>
        </div>
        <div class="stat-divider"></div>
        <div class="stat-item">
          <div class="stat-val">{{ profile.storageTotalGB }} GB</div>
          <div class="stat-lbl">Total</div>
        </div>
      </div>

      <div class="storage-bar">
        <app-progress-bar
          [value]="storagePercent"
          [showLabel]="true"
          label="Storage Used"
          [metaLeft]="profile.storageUsedGB + ' GB / ' + profile.storageTotalGB + ' GB'"
          [metaRight]="storagePercent + '%'"
          [variant]="storagePercent > 90 ? 'error' : storagePercent > 75 ? 'warning' : 'default'"
        />
      </div>

      <div class="card-footer">
        <span class="last-sync">
          <span class="material-symbols-outlined icon-sm" style="color:var(--color-outline)">schedule</span>
          Synced {{ profile.lastSync | date:'MMM d, h:mm a' }}
        </span>
        <div class="card-actions">
          <button class="btn-icon" title="Sync now" aria-label="Sync now">
            <span class="material-symbols-outlined icon-sm">sync</span>
          </button>
          <button class="btn-icon" title="Edit profile" aria-label="Edit profile">
            <span class="material-symbols-outlined icon-sm">edit</span>
          </button>
          <button class="btn-icon danger" title="Disconnect" aria-label="Disconnect">
            <span class="material-symbols-outlined icon-sm">link_off</span>
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .profile-card {
      background: var(--color-surface-container-lowest);
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius-lg);
      padding: 20px;
      display: flex;
      flex-direction: column;
      gap: 16px;
      transition: box-shadow 0.2s ease, border-color 0.2s ease;

      &:hover { box-shadow: var(--shadow-md); }
      &.error { border-color: var(--color-error); border-left: 3px solid var(--color-error); }
    }

    .card-header {
      display: flex;
      align-items: flex-start;
      gap: 12px;
    }

    .provider-icon {
      width: 44px;
      height: 44px;
      border-radius: var(--radius-md);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;

      &.google-workspace { background: #e8f0fe; color: #1a73e8; }
      &.google-drive     { background: #e6f4ea; color: #34a853; }
      &.onedrive         { background: #e3f2fd; color: #0078d4; }
      &.sharepoint       { background: #e3f2fd; color: #0078d4; }
      &.dropbox          { background: #e8f0fe; color: #0061ff; }
      &.box              { background: #e3f2fd; color: #0061d5; }
      &.s3               { background: #fff3e0; color: #ff9800; }
    }

    .card-title-group { flex: 1; min-width: 0; }

    .profile-name {
      font-size: 14px;
      font-weight: 600;
      color: var(--color-on-surface);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .profile-email {
      font-size: 12px;
      color: var(--color-outline);
      margin-top: 2px;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .card-stats {
      display: flex;
      align-items: center;
      background: var(--color-surface-container-low);
      border-radius: var(--radius-md);
      padding: 10px 0;
    }

    .stat-item {
      flex: 1;
      text-align: center;

      .stat-val { font-size: 15px; font-weight: 700; color: var(--color-on-surface); }
      .stat-lbl { font-size: 11px; color: var(--color-outline); margin-top: 2px; }
    }

    .stat-divider { width: 1px; height: 28px; background: var(--color-outline-variant); }

    .card-footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
    }

    .last-sync {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 12px;
      color: var(--color-outline);
    }

    .card-actions { display: flex; gap: 4px; }

    .btn-icon {
      width: 28px;
      height: 28px;
      border-radius: var(--radius);
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--color-on-surface-variant);
      transition: background 0.15s, color 0.15s;

      &:hover { background: var(--color-surface-container); color: var(--color-on-surface); }
      &.danger:hover { background: var(--color-error-container); color: var(--color-error); }
    }
  `]
})
export class ProfileCardComponent {
  @Input({ required: true }) profile!: IAppProfile;

  get storagePercent(): number {
    return Math.round((this.profile.storageUsedGB / this.profile.storageTotalGB) * 100);
  }

  get providerIcon(): string {
    const icons: Record<CloudProvider, string> = {
      'google-workspace': 'work',
      'google-drive': 'add_to_drive',
      'onedrive': 'cloud',
      'sharepoint': 'share',
      'dropbox': 'cloud_download',
      'box': 'inventory_2',
      's3': 'storage'
    };
    return icons[this.profile.provider] || 'cloud';
  }
}
