import { Role } from '../auth/roles';

export interface MenuItem {
  readonly label: string;
  readonly icon: string;
  readonly route: string;
  readonly roles: readonly Role[];
}

export interface MenuGroup {
  readonly label: string;
  readonly icon: string;
  readonly items: readonly MenuItem[];
}

export const MENU: readonly MenuGroup[] = [
  {
    label: 'Lançamentos',
    icon: '@tui.wallet',
    items: [{ label: 'Caixa do dia', icon: '@tui.banknote', route: '/caixa', roles: ['caixa'] }],
  },
  {
    label: 'Consolidado',
    icon: '@tui.chart-column',
    items: [
      { label: 'Saldo diário', icon: '@tui.landmark', route: '/saldo', roles: ['auxiliar', 'proprietario'] },
      { label: 'Relatório', icon: '@tui.file-text', route: '/relatorio', roles: ['auxiliar', 'proprietario'] },
    ],
  },
];
