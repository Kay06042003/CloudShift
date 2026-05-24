export type FilterOperator = 'include' | 'exclude';
export type ExecutionMode = 'immediate' | 'scheduled' | 'triggered';
export type JobType = 'full' | 'incremental' | 'delta';

export interface IPathFilter {
  id: string;
  operator: FilterOperator;
  pattern: string;
  type: 'file-extension' | 'folder-path' | 'file-size' | 'date-range';
}

export interface IExecutionRule {
  id: string;
  name: string;
  condition: string;
  action: string;
  enabled: boolean;
}

export interface IProjectMapping {
  id: string;
  name: string;
  description?: string;
  sourceProfileId: string;
  sourceProfileName: string;
  sourcePath: string;
  destinationProfileId: string;
  destinationProfileName: string;
  destinationPath: string;
  filters: IPathFilter[];
  executionRules: IExecutionRule[];
  jobType: JobType;
  executionMode: ExecutionMode;
  scheduleCron?: string;
  preservePermissions: boolean;
  deleteSourceAfterCopy: boolean;
  overwriteExisting: boolean;
  status: 'active' | 'draft' | 'archived';
  createdAt: Date;
  updatedAt: Date;
}
