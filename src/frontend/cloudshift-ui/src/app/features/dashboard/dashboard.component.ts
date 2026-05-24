import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CloudShiftApiService } from '../../services/cloudshift-api.service';
import { IDashboardStats, IFileLogEntry } from '../../models/dashboard.model';
import { IMigrationJob } from '../../models/migration-job.model';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { ProgressBarComponent } from '../../shared/components/progress-bar/progress-bar.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, StatCardComponent, StatusBadgeComponent, ProgressBarComponent],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <h1 class="page-title">Migration Activity</h1>
          <p class="page-subtitle">Real-time overview of all transfer operations</p>
        </div>
        <div class="page-actions">
          <span class="live-indicator">
            <span class="live-dot"></span>
            Live
          </span>
          <span class="last-updated">Updated {{ lastUpdated | date:'shortTime' }}</span>
        </div>
      </div>

      <!-- Stat Cards -->
      <div class="stats-grid">
        <app-stat-card
          title="Total Files Transferred"
          [value]="(stats.totalFilesTransferred | number) || '0'"
          icon="folder_copy"
          iconColor="blue"
          [trend]="12.4"
        />
        <app-stat-card
          title="Failed Files"
          [value]="(stats.failedFiles | number) || '0'"
          icon="error_outline"
          iconColor="red"
          [trend]="-3.2"
        />
        <app-stat-card
          title="Total Data Moved"
          [value]="stats.totalDataMovedGB + ' GB'"
          icon="storage"
          iconColor="green"
          [trend]="8.7"
        />
        <app-stat-card
          title="Active Jobs"
          [value]="stats.activeJobs.toString()"
          icon="sync"
          iconColor="orange"
        />
        <app-stat-card
          title="Completed Today"
          [value]="stats.completedJobsToday.toString()"
          icon="task_alt"
          iconColor="green"
          [trend]="2"
        />
        <app-stat-card
          title="Success Rate"
          [value]="stats.successRate + '%'"
          icon="verified"
          iconColor="purple"
          [trend]="0.3"
        />
      </div>

      <!-- Active Jobs + File Log -->
      <div class="dashboard-lower">
        <!-- Active Jobs -->
        <div class="cs-card active-jobs-card">
          <div class="card-header-row">
            <h2 class="card-title">Active Migration Jobs</h2>
            <span class="badge-count">{{ activeJobs.length }}</span>
          </div>
          <div class="active-jobs-list">
            @for (job of activeJobs; track job.id) {
              <div class="active-job-row">
                <div class="job-info">
                  <div class="job-name">{{ job.name }}</div>
                  <div class="job-meta">
                    <span>{{ job.sourceProfileName }}</span>
                    <span class="material-symbols-outlined icon-sm" style="color:var(--color-outline)">arrow_forward</span>
                    <span>{{ job.destinationProfileName }}</span>
                  </div>
                </div>
                <div class="job-progress-col">
                  <app-progress-bar
                    [value]="job.progress"
                    [showLabel]="true"
                    [metaLeft]="job.filesTransferred + ' / ' + job.totalFiles + ' files'"
                    [metaRight]="job.dataTransferredGB + ' GB / ' + job.totalDataGB + ' GB'"
                  />
                </div>
                <app-status-badge [status]="job.status" />
              </div>
            } @empty {
              <div class="empty-state-sm">
                <span class="material-symbols-outlined icon-xl" style="color:var(--color-outline)">task_alt</span>
                <p>No active jobs at the moment.</p>
              </div>
            }
          </div>
        </div>

        <!-- Real-Time File Log -->
        <div class="cs-card file-log-card">
          <div class="card-header-row">
            <h2 class="card-title">Real-Time File Log</h2>
            <span class="log-live-tag">
              <span class="live-dot small"></span>
              Streaming
            </span>
          </div>
          <div class="file-log-table-wrap">
            <table class="cs-table file-log-table">
              <thead>
                <tr>
                  <th>Timestamp</th>
                  <th>File</th>
                  <th>Size</th>
                  <th>Job</th>
                  <th>Duration</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                @for (entry of logEntries; track entry.id) {
                  <tr class="animate-fade-in">
                    <td class="code-text">{{ entry.timestamp | date:'HH:mm:ss' }}</td>
                    <td class="file-name-cell">
                      <span class="material-symbols-outlined icon-sm" style="color:var(--color-outline)">description</span>
                      {{ entry.fileName }}
                    </td>
                    <td>{{ formatSize(entry.sizeKB) }}</td>
                    <td class="text-secondary">{{ entry.jobName }}</td>
                    <td class="code-text">{{ entry.duration }}ms</td>
                    <td>
                      <app-status-badge
                        [status]="entry.status === 'transferred' ? 'completed' : entry.status === 'failed' ? 'failed' : 'idle'"
                        [label]="entry.status | titlecase"
                      />
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 16px;
      margin-bottom: 24px;
    }

    .page-actions {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .live-indicator {
      display: flex;
      align-items: center;
      gap: 6px;
      font-size: 13px;
      font-weight: 600;
      color: #1a7a4a;
      background: #e6f4ed;
      padding: 4px 10px;
      border-radius: var(--radius-full);
    }

    .live-dot {
      width: 8px;
      height: 8px;
      background: #1a7a4a;
      border-radius: 50%;
      animation: pulse 1.5s infinite;

      &.small { width: 6px; height: 6px; background: var(--color-primary); }
    }

    .log-live-tag {
      display: flex;
      align-items: center;
      gap: 5px;
      font-size: 12px;
      font-weight: 500;
      color: var(--color-primary);
    }

    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.4; }
    }

    .last-updated { font-size: 12px; color: var(--color-outline); }

    // Dashboard lower
    .dashboard-lower {
      display: grid;
      grid-template-columns: 1fr 1.6fr;
      gap: 16px;
    }

    .card-header-row {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 16px;
    }

    .card-title { font-size: 15px; font-weight: 600; }

    .badge-count {
      background: var(--color-primary-fixed);
      color: var(--color-primary);
      font-size: 12px;
      font-weight: 600;
      padding: 1px 8px;
      border-radius: var(--radius-full);
    }

    // Active jobs
    .active-jobs-card { height: fit-content; }
    .active-jobs-list { display: flex; flex-direction: column; gap: 16px; }

    .active-job-row {
      display: flex;
      flex-direction: column;
      gap: 10px;
      padding: 12px;
      background: var(--color-surface-container-low);
      border-radius: var(--radius-md);
      border: 1px solid var(--color-outline-variant);
    }

    .job-info {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 8px;
    }

    .job-name { font-size: 13px; font-weight: 600; }

    .job-meta {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 12px;
      color: var(--color-outline);
    }

    .job-progress-col { width: 100%; }

    // File log
    .file-log-card { overflow: hidden; padding: 20px 20px 0; }

    .file-log-table-wrap {
      max-height: 420px;
      overflow-y: auto;
      margin: 0 -20px;
    }

    .file-log-table {
      font-size: 12px;

      .file-name-cell {
        display: flex;
        align-items: center;
        gap: 6px;
        max-width: 200px;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
      }
    }

    .empty-state-sm {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 8px;
      padding: 32px;
      color: var(--color-outline);
      font-size: 13px;
    }

    @media (max-width: 1100px) {
      .dashboard-lower { grid-template-columns: 1fr; }
    }

    @media (max-width: 768px) {
      .stats-grid { grid-template-columns: 1fr 1fr; }
    }
  `]
})
export class DashboardComponent implements OnInit, OnDestroy {
  stats: IDashboardStats = {
    totalFilesTransferred: 0,
    failedFiles: 0,
    totalDataMovedGB: 0,
    activeJobs: 0,
    completedJobsToday: 0,
    successRate: 100
  };
  logEntries: IFileLogEntry[] = [];
  activeJobs: IMigrationJob[] = [];
  lastUpdated = new Date();
  private intervalId?: ReturnType<typeof setInterval>;

  constructor(private api: CloudShiftApiService) {}

  ngOnInit() {
    this.reload();

    this.intervalId = setInterval(() => {
      this.lastUpdated = new Date();
      this.reload();
    }, 3000);
  }

  reload() {
    this.api.getDashboardStats().subscribe({
      next: stats => this.stats = stats,
      error: error => console.error('Failed to load dashboard stats', error)
    });

    this.api.getFileLogEntries().subscribe({
      next: entries => this.logEntries = entries,
      error: error => console.error('Failed to load file log entries', error)
    });

    this.api.getMigrationJobs().subscribe({
      next: jobs => this.activeJobs = jobs.filter(j => j.status === 'running' || j.status === 'queued' || j.status === 'paused'),
      error: error => console.error('Failed to load active jobs', error)
    });
  }

  ngOnDestroy() {
    if (this.intervalId) clearInterval(this.intervalId);
  }

  formatSize(kb: number): string {
    if (kb >= 1024 * 1024) return (kb / 1024 / 1024).toFixed(1) + ' GB';
    if (kb >= 1024) return (kb / 1024).toFixed(1) + ' MB';
    return kb + ' KB';
  }
}
