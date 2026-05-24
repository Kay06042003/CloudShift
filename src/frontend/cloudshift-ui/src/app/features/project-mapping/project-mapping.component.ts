import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MockDataService } from '../../services/mock-data.service';
import { IProjectMapping } from '../../models/project-mapping.model';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-project-mapping',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, StatusBadgeComponent],
  template: `
    <div class="page-container">
      <div class="page-header">
        <div>
          <h1 class="page-title">Project Mapping</h1>
          <p class="page-subtitle">Configure source and destination paths, filters, and rules for migration jobs.</p>
        </div>
        <div class="page-actions">
          <button class="btn-primary" id="create-mapping-btn" (click)="showCreateForm.set(true)">
            <span class="material-symbols-outlined icon-sm">add</span>
            Create Mapping
          </button>
        </div>
      </div>

      <!-- Create Mapping Form (inline) -->
      @if (showCreateForm()) {
        <div class="create-form-panel animate-fade-in">
          <div class="form-panel-header">
            <h2 class="form-panel-title">Create Project Mapping</h2>
            <p class="form-panel-subtitle">Configure source and destination paths, filters, and rules for your new migration job.</p>
            <button class="close-panel-btn" (click)="showCreateForm.set(false)" id="close-create-mapping-btn" aria-label="Close form">
              <span class="material-symbols-outlined">close</span>
            </button>
          </div>

          <!-- Section 1: Source & Destination -->
          <div class="form-section">
            <div class="section-header">
              <span class="material-symbols-outlined section-icon">route</span>
              <div>
                <div class="section-title">Source &amp; Destination Pathing</div>
                <div class="section-subtitle">Select the origin and target accounts with their directory paths.</div>
              </div>
            </div>
            <div class="path-grid">
              <div class="path-box source">
                <div class="path-box-label">Source Configuration</div>
                <div class="form-group">
                  <label for="src-profile">Source Profile *</label>
                  <select id="src-profile" class="cs-select" [(ngModel)]="newMapping.sourceProfileId">
                    <option value="">Select profile...</option>
                    <option value="prof-001">Google Workspace (Primary)</option>
                    <option value="prof-002">Microsoft OneDrive (Corporate)</option>
                    <option value="prof-003">Legacy Google Drive</option>
                    <option value="prof-004">SharePoint (Intranet)</option>
                    <option value="prof-005">Dropbox Business</option>
                    <option value="prof-006">AWS S3 (Archive Bucket)</option>
                  </select>
                </div>
                <div class="form-group">
                  <label for="src-path">Source Path *</label>
                  <input id="src-path" type="text" class="cs-input code-text" placeholder="/path/to/source/**" [(ngModel)]="newMapping.sourcePath" />
                  <span class="hint">Use ** for recursive match</span>
                </div>
              </div>

              <div class="path-arrow">
                <span class="material-symbols-outlined icon-xl" style="color:var(--color-outline)">arrow_forward</span>
              </div>

              <div class="path-box destination">
                <div class="path-box-label">Destination Configuration</div>
                <div class="form-group">
                  <label for="dest-profile">Destination Profile *</label>
                  <select id="dest-profile" class="cs-select" [(ngModel)]="newMapping.destinationProfileId">
                    <option value="">Select profile...</option>
                    <option value="prof-001">Google Workspace (Primary)</option>
                    <option value="prof-002">Microsoft OneDrive (Corporate)</option>
                    <option value="prof-003">Legacy Google Drive</option>
                    <option value="prof-006">AWS S3 (Archive Bucket)</option>
                  </select>
                </div>
                <div class="form-group">
                  <label for="dest-path">Destination Path *</label>
                  <input id="dest-path" type="text" class="cs-input code-text" placeholder="/path/to/destination/" [(ngModel)]="newMapping.destinationPath" />
                </div>
              </div>
            </div>
          </div>

          <!-- Section 2: Filter Engine -->
          <div class="form-section">
            <div class="section-header">
              <span class="material-symbols-outlined section-icon">filter_alt</span>
              <div>
                <div class="section-title">Filter Engine</div>
                <div class="section-subtitle">Define include/exclude rules to control which files are migrated.</div>
              </div>
            </div>
            <div class="filter-builder">
              @for (f of newMapping.filters; track f.id) {
                <div class="filter-row">
                  <select class="cs-select filter-select" [(ngModel)]="f.operator">
                    <option value="include">Include</option>
                    <option value="exclude">Exclude</option>
                  </select>
                  <select class="cs-select filter-select" [(ngModel)]="f.type">
                    <option value="file-extension">File Extension</option>
                    <option value="folder-path">Folder Path</option>
                    <option value="file-size">File Size</option>
                    <option value="date-range">Date Range</option>
                  </select>
                  <input type="text" class="cs-input code-text" [(ngModel)]="f.pattern" placeholder="*.tmp, /folder/path, etc." />
                  <button class="remove-btn" (click)="removeFilter(f.id)" aria-label="Remove filter">
                    <span class="material-symbols-outlined icon-sm">delete</span>
                  </button>
                </div>
              }
              <button class="add-row-btn" (click)="addFilter()" id="add-filter-btn">
                <span class="material-symbols-outlined icon-sm">add</span>
                Add Filter Rule
              </button>
            </div>
          </div>

          <!-- Section 3: Execution Rules -->
          <div class="form-section">
            <div class="section-header">
              <span class="material-symbols-outlined section-icon">rule</span>
              <div>
                <div class="section-title">Execution Rules</div>
                <div class="section-subtitle">Automated actions triggered by job lifecycle events.</div>
              </div>
            </div>
            <div class="rules-grid">
              @for (rule of defaultRules; track rule.id) {
                <div class="rule-item" [class.enabled]="rule.enabled">
                  <div class="rule-toggle-area">
                    <label class="toggle-switch">
                      <input type="checkbox" [(ngModel)]="rule.enabled" [id]="'rule-' + rule.id">
                      <span class="toggle-slider"></span>
                    </label>
                  </div>
                  <div class="rule-info">
                    <div class="rule-name">{{ rule.name }}</div>
                    <div class="rule-desc">{{ rule.description }}</div>
                  </div>
                </div>
              }
            </div>
          </div>

          <!-- Section 4: Job Type -->
          <div class="form-section">
            <div class="section-header">
              <span class="material-symbols-outlined section-icon">rocket_launch</span>
              <div>
                <div class="section-title">Job Type &amp; Execution Mode</div>
                <div class="section-subtitle">Choose how and when to run this migration.</div>
              </div>
            </div>
            <div class="job-type-options">
              @for (jt of jobTypes; track jt.value) {
                <div
                  class="job-type-card"
                  [class.selected]="newMapping.jobType === jt.value"
                  (click)="newMapping.jobType = jt.value"
                  [id]="'job-type-' + jt.value"
                >
                  <span class="material-symbols-outlined">{{ jt.icon }}</span>
                  <div class="jt-name">{{ jt.label }}</div>
                  <div class="jt-desc">{{ jt.description }}</div>
                </div>
              }
            </div>

            <div class="execution-options">
              <div class="form-group">
                <label>Options</label>
                <div class="checkbox-group">
                  @for (opt of booleanOptions; track opt.key) {
                    <label class="checkbox-item">
                      <input type="checkbox" [(ngModel)]="newMapping[opt.key]" [id]="'opt-' + opt.key">
                      {{ opt.label }}
                    </label>
                  }
                </div>
              </div>
            </div>
          </div>

          <!-- Form Actions -->
          <div class="form-actions">
            <button class="btn-secondary" id="save-draft-btn" (click)="saveDraft()">Save as Draft</button>
            <button class="btn-secondary" (click)="showCreateForm.set(false)">Cancel</button>
            <button class="btn-primary" id="create-mapping-submit-btn" (click)="createMapping()">
              <span class="material-symbols-outlined icon-sm">rocket_launch</span>
              Create &amp; Schedule
            </button>
          </div>
        </div>
      }

      <!-- Mappings List -->
      <div class="mappings-list">
        <div class="list-header">
          <h2 class="headline-sm">Existing Mappings</h2>
          <span class="badge-count">{{ mappings.length }}</span>
        </div>

        @for (mapping of mappings; track mapping.id) {
          <div class="mapping-row animate-fade-in">
            <div class="mapping-main">
              <div class="mapping-name">{{ mapping.name }}</div>
              <p class="mapping-desc" *ngIf="mapping.description">{{ mapping.description }}</p>
              <div class="mapping-path-info">
                <span class="path-chip source-chip">
                  <span class="material-symbols-outlined icon-sm">upload</span>
                  {{ mapping.sourceProfileName }}: <code>{{ mapping.sourcePath }}</code>
                </span>
                <span class="material-symbols-outlined icon-sm" style="color:var(--color-outline)">arrow_forward</span>
                <span class="path-chip dest-chip">
                  <span class="material-symbols-outlined icon-sm">download</span>
                  {{ mapping.destinationProfileName }}: <code>{{ mapping.destinationPath }}</code>
                </span>
              </div>
            </div>
            <div class="mapping-meta">
              <app-status-badge [status]="mapping.status" />
              <span class="meta-chip">{{ mapping.jobType | titlecase }}</span>
              <span class="meta-chip">{{ mapping.executionMode | titlecase }}</span>
              <span class="meta-chip">{{ mapping.filters.length }} filters</span>
            </div>
            <div class="mapping-actions">
              <button class="btn-icon-sm" title="Run now" id="run-mapping-{{mapping.id}}">
                <span class="material-symbols-outlined icon-sm">play_arrow</span>
              </button>
              <button class="btn-icon-sm" title="Edit">
                <span class="material-symbols-outlined icon-sm">edit</span>
              </button>
              <button class="btn-icon-sm danger" title="Delete">
                <span class="material-symbols-outlined icon-sm">delete</span>
              </button>
            </div>
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .create-form-panel {
      background: var(--color-surface-container-lowest);
      border: 1px solid var(--color-primary-fixed-dim);
      border-radius: var(--radius-xl);
      padding: 28px;
      margin-bottom: 24px;
      box-shadow: var(--shadow-lg);
    }

    .form-panel-header {
      position: relative;
      margin-bottom: 24px;
      padding-bottom: 16px;
      border-bottom: 1px solid var(--color-outline-variant);

      .form-panel-title { font-size: 18px; font-weight: 700; }
      .form-panel-subtitle { font-size: 13px; color: var(--color-outline); margin-top: 4px; }
    }

    .close-panel-btn {
      position: absolute;
      top: 0;
      right: 0;
      width: 32px;
      height: 32px;
      border-radius: var(--radius);
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--color-outline);
      transition: background 0.15s;

      &:hover { background: var(--color-surface-container); color: var(--color-on-surface); }
    }

    .form-section {
      margin-bottom: 24px;
      padding-bottom: 24px;
      border-bottom: 1px dashed var(--color-outline-variant);

      &:last-child { border-bottom: none; }
    }

    .section-header {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      margin-bottom: 16px;
    }

    .section-icon {
      width: 36px;
      height: 36px;
      background: var(--color-primary-fixed);
      color: var(--color-primary);
      border-radius: var(--radius-md);
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      font-size: 18px;
    }

    .section-title { font-size: 15px; font-weight: 600; }
    .section-subtitle { font-size: 13px; color: var(--color-outline); margin-top: 2px; }

    // Path grid
    .path-grid {
      display: grid;
      grid-template-columns: 1fr auto 1fr;
      gap: 16px;
      align-items: center;
    }

    .path-arrow { display: flex; justify-content: center; }

    .path-box {
      background: var(--color-surface-container-low);
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius-lg);
      padding: 16px;
      display: flex;
      flex-direction: column;
      gap: 12px;

      &.source { border-left: 3px solid var(--color-secondary); }
      &.destination { border-left: 3px solid var(--color-primary); }
    }

    .path-box-label {
      font-size: 11px;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.06em;
      color: var(--color-outline);
    }

    // Filter builder
    .filter-builder { display: flex; flex-direction: column; gap: 8px; }

    .filter-row {
      display: grid;
      grid-template-columns: 120px 160px 1fr auto;
      gap: 8px;
      align-items: center;
    }

    .filter-select { }

    .remove-btn {
      width: 32px;
      height: 32px;
      border-radius: var(--radius);
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--color-outline);
      transition: all 0.15s;

      &:hover { background: var(--color-error-container); color: var(--color-error); }
    }

    .add-row-btn {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 6px 12px;
      border: 1px dashed var(--color-outline-variant);
      border-radius: var(--radius);
      font-size: 13px;
      color: var(--color-on-surface-variant);
      transition: all 0.15s;

      &:hover { border-color: var(--color-primary); color: var(--color-primary); background: var(--color-primary-fixed); }
    }

    // Rules
    .rules-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 10px;
    }

    .rule-item {
      display: flex;
      align-items: flex-start;
      gap: 12px;
      padding: 12px;
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius-md);
      background: var(--color-surface-container-low);
      transition: border-color 0.15s;

      &.enabled { border-color: var(--color-primary-fixed-dim); background: var(--color-primary-fixed); }
    }

    .rule-name { font-size: 13px; font-weight: 600; }
    .rule-desc { font-size: 12px; color: var(--color-outline); margin-top: 2px; }

    // Toggle switch
    .toggle-switch {
      position: relative;
      display: inline-block;
      width: 36px;
      height: 20px;
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
        width: 14px;
        height: 14px;
        left: 3px;
        top: 3px;
        background: white;
        border-radius: 50%;
        transition: 0.2s;
      }
    }

    input:checked + .toggle-slider {
      background: var(--color-primary);
    }

    input:checked + .toggle-slider::before {
      transform: translateX(16px);
    }

    // Job types
    .job-type-options {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 12px;
      margin-bottom: 16px;
    }

    .job-type-card {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      gap: 6px;
      padding: 16px;
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius-lg);
      cursor: pointer;
      transition: all 0.15s;
      background: var(--color-surface-container-low);

      &:hover { border-color: var(--color-primary-fixed-dim); }
      &.selected {
        border-color: var(--color-primary);
        background: var(--color-primary-fixed);
        box-shadow: 0 0 0 2px rgba(0,87,194,0.12);
      }

      .material-symbols-outlined { font-size: 24px; color: var(--color-primary); }
      .jt-name { font-size: 14px; font-weight: 600; }
      .jt-desc { font-size: 12px; color: var(--color-outline); }
    }

    .checkbox-group { display: flex; flex-direction: column; gap: 8px; }

    .checkbox-item {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 13px;
      color: var(--color-on-surface-variant);
      cursor: pointer;

      input[type="checkbox"] { accent-color: var(--color-primary); width: 15px; height: 15px; }
    }

    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 10px;
      padding-top: 16px;
    }

    // Mappings list
    .list-header {
      display: flex;
      align-items: center;
      gap: 10px;
      margin-bottom: 12px;
    }

    .badge-count {
      background: var(--color-surface-container);
      border-radius: var(--radius-full);
      font-size: 12px;
      font-weight: 600;
      padding: 1px 10px;
    }

    .mappings-list { display: flex; flex-direction: column; gap: 10px; }

    .mapping-row {
      display: flex;
      align-items: center;
      gap: 16px;
      background: var(--color-surface-container-lowest);
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius-lg);
      padding: 16px;
      transition: box-shadow 0.15s;

      &:hover { box-shadow: var(--shadow-sm); }
    }

    .mapping-main { flex: 1; min-width: 0; }
    .mapping-name { font-size: 14px; font-weight: 600; margin-bottom: 4px; }
    .mapping-desc { font-size: 12px; color: var(--color-outline); margin-bottom: 8px; }

    .mapping-path-info {
      display: flex;
      align-items: center;
      gap: 8px;
      flex-wrap: wrap;
    }

    .path-chip {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      padding: 3px 8px;
      border-radius: var(--radius);
      font-size: 12px;

      &.source-chip { background: #e8f5e9; color: #2e7d32; }
      &.dest-chip   { background: var(--color-primary-fixed); color: var(--color-primary-dark); }

      code { font-family: var(--font-mono); font-size: 11px; }
    }

    .mapping-meta {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      gap: 6px;
      flex-shrink: 0;
    }

    .meta-chip {
      font-size: 11px;
      padding: 2px 8px;
      background: var(--color-surface-container);
      border-radius: var(--radius-full);
      color: var(--color-on-surface-variant);
      font-weight: 500;
    }

    .mapping-actions {
      display: flex;
      flex-direction: column;
      gap: 4px;
      flex-shrink: 0;
    }

    .btn-icon-sm {
      width: 28px;
      height: 28px;
      border-radius: var(--radius);
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--color-on-surface-variant);
      transition: background 0.15s;

      &:hover { background: var(--color-surface-container); color: var(--color-on-surface); }
      &.danger:hover { background: var(--color-error-container); color: var(--color-error); }
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

    @media (max-width: 900px) {
      .path-grid { grid-template-columns: 1fr; }
      .path-arrow { display: none; }
      .filter-row { grid-template-columns: 1fr 1fr; }
      .job-type-options { grid-template-columns: 1fr; }
      .rules-grid { grid-template-columns: 1fr; }
    }
  `]
})
export class ProjectMappingComponent implements OnInit {
  mappings: IProjectMapping[] = [];
  showCreateForm = signal(false);

