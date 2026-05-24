import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <h1 class="page-title">Settings</h1>
          <p class="page-subtitle">Configure system preferences, notifications, and security policies.</p>
        </div>
      </div>

      <div class="settings-layout">
        <!-- Settings Sidebar Nav -->
        <div class="settings-nav">
          @for (section of sections; track section.id) {
            <button
              class="settings-nav-item"
              [class.active]="activeSection === section.id"
              (click)="activeSection = section.id"
              [id]="'settings-nav-' + section.id"
            >
              <span class="material-symbols-outlined icon-sm">{{ section.icon }}</span>
              {{ section.label }}
            </button>
          }
        </div>

        <!-- Settings Content -->
        <div class="settings-content">
          <!-- General -->
          @if (activeSection === 'general') {
            <div class="settings-panel animate-fade-in">
              <h2 class="panel-title">General Settings</h2>

              <div class="settings-group">
                <div class="settings-row">
                  <div class="settings-info">
                    <div class="settings-label">Organization Name</div>
                    <div class="settings-desc">Displayed in reports and email notifications.</div>
                  </div>
                  <input type="text" class="cs-input settings-input" value="Acme Corp" id="org-name" />
                </div>
                <div class="settings-row">
                  <div class="settings-info">
                    <div class="settings-label">Default Job Priority</div>
                    <div class="settings-desc">Priority assigned to new jobs unless overridden.</div>
                  </div>
                  <select class="cs-select settings-input" id="default-priority">
                    <option>Normal</option>
                    <option>High</option>
                    <option>Low</option>
                  </select>
                </div>
                <div class="settings-row">
                  <div class="settings-info">
                    <div class="settings-label">Timezone</div>
                    <div class="settings-desc">Used for scheduling and log timestamps.</div>
                  </div>
                  <select class="cs-select settings-input" id="timezone">
                    <option>Asia/Ho_Chi_Minh (UTC+7)</option>
                    <option>UTC</option>
                    <option>America/New_York (UTC-5)</option>
                  </select>
                </div>
              </div>
            </div>
          }

          <!-- Notifications -->
          @if (activeSection === 'notifications') {
            <div class="settings-panel animate-fade-in">
              <h2 class="panel-title">Notification Preferences</h2>
              <div class="settings-group">
                @for (notif of notifications; track notif.id) {
                  <div class="settings-row">
                    <div class="settings-info">
                      <div class="settings-label">{{ notif.label }}</div>
                      <div class="settings-desc">{{ notif.desc }}</div>
                    </div>
                    <label class="toggle-switch">
                      <input type="checkbox" [checked]="notif.enabled" [id]="'notif-' + notif.id">
                      <span class="toggle-slider"></span>
                    </label>
                  </div>
                }
              </div>
            </div>
          }

          <!-- Security -->
          @if (activeSection === 'security') {
            <div class="settings-panel animate-fade-in">
              <h2 class="panel-title">Security &amp; Access Control</h2>
              <div class="settings-group">
                <div class="settings-row">
                  <div class="settings-info">
                    <div class="settings-label">Two-Factor Authentication</div>
                    <div class="settings-desc">Require 2FA for all admin accounts.</div>
                  </div>
                  <label class="toggle-switch">
                    <input type="checkbox" checked id="2fa-toggle">
                    <span class="toggle-slider"></span>
                  </label>
                </div>
                <div class="settings-row">
                  <div class="settings-info">
                    <div class="settings-label">API Key</div>
                    <div class="settings-desc">Used for external integrations.</div>
                  </div>
                  <div class="api-key-row">
                    <input type="password" class="cs-input settings-input" value="cm_sk_prod_abc123xyz" id="api-key" readonly />
                    <button class="btn-secondary-sm" id="rotate-key-btn">Rotate Key</button>
                  </div>
                </div>
                <div class="settings-row">
                  <div class="settings-info">
                    <div class="settings-label">Audit Log Retention</div>
                    <div class="settings-desc">How long to store detailed audit logs.</div>
                  </div>
                  <select class="cs-select settings-input" id="log-retention">
                    <option>90 days</option>
                    <option>180 days</option>
                    <option>1 year</option>
                    <option>Indefinitely</option>
                  </select>
                </div>
              </div>
            </div>
          }

          <div class="panel-footer">
            <button class="btn-secondary" id="discard-settings-btn">Discard Changes</button>
            <button class="btn-primary" id="save-settings-btn">Save Settings</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .settings-layout {
      display: grid;
      grid-template-columns: 200px 1fr;
      gap: 24px;
      align-items: start;
    }

    .settings-nav {
      background: var(--color-surface-container-lowest);
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius-lg);
      padding: 8px;
      display: flex;
      flex-direction: column;
      gap: 2px;
      position: sticky;
      top: calc(var(--header-height) + 24px);
    }

    .settings-nav-item {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 9px 12px;
      border-radius: var(--radius-md);
      font-size: 13px;
      font-weight: 500;
      color: var(--color-on-surface-variant);
      transition: background 0.15s, color 0.15s;
      cursor: pointer;
      text-align: left;

      &:hover { background: var(--color-surface-container); color: var(--color-on-surface); }
      &.active { background: var(--color-primary-fixed); color: var(--color-primary); font-weight: 600; }
    }

    .settings-panel {
      background: var(--color-surface-container-lowest);
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius-lg);
      overflow: hidden;
    }

    .panel-title {
      font-size: 16px;
      font-weight: 600;
      padding: 20px 24px;
      border-bottom: 1px solid var(--color-outline-variant);
    }

    .settings-group { }

    .settings-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 24px;
      padding: 16px 24px;
      border-bottom: 1px solid var(--color-surface-container);

      &:last-child { border-bottom: none; }
    }

    .settings-info { flex: 1; }
    .settings-label { font-size: 14px; font-weight: 500; color: var(--color-on-surface); }
    .settings-desc { font-size: 12px; color: var(--color-outline); margin-top: 2px; }

    .settings-input { width: 240px; }

    .api-key-row {
      display: flex;
      gap: 8px;
      align-items: center;

      .settings-input { width: 200px; font-family: var(--font-mono); font-size: 12px; }
    }

    .btn-secondary-sm {
      padding: 5px 12px;
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius);
      font-size: 12px;
      font-weight: 500;
      color: var(--color-on-surface-variant);
      background: var(--color-surface-container-lowest);
      transition: background 0.15s;
      white-space: nowrap;

      &:hover { background: var(--color-surface-container); }
    }

    .panel-footer {
      display: flex;
      justify-content: flex-end;
      gap: 10px;
      padding: 16px 24px;
      margin-top: 16px;
    }

    // Toggle
    .toggle-switch {
      position: relative;
      display: inline-block;
      width: 40px;
      height: 22px;
      flex-shrink: 0;

      input { opacity: 0; width: 0; height: 0; }
    }

    .toggle-slider {
      position: absolute;
      cursor: pointer;
      inset: 0;
      background: var(--color-surface-container-high);
      border-radius: 999px;
      transition: 0.2s;

      &::before {
        content: '';
        position: absolute;
        width: 16px;
        height: 16px;
        left: 3px;
        top: 3px;
        background: white;
        border-radius: 50%;
        transition: 0.2s;
      }
    }

    input:checked + .toggle-slider { background: var(--color-primary); }
    input:checked + .toggle-slider::before { transform: translateX(18px); }

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
      padding: 8px 16px;
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius);
      font-size: 14px;
      font-weight: 500;
      color: var(--color-on-surface-variant);
      background: var(--color-surface-container-lowest);
      transition: background 0.15s;

      &:hover { background: var(--color-surface-container); }
    }

    @media (max-width: 768px) {
      .settings-layout { grid-template-columns: 1fr; }
      .settings-nav { position: static; }
    }
  `]
})
export class SettingsComponent {
  activeSection = 'general';

  sections = [
    { id: 'general',       label: 'General',       icon: 'settings' },
    { id: 'notifications', label: 'Notifications',  icon: 'notifications' },
    { id: 'security',      label: 'Security',       icon: 'security' },
  ];

  notifications = [
    { id: 'job-complete', label: 'Job Completed',     desc: 'Receive an email when a migration job finishes.', enabled: true },
    { id: 'job-failed',   label: 'Job Failed',        desc: 'Alert when a job encounters a critical error.',   enabled: true },
    { id: 'high-error',   label: 'High Error Rate',   desc: 'Warn when error rate exceeds 5% threshold.',      enabled: false },
    { id: 'storage-warn', label: 'Storage Warning',   desc: 'Alert when destination storage is over 85%.',     enabled: true },
    { id: 'weekly-digest',label: 'Weekly Digest',     desc: 'Summary email of all activity each Monday.',      enabled: false },
  ];
}
