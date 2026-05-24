import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CloudShiftApiService } from '../../services/cloudshift-api.service';
import { IMigrationJob, JobStatus } from '../../models/migration-job.model';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { ProgressBarComponent } from '../../shared/components/progress-bar/progress-bar.component';

@Component({
  selector: 'app-migration-jobs',
  standalone: true,
  imports: [CommonModule, FormsModule, StatusBadgeComponent, ProgressBarComponent],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <h1 class="page-title">Migration Jobs</h1>
          <p class="page-subtitle">Monitor and manage all scheduled and running data transfer jobs.</p>
        </div>
        <div class="page-actions">
          <button class="btn-secondary" id="refresh-jobs-btn" (click)="loadJobs()">
            <span class="material-symbols-outlined icon-sm">refresh</span>
            Refresh
          </button>
          <button class="btn-primary" id="new-job-btn">
            <span class="material-symbols-outlined icon-sm">add</span>
            New Job
          </button>
        </div>
      </div>

      <!-- Filter Bar -->
      <div class="filter-bar">
        <div class="search-wrap">
          <span class="material-symbols-outlined icon-sm search-ico">search</span>
          <input
            type="text"
            class="cs-input search-field"
            placeholder="Search jobs..."
            [(ngModel)]="searchQuery"
            id="jobs-search"
          />
        </div>

        <div class="status-filters">
          @for (f of statusFilters; track f.value) {
            <button
              class="filter-chip"
              [class.active]="activeFilter === f.value"
              (click)="activeFilter = f.value"
              [id]="'job-filter-' + f.value"
            >
              <span class="chip-dot" [ngClass]="f.dotClass"></span>
              {{ f.label }}
              <span class="chip-num">{{ getCount(f.value) }}</span>
            </button>
          }
        </div>

        <div class="sort-wrap">
          <span class="material-symbols-outlined icon-sm" style="color:var(--color-outline)">sort</span>
          <select class="cs-select sort-select" [(ngModel)]="sortBy" id="jobs-sort">
            <option value="startedAt">Sort: Start Time</option>
            <option value="name">Sort: Name</option>
            <option value="progress">Sort: Progress</option>
            <option value="status">Sort: Status</option>
          </select>
        </div>
      </div>

      <!-- Jobs Table -->
      <div class="cs-table-wrapper jobs-table">
        <table class="cs-table">
          <thead>
            <tr>
              <th>Job Name</th>
              <th>Source → Destination</th>
              <th>Type</th>
              <th>Progress</th>
              <th>Files</th>
              <th>Data</th>
              <th>Started</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            @for (job of filteredJobs; track job.id) {
              <tr class="animate-fade-in">
                <td>
                  <div class="job-name-cell">
                    <span class="material-symbols-outlined icon-sm job-type-icon" [ngClass]="job.jobType">sync</span>
                    <div>
                      <div class="job-name">{{ job.name }}</div>
                      <div class="job-priority" [ngClass]="job.priority">
                        {{ job.priority | titlecase }} Priority
                      </div>
                    </div>
                  </div>
                </td>
                <td>
                  <div class="flow-cell">
                    <span class="flow-item src">{{ job.sourceProfileName }}</span>
                    <span class="material-symbols-outlined icon-sm" style="color:var(--color-outline)">arrow_forward</span>
                    <span class="flow-item dst">{{ job.destinationProfileName }}</span>
                  </div>
                </td>
                <td>
                  <span class="type-badge" [ngClass]="job.jobType">{{ job.jobType }}</span>
                </td>
                <td class="progress-cell">
                  <app-progress-bar
                    [value]="job.progress"
                    [showLabel]="false"
                    [variant]="job.status === 'failed' ? 'error' : job.status === 'completed' ? 'success' : 'default'"
                  />
                  <span class="progress-pct">{{ job.progress }}%</span>
                </td>
                <td>
                  <div class="count-cell">
                    <div>{{ job.filesTransferred | number }}</div>
                    <div class="count-total">/ {{ job.totalFiles | number }}</div>
                  </div>
                </td>
                <td>
                  <div class="count-cell">
                    <div>{{ job.dataTransferredGB }} GB</div>
                    <div class="count-total">/ {{ job.totalDataGB }} GB</div>
                  </div>
                </td>
                <td>
                  <div class="date-cell">
                    <div>{{ job.startedAt | date:'MMM d' }}</div>
                    <div class="count-total">{{ job.startedAt | date:'HH:mm' }}</div>
                  </div>
                </td>
                <td>
                  <app-status-badge [status]="job.status" />
                  <div class="error-msg" *ngIf="job.errorMessage">
                    <span class="material-symbols-outlined icon-sm" style="color:var(--color-error)">warning</span>
                    {{ job.errorMessage }}
                  </div>
                </td>
                <td>
                  <div class="action-btns">
                    <button class="action-btn" [title]="job.status === 'running' ? 'Pause' : 'Resume'" [id]="'toggle-job-' + job.id">
                      <span class="material-symbols-outlined icon-sm">
                        {{ job.status === 'running' ? 'pause' : job.status === 'paused' ? 'play_arrow' : 'replay' }}
                      </span>
                    </button>
                    <button class="action-btn" title="View logs" [id]="'logs-job-' + job.id">
                      <span class="material-symbols-outlined icon-sm">article</span>
                    </button>
                    <button class="action-btn danger" title="Delete job" [id]="'delete-job-' + job.id">
                      <span class="material-symbols-outlined icon-sm">delete</span>
                    </button>
                  </div>
                </td>
              </tr>
            } @empty {
              <tr>
                <td colspan="9" class="empty-row">
                  <div class="empty-table-state">
                    <span class="material-symbols-outlined" style="font-size:36px;color:var(--color-outline)">sync_disabled</span>
                    <p>No migration jobs match your filter.</p>
                  </div>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <!-- Footer Info -->
      <div class="table-footer">
        Showing {{ filteredJobs.length }} of {{ jobs.length }} jobs
      </div>
    </div>
  `,
  styles: [`
    .filter-bar {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 16px;
      flex-wrap: wrap;
    }

    .search-wrap {
      position: relative;
      min-width: 200px;

      .search-ico {
        position: absolute;
        left: 10px;
        top: 50%;
        transform: translateY(-50%);
        color: var(--color-outline);
      }

      .search-field { padding-left: 32px; }
    }

    .status-filters { display: flex; gap: 6px; flex-wrap: wrap; }

    .filter-chip {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 4px 12px;
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius-full);
      font-size: 12px;
      font-weight: 500;
      color: var(--color-on-surface-variant);
      background: transparent;
      cursor: pointer;
      transition: all 0.15s;

      &.active { background: var(--color-primary-fixed); border-color: var(--color-primary); color: var(--color-primary); }
    }

    .chip-dot {
      width: 6px;
      height: 6px;
      border-radius: 50%;

      &.running   { background: var(--color-primary); }
      &.completed { background: #1a7a4a; }
      &.failed    { background: var(--color-error); }
      &.pending   { background: #e67700; }
      &.paused    { background: var(--color-outline); }
    }

    .chip-num {
      background: var(--color-surface-container-high);
      border-radius: var(--radius-full);
      font-size: 10px;
      padding: 0 5px;
    }

    .sort-wrap {
      display: flex;
      align-items: center;
      gap: 6px;
      margin-left: auto;

      .sort-select { width: 160px; }
    }

    // Table cells
    .jobs-table { margin-bottom: 8px; }

    .job-name-cell {
      display: flex;
      align-items: flex-start;
      gap: 10px;
    }

    .job-type-icon { color: var(--color-outline); }

    .job-name { font-size: 13px; font-weight: 600; white-space: nowrap; }

    .job-priority {
      font-size: 11px;
      margin-top: 2px;

      &.high, &.critical { color: var(--color-error); }
      &.normal { color: var(--color-outline); }
      &.low    { color: var(--color-on-surface-variant); }
    }

    .flow-cell {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 12px;
    }

    .flow-item {
      &.src { color: var(--color-secondary); }
      &.dst { color: var(--color-primary); }
    }

    .type-badge {
      font-size: 11px;
      font-weight: 600;
      padding: 2px 7px;
      border-radius: var(--radius-full);
      text-transform: capitalize;

      &.full        { background: var(--color-primary-fixed); color: var(--color-primary-dark); }
      &.incremental { background: #e8f5e9; color: #2e7d32; }
      &.delta       { background: #fff3e0; color: #a06000; }
    }

    .progress-cell {
      min-width: 140px;
      display: flex;
      align-items: center;
      gap: 8px;

      .progress-pct { font-size: 12px; font-weight: 600; color: var(--color-on-surface); white-space: nowrap; }

      app-progress-bar { flex: 1; }
    }

    .count-cell {
      font-size: 13px;
      font-weight: 600;
      .count-total { font-size: 11px; font-weight: 400; color: var(--color-outline); }
    }

    .date-cell {
      font-size: 12px;
      .count-total { color: var(--color-outline); }
    }

    .error-msg {
      display: flex;
      align-items: flex-start;
      gap: 4px;
      font-size: 11px;
      color: var(--color-error);
      margin-top: 4px;
      max-width: 200px;
    }

    .action-btns { display: flex; gap: 2px; }

    .action-btn {
      width: 26px;
      height: 26px;
      border-radius: var(--radius);
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--color-on-surface-variant);
      transition: background 0.15s;

      &:hover { background: var(--color-surface-container); color: var(--color-on-surface); }
      &.danger:hover { background: var(--color-error-container); color: var(--color-error); }
    }

    .empty-row td { padding: 48px !important; }
    .empty-table-state { display: flex; flex-direction: column; align-items: center; gap: 8px; color: var(--color-outline); font-size: 13px; }

    .table-footer {
      font-size: 12px;
      color: var(--color-outline);
      text-align: right;
      padding: 4px 0;
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
export class MigrationJobsComponent implements OnInit {
  jobs: IMigrationJob[] = [];
  searchQuery = '';
  activeFilter = 'all';
  sortBy = 'startedAt';

  statusFilters = [
    { value: 'all',       label: 'All Jobs',  dotClass: '' },
    { value: 'running',   label: 'Running',   dotClass: 'running' },
    { value: 'pending',   label: 'Pending',   dotClass: 'pending' },
    { value: 'completed', label: 'Completed', dotClass: 'completed' },
    { value: 'failed',    label: 'Failed',    dotClass: 'failed' },
    { value: 'paused',    label: 'Paused',    dotClass: 'paused' },
  ];

  constructor(private api: CloudShiftApiService) {}

  ngOnInit() { this.loadJobs(); }

  loadJobs() {
    this.api.getMigrationJobs().subscribe({
      next: jobs => this.jobs = jobs,
      error: error => console.error('Failed to load migration jobs', error)
    });
  }

  get filteredJobs(): IMigrationJob[] {
    return this.jobs.filter(j => {
      const matchSearch = !this.searchQuery ||
        j.name.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        j.sourceProfileName.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        j.destinationProfileName.toLowerCase().includes(this.searchQuery.toLowerCase());
      const matchFilter = this.activeFilter === 'all' || j.status === this.activeFilter;
      return matchSearch && matchFilter;
    });
  }

  getCount(filter: string): number {
    if (filter === 'all') return this.jobs.length;
    return this.jobs.filter(j => j.status === filter).length;
  }
}
