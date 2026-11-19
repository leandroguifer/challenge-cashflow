import { Routes } from '@angular/router';
import { authGuard, homeGuard } from './core/auth/auth.guard';
import { Shell } from './core/layout/shell/shell';

export const routes: Routes = [
  {
    path: '',
    component: Shell,
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    children: [
      { path: '', pathMatch: 'full', canActivate: [homeGuard], children: [] },
      {
        path: 'caixa',
        data: { roles: ['caixa'], section: 'Lançamentos', title: 'Caixa do dia' },
        loadComponent: () => import('./pages/caixa/caixa').then((m) => m.Caixa),
      },
      {
        path: 'saldo',
        data: { roles: ['auxiliar', 'proprietario'], section: 'Consolidado', title: 'Saldo diário' },
        loadComponent: () => import('./pages/saldo/saldo').then((m) => m.Saldo),
      },
      {
        path: 'relatorio',
        data: { roles: ['auxiliar', 'proprietario'], section: 'Consolidado', title: 'Relatório' },
        loadComponent: () => import('./pages/relatorio/relatorio').then((m) => m.Relatorio),
      },
      {
        path: 'acesso-negado',
        data: { title: 'Acesso negado' },
        loadComponent: () => import('./pages/acesso-negado/acesso-negado').then((m) => m.AcessoNegado),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
