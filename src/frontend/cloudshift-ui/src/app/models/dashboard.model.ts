export interface IFileLogEntry {
  id: string;
  timestamp: Date;
  fileName: string;
  filePath: string;
  sizeKB: number;
  status: 'transferred' | 'failed' | 'skipped';
  jobName: string;
  duration: number; // ms
}

export interface IDashboardStats {
  totalFilesTransferred: number;
  failedFiles: number;
  totalDataMovedGB: number;
  activeJobs: number;
  completedJobsToday: number;
  successRate: number;
}
