import { Injectable } from '@angular/core';
import { IAppProfile } from '../models/app-profile.model';
import { IMigrationJob } from '../models/migration-job.model';
import { IProjectMapping } from '../models/project-mapping.model';
import { IDashboardStats, IFileLogEntry } from '../models/dashboard.model';

@Injectable({ providedIn: 'root' })
export class MockDataService {

  // ── App Profiles ──────────────────────────────────────────────────────────
  getAppProfiles(): IAppProfile[] {
    return [
      {
        id: 'prof-001',
        name: 'Google Workspace (Primary)',
        provider: 'google-workspace',
        email: 'engineering@company.com',
        status: 'active',
        storageUsedGB: 142.5,
        storageTotalGB: 500,
        filesCount: 184320,
        lastSync: new Date('2026-05-11T08:30:00'),
        createdAt: new Date('2025-01-15')
      },
      {
        id: 'prof-002',
        name: 'Microsoft OneDrive (Corporate)',
        provider: 'onedrive',
        email: 'admin@corp.com',
        status: 'active',
        storageUsedGB: 87.2,
        storageTotalGB: 1000,
        filesCount: 95412,
        lastSync: new Date('2026-05-11T09:15:00'),
        createdAt: new Date('2025-02-03')
      },
      {
        id: 'prof-003',
        name: 'Legacy Google Drive',
        provider: 'google-drive',
        email: 'marketing-old@company.com',
        status: 'idle',
        storageUsedGB: 310.8,
        storageTotalGB: 400,
        filesCount: 421800,
        lastSync: new Date('2026-05-09T14:00:00'),
        createdAt: new Date('2024-08-20')
      },
      {
        id: 'prof-004',
        name: 'SharePoint (Intranet)',
        provider: 'sharepoint',
        email: 'svc-account@corp.onmicrosoft.com',
        status: 'error',
        storageUsedGB: 54.3,
        storageTotalGB: 250,
        filesCount: 62100,
        lastSync: new Date('2026-05-10T11:45:00'),
        createdAt: new Date('2025-03-12')
      },
      {
        id: 'prof-005',
        name: 'Dropbox Business',
        provider: 'dropbox',
        email: 'team-dropbox@company.com',
        status: 'active',
        storageUsedGB: 28.9,
        storageTotalGB: 200,
        filesCount: 18750,
        lastSync: new Date('2026-05-11T07:00:00'),
        createdAt: new Date('2025-04-01')
      },
      {
        id: 'prof-006',
        name: 'AWS S3 (Archive Bucket)',
        provider: 's3',
        email: 'arn:aws:iam::123456789:user/cloudmigrator',
        status: 'active',
        storageUsedGB: 2048,
        storageTotalGB: 10240,
        filesCount: 2100000,
        lastSync: new Date('2026-05-11T06:00:00'),
        createdAt: new Date('2024-12-01')
      }
    ];
  }

