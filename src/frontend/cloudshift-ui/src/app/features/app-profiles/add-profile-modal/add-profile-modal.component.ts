import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ModalComponent } from '../../../shared/components/modal/modal.component';
import { AddProfileFormValue, IOAuthProviderApp } from '../../../services/cloudshift-api.service';

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
        <div class="section-label">1. Select Provider</div>
        <div class="provider-grid">
          <button
            type="button"
            class="provider-option"
            [class.selected]="form.get('provider')?.value === 'google-drive'"
            (click)="selectProvider('google-drive')"
          >
            <span class="material-symbols-outlined">add_to_drive</span>
            <span>Google Drive</span>
          </button>
          <button
            type="button"
            class="provider-option"
            [class.selected]="form.get('provider')?.value === 'onedrive'"
            (click)="selectProvider('onedrive')"
          >
            <span class="material-symbols-outlined">cloud</span>
            <span>OneDrive</span>
          </button>
        </div>

        <div class="form-divider"></div>

        <div class="section-label">2. Select OAuth App</div>
        <div class="provider-grid">
          @for (p of filteredProviderApps; track p.id) {
            <button
              type="button"
              class="provider-option"
              [class.selected]="form.get('providerAppId')?.value === p.id"
              (click)="selectProviderApp(p.id)"
            >
              <span class="material-symbols-outlined">{{ p.provider === 'onedrive' ? 'cloud' : 'add_to_drive' }}</span>
              <span>{{ p.name }}</span>
              <small>{{ p.clientId }}</small>
            </button>
          } @empty {
            <div class="empty-provider-apps">
              No OAuth app is configured for this provider. Add one in OAuth Provider Apps first.
            </div>
          }
        </div>

        <div class="oauth-notice">
          <span class="material-symbols-outlined icon-sm" style="color:var(--color-primary)">info</span>
          Google/Microsoft will ask you to choose the account to connect. CloudShift stores encrypted account tokens after authorization.
        </div>
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
          [disabled]="isConnectDisabled"
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
      grid-template-columns: repeat(2, minmax(0, 1fr));
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

      small {
        color: var(--color-outline);
        font-size: 11px;
      }

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

    .empty-provider-apps {
      grid-column: 1 / -1;
      padding: 18px;
      border: 1px dashed var(--color-outline-variant);
      border-radius: var(--radius-md);
      color: var(--color-outline);
      font-size: 13px;
      text-align: center;
    }

    .form-divider { height: 1px; background: var(--color-outline-variant); margin: 16px 0; }

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
export class AddProfileModalComponent implements OnChanges {
  @Input() isOpen = false;
  @Input() providerApps: IOAuthProviderApp[] = [];
  @Output() close = new EventEmitter<void>();
  @Output() profileAdded = new EventEmitter<AddProfileFormValue>();

  form: FormGroup;

  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      provider: ['google-drive', Validators.required],
      providerAppId: ['', Validators.required]
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['isOpen'] && this.isOpen) {
      this.syncSelectedProviderApp();
    }
  }

  get filteredProviderApps(): IOAuthProviderApp[] {
    return this.providerApps.filter(app => app.provider === this.form.get('provider')?.value && app.isActive);
  }

  get isConnectDisabled(): boolean {
    if (this.form.invalid) {
      return true;
    }

    return !this.form.get('providerAppId')?.value;
  }

  selectProvider(provider: IOAuthProviderApp['provider']) {
    this.form.patchValue({ provider });
    this.syncSelectedProviderApp();
  }

  selectProviderApp(providerAppId: string) {
    this.form.patchValue({ providerAppId });
  }

  onSubmit() {
    if (this.form.valid) {
      this.profileAdded.emit(this.form.getRawValue() as AddProfileFormValue);
      this.onClose();
    } else {
      this.form.markAllAsTouched();
    }
  }

  onClose() {
    this.form.reset({ provider: 'google-drive', providerAppId: '' });
    this.close.emit();
  }

  providerLabel(provider: IOAuthProviderApp['provider']): string {
    return provider === 'onedrive' ? 'OneDrive' : 'Google Drive';
  }

  private syncSelectedProviderApp() {
    const selectedProvider = this.form.get('provider')?.value;
    const currentProviderAppId = this.form.get('providerAppId')?.value;
    const currentStillValid = this.providerApps.some(app =>
      app.id === currentProviderAppId && app.provider === selectedProvider && app.isActive);

    if (!currentStillValid) {
      this.form.patchValue({ providerAppId: this.filteredProviderApps[0]?.id ?? '' });
    }
  }
}
