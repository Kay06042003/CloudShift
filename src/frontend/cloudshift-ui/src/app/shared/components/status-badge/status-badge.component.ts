import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type BadgeStatus =
  | 'active' | 'idle' | 'error' | 'connecting'
  | 'running' | 'completed' | 'failed' | 'pending' | 'paused' | 'queued'
  | 'draft' | 'archived';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <span class="badge" [ngClass]="status">
      <span class="badge-dot"></span>
      {{ label || statusLabel }}
    </span>
  `,
  styles: [`
    :host { display: inline-flex; }

    .badge {
      display: inline-flex;
      align-items: center;
      gap: 5px;
      padding: 2px 8px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 500;
      line-height: 18px;
      white-space: nowrap;
    }

    .badge-dot {
      width: 6px;
      height: 6px;
      border-radius: 50%;
      flex-shrink: 0;
    }

    .active    { background: #e6f4ed; color: #1a7a4a; .badge-dot { background: #1a7a4a; } }
    .completed { background: #e6f4ed; color: #1a7a4a; .badge-dot { background: #1a7a4a; } }
    .idle      { background: #f0f1f3; color: #414755; .badge-dot { background: #727786; } }
    .paused    { background: #f0f1f3; color: #414755; .badge-dot { background: #727786; } }
    .queued    { background: #f0f1f3; color: #414755; .badge-dot { background: #727786; } }
    .draft     { background: #f0f1f3; color: #414755; .badge-dot { background: #727786; } }
    .archived  { background: #f0f1f3; color: #6b7280; .badge-dot { background: #9ca3af; } }
    .error     { background: #ffdad6; color: #93000a; .badge-dot { background: #ba1a1a; } }
    .failed    { background: #ffdad6; color: #93000a; .badge-dot { background: #ba1a1a; } }
    .running   { background: #e3eefa; color: #004398; .badge-dot { background: #0057c2; animation: pulse 1.5s infinite; } }
    .pending   { background: #fff3e0; color: #a06000; .badge-dot { background: #e67700; } }
    .connecting{ background: #fff3e0; color: #a06000; .badge-dot { background: #e67700; animation: pulse 1.5s infinite; } }

    @keyframes pulse {
      0%, 100% { opacity: 1; transform: scale(1); }
      50% { opacity: 0.5; transform: scale(0.8); }
    }
  `]
})
export class StatusBadgeComponent {
  @Input() status: BadgeStatus = 'idle';
  @Input() label?: string;

  get statusLabel(): string {
    const labels: Record<BadgeStatus, string> = {
      active: 'Active', idle: 'Idle', error: 'Error', connecting: 'Connecting',
      running: 'Running', completed: 'Completed', failed: 'Failed',
      pending: 'Pending', paused: 'Paused', queued: 'Queued',
      draft: 'Draft', archived: 'Archived'
    };
    return labels[this.status] || this.status;
  }
}