  // ── Migration Jobs ────────────────────────────────────────────────────────
  getMigrationJobs(): IMigrationJob[] {
    return [
      {
        id: 'job-001',
        projectMappingId: 'map-001',
        name: 'Backup Data 2023 → Archive',
        sourceProfileId: 'prof-003',
        sourceProfileName: 'Legacy Google Drive',
        destinationProfileId: 'prof-006',
        destinationProfileName: 'AWS S3 (Archive)',
        status: 'running',
        progress: 67,
        filesTransferred: 47821,
        totalFiles: 71380,
        dataTransferredGB: 28.4,
        totalDataGB: 42.3,
        startedAt: new Date('2026-05-11T04:00:00'),
        estimatedCompletionAt: new Date('2026-05-11T14:30:00'),
        jobType: 'full',
        priority: 'high'
      },
      {
        id: 'job-002',
        projectMappingId: 'map-002',
        name: 'Engineering Docs Sync',
        sourceProfileId: 'prof-001',
        sourceProfileName: 'Google Workspace (Primary)',
        destinationProfileId: 'prof-002',
        destinationProfileName: 'Microsoft OneDrive',
        status: 'completed',
        progress: 100,
        filesTransferred: 12480,
        totalFiles: 12480,
        dataTransferredGB: 5.8,
        totalDataGB: 5.8,
        startedAt: new Date('2026-05-10T22:00:00'),
        completedAt: new Date('2026-05-11T01:12:00'),
        jobType: 'incremental',
        priority: 'normal'
      },
      {
        id: 'job-003',
        projectMappingId: 'map-003',
        name: 'Marketing Archive Migration',
        sourceProfileId: 'prof-003',
        sourceProfileName: 'Legacy Google Drive',
        destinationProfileId: 'prof-002',
        destinationProfileName: 'Microsoft OneDrive',
        status: 'failed',
        progress: 34,
        filesTransferred: 8420,
        totalFiles: 24800,
        dataTransferredGB: 12.1,
        totalDataGB: 35.6,
        startedAt: new Date('2026-05-11T01:30:00'),
        jobType: 'full',
        priority: 'normal',
        errorMessage: 'Destination quota exceeded. Free up space and retry.'
      },
      {
        id: 'job-004',
        projectMappingId: 'map-004',
        name: 'SharePoint → OneDrive Lift',
        sourceProfileId: 'prof-004',
        sourceProfileName: 'SharePoint (Intranet)',
        destinationProfileId: 'prof-002',
        destinationProfileName: 'Microsoft OneDrive',
        status: 'pending',
        progress: 0,
        filesTransferred: 0,
        totalFiles: 62100,
        dataTransferredGB: 0,
        totalDataGB: 54.3,
        startedAt: new Date('2026-05-12T02:00:00'),
        jobType: 'full',
        priority: 'low'
      },
      {
        id: 'job-005',
        projectMappingId: 'map-005',
        name: 'Dropbox → Google Workspace',
        sourceProfileId: 'prof-005',
        sourceProfileName: 'Dropbox Business',
        destinationProfileId: 'prof-001',
        destinationProfileName: 'Google Workspace (Primary)',
        status: 'paused',
        progress: 52,
        filesTransferred: 9720,
        totalFiles: 18750,
        dataTransferredGB: 15.0,
        totalDataGB: 28.9,
        startedAt: new Date('2026-05-11T00:00:00'),
        jobType: 'incremental',
        priority: 'normal'
      },
      {
        id: 'job-006',
        projectMappingId: 'map-006',
        name: 'Delta Sync — Finance Q1',
        sourceProfileId: 'prof-001',
        sourceProfileName: 'Google Workspace (Primary)',
        destinationProfileId: 'prof-006',
        destinationProfileName: 'AWS S3 (Archive)',
        status: 'queued',
        progress: 0,
        filesTransferred: 0,
        totalFiles: 4200,
        dataTransferredGB: 0,
        totalDataGB: 2.1,
        startedAt: new Date('2026-05-12T06:00:00'),
        jobType: 'delta',
        priority: 'high'
      }
    ];
  }

