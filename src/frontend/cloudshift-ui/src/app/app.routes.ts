import { Routes } from '@angular/router';
import { ShellComponent } from './core/layout/shell/shell.component';

export const routes: Routes = [
  {
    path: '',
    component: ShellComponent,
    children: [
      {
        path: '',
        redirectTo: 'app-profiles',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
        title: 'Dashboard — CloudMigrator'
      },
      {
        path: 'app-profiles',
        loadComponent: () =>
          import('./features/app-profiles/app-profiles.component').then(m => m.AppProfilesComponent),
        title: 'App Profiles — CloudMigrator'
      },
      {
        path: 'project-mapping',
        loadComponent: () =>
          import('./features/project-mapping/project-mapping.component').then(m => m.ProjectMappingComponent),
        title: 'Project Mapping — CloudMigrator'
      },
      {
        path: 'migration-jobs',
        loadComponent: () =>
          import('./features/migration-jobs/migration-jobs.component').then(m => m.MigrationJobsComponent),
        title: 'Migration Jobs — CloudMigrator'
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./features/settings/settings.component').then(m => m.SettingsComponent),
        title: 'Settings — CloudMigrator'
      },
    ]
  },
  {
    path: '**',
    redirectTo: 'app-profiles'
  }
];
