export type CloudProvider =
  | 'google-workspace'
  | 'google-drive'
  | 'onedrive'
  | 'sharepoint'
  | 'dropbox'
  | 'box'
  | 's3';

export type ProfileStatus = 'active' | 'idle' | 'error' | 'connecting';

export interface IAppProfile {
  id: string;
  name: string;
  provider: CloudProvider;
  email: string;
  status: ProfileStatus;
  storageUsedGB: number;
  storageTotalGB: number;
  filesCount: number;
  lastSync: Date;
  createdAt: Date;
}
