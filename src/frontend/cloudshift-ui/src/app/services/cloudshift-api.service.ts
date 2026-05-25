import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { IAppProfile } from '../models/app-profile.model';
import { IMigrationJob } from '../models/migration-job.model';
import { IPathFilter, IProjectMapping } from '../models/project-mapping.model';
import { IDashboardStats, IFileLogEntry } from '../models/dashboard.model';

const API_BASE_URL = 'http://localhost:5132/api';
const DEMO_USER_ID = '11111111-1111-1111-1111-111111111111';

type ProviderValue = 1 | 2;
type JobTypeValue = 1 | 2;
type JobStatusValue = 1 | 2 | 3 | 4 | 5;

interface ApiAppProfile {
  id: string;
  userId: string;
  providerAppId: string | null;
  provider: ProviderValue;
  providerName: string;
  externalAccountId: string;
  email: string;
  expiresAt: string;
  createdAt: string;
}

export interface AddProfileFormValue {
  provider: 'google-drive' | 'onedrive';
  providerAppId: string;
}

interface ApiOAuthProviderApp {
  id: string;
  userId: string;
  provider: ProviderValue;
  providerName: string;
  name: string;
  clientId: string;
  tenantId: string;
  redirectUri: string;
  scopes: string;
  isActive: boolean;
  linkedProfileCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface IOAuthProviderApp {
  id: string;
  userId: string;
  provider: 'google-drive' | 'onedrive';
  providerName: string;
  name: string;
  clientId: string;
  tenantId: string;
  redirectUri: string;
  scopes: string;
  isActive: boolean;
  linkedProfileCount: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateOAuthProviderAppFormValue {
  provider: 'google-drive' | 'onedrive';
  name: string;
  clientId: string;
  clientSecret: string;
  tenantId: string;
  redirectUri: string;
  scopes: string;
}

interface CreateOAuthProviderAppRequest {
  userId: string;
  provider: ProviderValue;
  name: string;
  clientId: string;
  clientSecret: string;
  tenantId: string;
  redirectUri: string;
  scopes: string;
}

export interface UpdateOAuthProviderAppFormValue {
  provider: 'google-drive' | 'onedrive';
  name: string;
  clientId: string;
  clientSecret: string;
  tenantId: string;
  redirectUri: string;
  scopes: string;
  isActive: boolean;
}

interface UpdateOAuthProviderAppRequest {
  userId: string;
  provider: ProviderValue;
  name: string;
  clientId: string;
  clientSecret: string | null;
  tenantId: string;
  redirectUri: string;
  scopes: string;
  isActive: boolean;
}

export interface DeleteOAuthProviderAppResult {
  id: string;
  userId: string;
  deleted: boolean;
  deactivated: boolean;
  linkedProfileCount: number;
  message: string;
}

interface ApiFilterConfig {
  includeExtensions: string[];
  excludeExtensions: string[];
  maxSizeMB: number | null;
  minSizeMB: number | null;
  modifiedAfter: string | null;
  modifiedBefore: string | null;
  includeNamePatterns: string[];
}

interface ApiProjectMapping {
  id: string;
  userId: string;
  name: string;
  sourceProfileId: string;
  sourceProviderName: string;
  destProfileId: string;
  destProviderName: string;
  sourcePath: string;
  destPath: string;
  filterConfig: ApiFilterConfig;
  conflictResolutionRule: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateMappingFormValue {
  name: string;
  sourceProfileId: string;
  sourcePath: string;
  destinationProfileId: string;
  destinationPath: string;
  filters: IPathFilter[];
  jobType: 'full' | 'delta';
  preservePermissions: boolean;
  deleteSourceAfterCopy: boolean;
  overwriteExisting: boolean;
}

interface CreateProjectMappingRequest {
  userId: string;
  name: string;
  sourceProfileId: string;
  destProfileId: string;
  sourcePath: string;
  destPath: string;
  filterConfig: ApiFilterConfig;
  conflictResolutionRule: string;
}

interface ApiMigrationJob {
  id: string;
  projectMappingId: string;
  status: JobStatusValue;
  statusName: string;
  jobType: JobTypeValue;
  jobTypeName: string;
  totalItems: number;
  processedItems: number;
  startedAt: string | null;
  completedAt: string | null;
  createdAt: string;
}

@Injectable({ providedIn: 'root' })
export class CloudShiftApiService {
  readonly userId = DEMO_USER_ID;

  constructor(private http: HttpClient) {}

  getAppProfiles(): Observable<IAppProfile[]> {
    return this.http
      .get<ApiAppProfile[]>(`${API_BASE_URL}/app-profiles`, { params: { userId: this.userId } })
      .pipe(map(profiles => profiles.map(profile => this.mapProfile(profile))));
  }

  getOAuthProviderApps(): Observable<IOAuthProviderApp[]> {
    return this.http
      .get<ApiOAuthProviderApp[]>(`${API_BASE_URL}/oauth-provider-apps`, { params: { userId: this.userId } })
      .pipe(map(apps => apps.map(app => this.mapOAuthProviderApp(app))));
  }

  createOAuthProviderApp(form: CreateOAuthProviderAppFormValue): Observable<IOAuthProviderApp> {
    const request: CreateOAuthProviderAppRequest = {
      userId: this.userId,
      provider: this.toProviderValue(form.provider),
      name: form.name,
      clientId: form.clientId,
      clientSecret: form.clientSecret,
      tenantId: form.provider === 'onedrive' ? form.tenantId || 'common' : '',
      redirectUri: form.redirectUri,
      scopes: form.scopes
    };

    return this.http
      .post<ApiOAuthProviderApp>(`${API_BASE_URL}/oauth-provider-apps`, request)
      .pipe(map(app => this.mapOAuthProviderApp(app)));
  }

  updateOAuthProviderApp(id: string, form: UpdateOAuthProviderAppFormValue): Observable<IOAuthProviderApp> {
    const request: UpdateOAuthProviderAppRequest = {
      userId: this.userId,
      provider: this.toProviderValue(form.provider),
      name: form.name,
      clientId: form.clientId,
      clientSecret: form.clientSecret.trim() ? form.clientSecret : null,
      tenantId: form.provider === 'onedrive' ? form.tenantId || 'common' : '',
      redirectUri: form.redirectUri,
      scopes: form.scopes,
      isActive: form.isActive
    };

    return this.http
      .put<ApiOAuthProviderApp>(`${API_BASE_URL}/oauth-provider-apps/${id}`, request)
      .pipe(map(app => this.mapOAuthProviderApp(app)));
  }

  deleteOAuthProviderApp(id: string): Observable<DeleteOAuthProviderAppResult> {
    return this.http.delete<DeleteOAuthProviderAppResult>(
      `${API_BASE_URL}/oauth-provider-apps/${id}`,
      { params: { userId: this.userId } }
    );
  }

  startAppProfileOAuth(form: AddProfileFormValue): void {
    const params = new URLSearchParams({ userId: this.userId });
    window.location.href = `${API_BASE_URL}/app-profiles/oauth/provider-apps/${form.providerAppId}/authorize?${params.toString()}`;
  }

  getSuggestedOAuthRedirectUri(provider: 'google-drive' | 'onedrive'): string {
    const providerPath = provider === 'onedrive' ? 'onedrive' : 'google';
    return `${API_BASE_URL}/app-profiles/oauth/${providerPath}/callback`;
  }

  getProjectMappings(): Observable<IProjectMapping[]> {
    return this.http
      .get<ApiProjectMapping[]>(`${API_BASE_URL}/mappings`, { params: { userId: this.userId } })
      .pipe(map(mappings => mappings.map(mapping => this.mapProjectMapping(mapping))));
  }

  createProjectMapping(form: CreateMappingFormValue): Observable<IProjectMapping> {
    const request: CreateProjectMappingRequest = {
      userId: this.userId,
      name: form.name,
      sourceProfileId: form.sourceProfileId,
      destProfileId: form.destinationProfileId,
      sourcePath: form.sourcePath,
      destPath: form.destinationPath,
      filterConfig: this.toFilterConfig(form.filters),
      conflictResolutionRule: form.overwriteExisting ? 'Overwrite' : 'Skip'
    };

    return this.http
      .post<ApiProjectMapping>(`${API_BASE_URL}/mappings`, request)
      .pipe(map(mapping => this.mapProjectMapping(mapping)));
  }

  startMigration(mappingId: string, jobType: 'full' | 'delta' = 'full'): Observable<IMigrationJob> {
    return this.http
      .post<ApiMigrationJob>(`${API_BASE_URL}/mappings/${mappingId}/start`, {
        jobType: jobType === 'delta' ? 2 : 1
      })
      .pipe(map(job => this.mapMigrationJob(job)));
  }

  getMigrationJobs(): Observable<IMigrationJob[]> {
    return this.http
      .get<ApiMigrationJob[]>(`${API_BASE_URL}/mappings/jobs`, { params: { userId: this.userId } })
      .pipe(map(jobs => jobs.map(job => this.mapMigrationJob(job))));
  }

  getDashboardStats(): Observable<IDashboardStats> {
    return this.getMigrationJobs().pipe(
      map(jobs => {
        const completed = jobs.filter(job => job.status === 'completed').length;
        const failed = jobs.filter(job => job.status === 'failed').length;
        const active = jobs.filter(job => job.status === 'running' || job.status === 'queued').length;
        const totalProcessed = jobs.reduce((sum, job) => sum + job.filesTransferred, 0);
        const totalFinished = completed + failed;

        return {
          totalFilesTransferred: totalProcessed,
          failedFiles: failed,
          totalDataMovedGB: 0,
          activeJobs: active,
          completedJobsToday: completed,
          successRate: totalFinished === 0 ? 100 : Math.round((completed / totalFinished) * 1000) / 10
        };
      })
    );
  }

  getFileLogEntries(): Observable<IFileLogEntry[]> {
    return this.getMigrationJobs().pipe(
      map(jobs => jobs.slice(0, 20).map(job => ({
        id: `job-log-${job.id}`,
        timestamp: job.completedAt ?? job.startedAt,
        fileName: job.name,
        filePath: job.projectMappingId,
        sizeKB: 0,
        status: job.status === 'failed' ? 'failed' : job.status === 'completed' ? 'transferred' : 'skipped',
        jobName: job.name,
        duration: 0
      })))
    );
  }

  private mapProfile(profile: ApiAppProfile): IAppProfile {
    const provider = this.toProviderSlug(profile.providerName, profile.provider);

    return {
      id: profile.id,
      name: `${this.providerLabel(provider)} (${profile.email})`,
      provider,
      email: profile.email,
      status: new Date(profile.expiresAt).getTime() > Date.now() ? 'active' : 'idle',
      storageUsedGB: 0,
      storageTotalGB: 0,
      filesCount: 0,
      lastSync: new Date(profile.createdAt),
      createdAt: new Date(profile.createdAt)
    };
  }

  private mapOAuthProviderApp(app: ApiOAuthProviderApp): IOAuthProviderApp {
    return {
      id: app.id,
      userId: app.userId,
      provider: this.toOAuthProviderSlug(app.providerName, app.provider),
      providerName: app.providerName,
      name: app.name,
      clientId: app.clientId,
      tenantId: app.tenantId,
      redirectUri: app.redirectUri,
      scopes: app.scopes,
      isActive: app.isActive,
      linkedProfileCount: app.linkedProfileCount,
      createdAt: new Date(app.createdAt),
      updatedAt: new Date(app.updatedAt)
    };
  }

  private mapProjectMapping(mapping: ApiProjectMapping): IProjectMapping {
    return {
      id: mapping.id,
      name: mapping.name,
      sourceProfileId: mapping.sourceProfileId,
      sourceProfileName: mapping.sourceProviderName,
      sourcePath: mapping.sourcePath,
      destinationProfileId: mapping.destProfileId,
      destinationProfileName: mapping.destProviderName,
      destinationPath: mapping.destPath,
      filters: this.fromFilterConfig(mapping.filterConfig),
      executionRules: [],
      jobType: 'full',
      executionMode: 'immediate',
      preservePermissions: false,
      deleteSourceAfterCopy: false,
      overwriteExisting: mapping.conflictResolutionRule === 'Overwrite',
      status: 'active',
      createdAt: new Date(mapping.createdAt),
      updatedAt: new Date(mapping.updatedAt)
    };
  }

  private mapMigrationJob(job: ApiMigrationJob): IMigrationJob {
    const progress = job.totalItems === 0 ? 0 : Math.round((job.processedItems / job.totalItems) * 100);
    const startedAt = new Date(job.startedAt ?? job.createdAt);

    return {
      id: job.id,
      projectMappingId: job.projectMappingId,
      name: `Migration ${job.id.slice(0, 8)}`,
      sourceProfileId: '',
      sourceProfileName: 'Source',
      destinationProfileId: '',
      destinationProfileName: 'Destination',
      status: this.toJobStatus(job.statusName),
      progress,
      filesTransferred: job.processedItems,
      totalFiles: job.totalItems,
      dataTransferredGB: 0,
      totalDataGB: 0,
      startedAt,
      completedAt: job.completedAt ? new Date(job.completedAt) : undefined,
      jobType: job.jobTypeName.toLowerCase() === 'delta' ? 'delta' : 'full',
      priority: 'normal'
    };
  }

  private toProviderValue(provider: 'google-drive' | 'onedrive'): ProviderValue {
    return provider === 'onedrive' ? 2 : 1;
  }

  private toProviderSlug(providerName: string, provider: ProviderValue): IAppProfile['provider'] {
    if (providerName === 'OneDrive' || provider === 2) return 'onedrive';
    return 'google-drive';
  }

  private toOAuthProviderSlug(providerName: string, provider: ProviderValue): IOAuthProviderApp['provider'] {
    if (providerName === 'OneDrive' || provider === 2) return 'onedrive';
    return 'google-drive';
  }

  private providerLabel(provider: IAppProfile['provider']): string {
    return provider === 'onedrive' ? 'OneDrive' : 'Google Drive';
  }

  private toJobStatus(statusName: string): IMigrationJob['status'] {
    const normalized = statusName.toLowerCase();
    if (normalized === 'processing') return 'running';
    if (normalized === 'queued') return 'queued';
    if (normalized === 'completed') return 'completed';
    if (normalized === 'failed') return 'failed';
    if (normalized === 'paused') return 'paused';
    return 'pending';
  }

  private toFilterConfig(filters: IPathFilter[]): ApiFilterConfig {
    return {
      includeExtensions: filters
        .filter(filter => filter.operator === 'include' && filter.type === 'file-extension')
        .map(filter => filter.pattern),
      excludeExtensions: filters
        .filter(filter => filter.operator === 'exclude' && filter.type === 'file-extension')
        .map(filter => filter.pattern),
      maxSizeMB: null,
      minSizeMB: null,
      modifiedAfter: null,
      modifiedBefore: null,
      includeNamePatterns: filters
        .filter(filter => filter.operator === 'include' && filter.type !== 'file-extension')
        .map(filter => filter.pattern)
    };
  }

  private fromFilterConfig(config: ApiFilterConfig): IPathFilter[] {
    return [
      ...config.includeExtensions.map((pattern, index) => ({
        id: `include-ext-${index}`,
        operator: 'include' as const,
        pattern,
        type: 'file-extension' as const
      })),
      ...config.excludeExtensions.map((pattern, index) => ({
        id: `exclude-ext-${index}`,
        operator: 'exclude' as const,
        pattern,
        type: 'file-extension' as const
      })),
      ...config.includeNamePatterns.map((pattern, index) => ({
        id: `include-name-${index}`,
        operator: 'include' as const,
        pattern,
        type: 'folder-path' as const
      }))
    ];
  }
}
