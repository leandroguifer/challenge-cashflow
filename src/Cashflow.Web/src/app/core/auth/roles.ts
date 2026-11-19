export type Role = 'caixa' | 'auxiliar' | 'proprietario';

export const ROLE_LABEL: Record<Role, string> = {
  caixa: 'Caixa',
  auxiliar: 'Auxiliar administrativo',
  proprietario: 'Proprietário',
};

export const ALL_ROLES: readonly Role[] = ['caixa', 'auxiliar', 'proprietario'];