  // ── Project Mappings ──────────────────────────────────────────────────────
  getProjectMappings(): IProjectMapping[] {
    return [
      {
        id: 'map-001',
        name: 'Archive Cold Storage Pipeline',
        description: 'Migrating unstructured archive data to cold storage tier.',
        sourceProfileId: 'prof-003',
        sourceProfileName: 'Legacy Google Drive',
        sourcePath: '/shared/archive/2023/**',
        destinationProfileId: 'prof-006',
        destinationProfileName: 'AWS S3 (Archive)',
        destinationPath: 's3://company-archive/gd-backup/2023/',
        filters: [
          { id: 'f1', operator: 'exclude', pattern: '*.tmp', type: 'file-extension' },
          { id: 'f2', operator: 'exclude', pattern: '*.cache', type: 'file-extension' },
          { id: 'f3', operator: 'include', pattern: '/shared/archive/**', type: 'folder-path' }
        ],
        executionRules: [
          { id: 'r1', name: 'Retry on failure', condition: 'on_error', action: 'retry_3_times', enabled: true },
          { id: 'r2', name: 'Notify on complete', condition: 'on_completion', action: 'send_email', enabled: true }
        ],
        jobType: 'full',
        executionMode: 'scheduled',
        scheduleCron: '0 2 * * *',
        preservePermissions: true,
        deleteSourceAfterCopy: false,
        overwriteExisting: false,
        status: 'active',
        createdAt: new Date('2025-12-01'),
        updatedAt: new Date('2026-05-01')
      },
      {
        id: 'map-002',
        name: 'Eng Docs → OneDrive Sync',
        description: 'Daily incremental sync of engineering documents to OneDrive.',
        sourceProfileId: 'prof-001',
        sourceProfileName: 'Google Workspace (Primary)',
        sourcePath: '/Engineering/',
        destinationProfileId: 'prof-002',
        destinationProfileName: 'Microsoft OneDrive',
        destinationPath: '/Shared/Engineering-Mirror/',
        filters: [
          { id: 'f1', operator: 'include', pattern: '*.docx,*.xlsx,*.pdf', type: 'file-extension' }
        ],
        executionRules: [
          { id: 'r1', name: 'Skip hidden files', condition: 'always', action: 'skip_hidden', enabled: true }
        ],
        jobType: 'incremental',
        executionMode: 'scheduled',
        scheduleCron: '0 22 * * *',
        preservePermissions: false,
        deleteSourceAfterCopy: false,
        overwriteExisting: true,
        status: 'active',
        createdAt: new Date('2026-01-15'),
        updatedAt: new Date('2026-04-20')
      },
      {
        id: 'map-003',
        name: 'Marketing Legacy Archive',
        description: 'One-time full migration of legacy marketing materials.',
        sourceProfileId: 'prof-003',
        sourceProfileName: 'Legacy Google Drive',
        sourcePath: '/Marketing/',
        destinationProfileId: 'prof-002',
        destinationProfileName: 'Microsoft OneDrive',
        destinationPath: '/Archive/Marketing-2024/',
        filters: [],
        executionRules: [],
        jobType: 'full',
        executionMode: 'immediate',
        preservePermissions: true,
        deleteSourceAfterCopy: true,
        overwriteExisting: false,
        status: 'draft',
        createdAt: new Date('2026-05-05'),
        updatedAt: new Date('2026-05-10')
      }
    ];
  }

  // ── Dashboard Stats ───────────────────────────────────────────────────────
  getDashboardStats(): IDashboardStats {
    return {
      totalFilesTransferred: 2847391,
      failedFiles: 143,
      totalDataMovedGB: 1842.7,
      activeJobs: 2,
      completedJobsToday: 5,
      successRate: 99.4
    };
  }

  // ── File Log Entries ──────────────────────────────────────────────────────
  getFileLogEntries(): IFileLogEntry[] {
    const statuses: Array<'transferred' | 'failed' | 'skipped'> = ['transferred', 'transferred', 'transferred', 'transferred', 'failed', 'transferred', 'skipped', 'transferred'];
    const fileNames = [
      'annual_report_2023.pdf', 'q4_financials.xlsx', 'product_roadmap.pptx',
      'logo_assets.zip', 'video_intro.mp4', 'design_system.fig', 'user_data_dump.csv',
      'server_config.yaml', 'readme.md', 'invoice_march.pdf', 'src_backup.tar.gz',
      'meeting_notes_may.docx', 'client_contracts.pdf', 'db_schema.sql',
      'analytics_export.json', 'employee_handbook.pdf', 'brand_guidelines.pdf'
    ];
    const jobs = ['Backup Data 2023', 'Engineering Docs Sync', 'Dropbox → GWS'];

    return Array.from({ length: 20 }, (_, i) => ({
      id: `log-${i + 1}`,
      timestamp: new Date(Date.now() - i * 18000),
      fileName: fileNames[i % fileNames.length],
      filePath: `/shared/archive/2023/${fileNames[i % fileNames.length]}`,
      sizeKB: Math.floor(Math.random() * 50000) + 100,
      status: statuses[i % statuses.length],
      jobName: jobs[i % jobs.length],
      duration: Math.floor(Math.random() * 2000) + 50
    }));
  }
}
