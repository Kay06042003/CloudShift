import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="stat-card">
      <div class="stat-icon-wrap" [ngClass]="iconColor">
        <span class="material-symbols-outlined icon-lg">{{ icon }}</span>
      </div>
      <div class="stat-body">
        <div class="stat-value">{{ value }}</div>
        <div class="stat-title">{{ title }}</div>
      </div>
      <div class="stat-trend" *ngIf="trend !== undefined" [ngClass]="trend >= 0 ? 'up' : 'down'">
        <span class="material-symbols-outlined icon-sm">
          {{ trend >= 0 ? 'trending_up' : 'trending_down' }}
        </span>
        {{ trend >= 0 ? '+' : '' }}{{ trend }}%
      </div>
    </div>
  `,
  styles: [`
    .stat-card {
      background: var(--color-surface-container-lowest);
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius-lg);
      padding: 20px;
      display: flex;
      align-items: flex-start;
      gap: 16px;
      transition: box-shadow 0.2s ease;

      &:hover { box-shadow: var(--shadow-md); }
    }

    .stat-icon-wrap {
      width: 48px;
      height: 48px;
      border-radius: var(--radius-lg);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;

      &.blue   { background: var(--color-primary-fixed); color: var(--color-primary); }
      &.green  { background: #e6f4ed; color: #1a7a4a; }
      &.red    { background: var(--color-error-container); color: var(--color-error); }
      &.orange { background: #fff3e0; color: #e67700; }
      &.purple { background: #f0ebff; color: #6c3fc8; }
    }

    .stat-body { flex: 1; }

    .stat-value {
      font-size: 24px;
      font-weight: 700;
      color: var(--color-on-surface);
      line-height: 1.2;
    }

    .stat-title {
      font-size: 13px;
      color: var(--color-on-surface-variant);
      margin-top: 2px;
      font-weight: 500;
    }

    .stat-trend {
      display: flex;
      align-items: center;
      gap: 2px;
      font-size: 12px;
      font-weight: 500;
      margin-top: 4px;
      padding: 2px 6px;
      border-radius: 4px;

      &.up   { background: #e6f4ed; color: #1a7a4a; }
      &.down { background: var(--color-error-container); color: var(--color-error); }
    }
  `]
})
export class StatCardComponent {
  @Input() title: string = '';
  @Input() value: string = '';
  @Input() icon: string = 'bar_chart';
  @Input() iconColor: 'blue' | 'green' | 'red' | 'orange' | 'purple' = 'blue';
  @Input() trend?: number;
}
