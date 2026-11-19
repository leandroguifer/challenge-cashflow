import { CashStatus } from '../core/api/entries-api.service';

export const STATUS_LABEL: Record<CashStatus, string> = {
  'nao-aberto': 'Não aberto',
  aberto: 'Aberto',
  fechado: 'Fechado',
};

export const STATUS_COLOR: Record<CashStatus, string> = {
  'nao-aberto': 'var(--tui-text-tertiary)',
  aberto: 'var(--tui-status-positive)',
  fechado: 'var(--tui-status-warning)',
};
