import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-progress-bar',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="progress-container">
      <div class="progress-header" *ngIf="showLabel">
        <span class="progress-label">{{ label }}</span>
        <span class="progress-value">{{ value }}%</span>
      </div>
      <div class="progress-track">
        <div class="progress-fill" [style.width.%]="value" [ngClass]="colorClass"></div>
      </div>
      <div class="progress-meta" *ngIf="metaLeft || metaRight">
        <span class="meta-left">{{ metaLeft }}</span>
        <span class="meta-right">{{ metaRight }}</span>
      </div>
    </div>
  `,
  styles: [`
    .progress-container { display: flex; flex-direction: column; gap: 4px; }

    .progress-header {
      display: flex;
      justify-content: space-between;
      align-items: center;

      .progress-label { font-size: 12px; font-weight: 500; color: var(--color-on-surface-variant); }
      .progress-value { font-size: 12px; font-weight: 600; color: var(--color-on-surface); }
    }

    .progress-track {
      height: 6px;
      background: var(--color-surface-container-high);
      border-radius: 9999px;
      overflow: hidden;
    }

    .progress-fill {
      height: 100%;
      border-radius: 9999px;
      background: var(--color-primary);
      transition: width 0.6s cubic-bezier(0.4, 0, 0.2, 1);

      &.success { background: #1a7a4a; }
      &.warning { background: #e67700; }
      &.error   { background: var(--color-error); }
    }

    .progress-meta {
      display: flex;
      justify-content: space-between;
      font-size: 11px;
      color: var(--color-outline);
    }
  `]
})
export class ProgressBarComponent {
  @Input() value: number = 0;
  @Input() label?: string;
  @Input() showLabel: boolean = true;
  @Input() metaLeft?: string;
  @Input() metaRight?: string;
  @Input() variant: 'default' | 'success' | 'warning' | 'error' = 'default';

  get colorClass(): string {
    if (this.value === 100) return 'success';
    if (this.variant !== 'default') return this.variant;
    return '';
  }
}