  newMapping: any = {
    sourceProfileId: '',
    sourcePath: '',
    destinationProfileId: '',
    destinationPath: '',
    filters: [],
    jobType: 'full',
    preservePermissions: true,
    deleteSourceAfterCopy: false,
    overwriteExisting: false
  };

  jobTypes = [
    { value: 'full', label: 'Full Migration', icon: 'cloud_upload', description: 'Transfer all files from source to destination.' },
    { value: 'incremental', label: 'Incremental', icon: 'sync', description: 'Only transfer new or modified files since last run.' },
    { value: 'delta', label: 'Delta Sync', icon: 'difference', description: 'Sync differences; add, update and delete to match source.' },
  ];

  defaultRules = [
    { id: 'r1', name: 'Retry on Failure', description: 'Retry failed files up to 3 times automatically.', enabled: true },
    { id: 'r2', name: 'Email on Completion', description: 'Send email notification when job completes.', enabled: true },
    { id: 'r3', name: 'Skip Hidden Files', description: 'Skip files and folders starting with a dot (.).' , enabled: true },
    { id: 'r4', name: 'Checksum Validation', description: 'Verify file integrity via MD5 checksum after transfer.', enabled: false },
    { id: 'r5', name: 'Pause on High Error Rate', description: 'Auto-pause if error rate exceeds 5%.', enabled: false },
    { id: 'r6', name: 'Preserve Timestamps', description: 'Keep original file creation and modification timestamps.', enabled: true },
  ];

  booleanOptions = [
    { key: 'preservePermissions', label: 'Preserve file permissions' },
    { key: 'deleteSourceAfterCopy', label: 'Delete source files after successful copy' },
    { key: 'overwriteExisting', label: 'Overwrite existing files at destination' },
  ];

  constructor(private mockData: MockDataService) {}

  ngOnInit() {
    this.mappings = this.mockData.getProjectMappings();
  }

  addFilter() {
    this.newMapping.filters.push({
      id: `f-${Date.now()}`,
      operator: 'exclude',
      pattern: '',
      type: 'file-extension'
    });
  }

  removeFilter(id: string) {
    this.newMapping.filters = this.newMapping.filters.filter((f: any) => f.id !== id);
  }

  saveDraft() {
    console.log('Saving as draft', this.newMapping);
    this.showCreateForm.set(false);
  }

  createMapping() {
    console.log('Creating mapping', this.newMapping);
    this.showCreateForm.set(false);
  }
}
