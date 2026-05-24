import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ModalComponent } from '../../../shared/components/modal/modal.component';
import { CloudProvider } from '../../../models/app-profile.model';

interface ProviderOption {
  value: CloudProvider;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-add-profile-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ModalComponent],
  template: `
    <app-modal
      title="Connect Cloud Storage Profile"
      [isOpen]="isOpen"
      maxWidth="580px"
      (close)="onClose()"
    >
      <form [formGroup]="form" (ngSubmit)="onSubmit()" id="add-profile-form">
        <!-- Provider Selection -->
        <div class="section-label">1. Select Provider</div>
        <div class="provider-grid">
          @for (p of providers; track p.value) {
            <button
              type="button"
              class="provider-option"
              [class.selected]="form.get('provider')?.value === p.value"
              (click)="selectProvider(p.value)"
            >
              <span class="material-symbols-outlined">{{ p.icon }}</span>
              <span>{{ p.label }}</span>
            </button>
          }
        </div>

        <div class="form-divider"></div>

        <!-- Profile Details -->
        <div class="section-label">2. Profile Details</div>
        <div class="form-grid">
          <div class="form-group">
            <label for="profile-name">Profile Name *</label>
            <input
              id="profile-name"
              type="text"
              class="cs-input"
              formControlName="name"
              placeholder="e.g. Engineering Google Workspace"
              [class.error]="form.get('name')?.invalid && form.get('name')?.touched"
            />
            <span class="error-text" *ngIf="form.get('name')?.invalid && form.get('name')?.touched">
              Profile name is required
            </span>
          </div>

          <div class="form-group">
            <label for="profile-email">Account Email / Identifier *</label>
            <input
              id="profile-email"
              type="email"
              class="cs-input"
              formControlName="email"
              placeholder="e.g. admin@company.com"
              [class.error]="form.get('email')?.invalid && form.get('email')?.touched"
            />
            <span class="error-text" *ngIf="form.get('email')?.invalid && form.get('email')?.touched">
              Valid email or identifier is required
            </span>
          </div>

          <div class="form-group">
            <label for="storage-quota">Storage Quota (GB)</label>
            <input
              id="storage-quota"
              type="number"
              class="cs-input"
              formControlName="storageTotalGB"
              placeholder="e.g. 1000"
              min="1"
            />
          </div>

          <div class="form-group">
            <label for="auth-method">Authentication Method</label>
            <select id="auth-method" class="cs-select" formControlName="authMethod">
              <option value="oauth2">OAuth 2.0 (Recommended)</option>
              <option value="service-account">Service Account Key (JSON)</option>
              <option value="api-key">API Key</option>
            </select>
          </div>
        </div>

        @if (form.get('authMethod')?.value === 'oauth2') {
          <div class="oauth-notice">
            <span class="material-symbols-outlined icon-sm" style="color:var(--color-primary)">info</span>
            You will be redirected to authorize access via OAuth 2.0 after saving.
          </div>
        }
      </form>

      <div modal-footer>
        <button type="button" class="btn-secondary" (click)="onClose()" id="cancel-profile-btn">
          Cancel
        </button>
        <button
          type="submit"
          form="add-profile-form"
          class="btn-primary"
          id="save-profile-btn"
          [disabled]="form.invalid"
        >
          <span class="material-symbols-outlined icon-sm">add_link</span>
          Connect Profile
        </button>
      </div>
    </app-modal>
  `,
  styles: [`
    .section-label {
      font-size: 12px;
      font-weight: 600;
      color: var(--color-on-surface-variant);
      text-transform: uppercase;
      letter-spacing: 0.06em;
      margin-bottom: 10px;
    }

    .provider-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 8px;
      margin-bottom: 4px;
    }

    .provider-option {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 6px;
      padding: 12px 8px;
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius-md);
      background: var(--color-surface-container-low);
      color: var(--color-on-surface-variant);
      font-size: 12px;
      font-weight: 500;
      transition: all 0.15s ease;
      cursor: pointer;

      &:hover {
        border-color: var(--color-primary-fixed-dim);
        background: var(--color-primary-fixed);
        color: var(--color-primary);
      }

      &.selected {
        border-color: var(--color-primary);
        background: var(--color-primary-fixed);
        color: var(--color-primary);
        font-weight: 600;
      }
    }

    .form-divider { height: 1px; background: var(--color-outline-variant); margin: 16px 0; }

    .form-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 14px;
    }

    .oauth-notice {
      display: flex;
      align-items: center;
      gap: 8px;
      background: var(--color-primary-fixed);
      border: 1px solid var(--color-primary-fixed-dim);
      border-radius: var(--radius-md);
      padding: 10px 12px;
      font-size: 13px;
      color: var(--color-primary-dark);
      margin-top: 12px;
    }

    .btn-primary {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 8px 16px;
      background: var(--color-primary);
      color: var(--color-on-primary);
      border-radius: var(--radius);
      font-size: 14px;
      font-weight: 500;
      transition: background 0.15s;

      &:hover:not(:disabled) { background: var(--color-primary-dark); }
      &:disabled { opacity: 0.5; cursor: not-allowed; }
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
  `]
})
export class AddProfileModalComponent {
  @Input() isOpen = false;
  @Output() close = new EventEmitter<void>();
  @Output() profileAdded = new EventEmitter<void>();

  form: FormGroup;

  providers: ProviderOption[] = [
    { value: 'google-workspace', label: 'Google Workspace', icon: 'work' },
    { value: 'google-drive',     label: 'Google Drive',     icon: 'add_to_drive' },
    { value: 'onedrive',         label: 'OneDrive',         icon: 'cloud' },
    { value: 'sharepoint',       label: 'SharePoint',       icon: 'share' },
    { value: 'dropbox',          label: 'Dropbox',          icon: 'cloud_download' },
    { value: 's3',               label: 'AWS S3',           icon: 'storage' },
  ];

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      provider:      ['google-workspace', Validators.required],
      name:          ['', Validators.required],
      email:         ['', [Validators.required, Validators.email]],
      storageTotalGB:[1000],
      authMethod:    ['oauth2']
    });
  }

  selectProvider(value: CloudProvider) {
    this.form.patchValue({ provider: value });
  }

  onSubmit() {
    if (this.form.valid) {
      this.profileAdded.emit();
      this.onClose();
    } else {
      this.form.markAllAsTouched();
    }
  }

  onClose() {
    this.form.reset({ provider: 'google-workspace', authMethod: 'oauth2', storageTotalGB: 1000 });
    this.close.emit();
  }
}
