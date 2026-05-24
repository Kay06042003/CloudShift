export type JobStatus = 'running' | 'completed' | 'failed' | 'pending' | 'paused' | 'queued';

export interface IMigrationJob {
  id: string;
  projectMappingId: string;
  name: string;
  sourceProfileId: string;
  sourceProfileName: string;
  destinationProfileId: string;
  destinationProfileName: string;
  status: JobStatus;
  progress: number; // 0-100
  filesTransferred: number;
  totalFiles: number;
  dataTransferredGB: number;
  totalDataGB: number;
  startedAt: Date;
  estimatedCompletionAt?: Date;
  completedAt?: Date;
  errorMessage?: string;
  jobType: 'full' | 'incremental' | 'delta';
  priority: 'low' | 'normal' | 'high' | 'critical';
}
