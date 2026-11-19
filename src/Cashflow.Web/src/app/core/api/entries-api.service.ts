import { Injectable, signal } from '@angular/core';

export type EntryType = 'entrada' | 'saida';
export type CashStatus = 'nao-aberto' | 'aberto' | 'fechado';

export interface Entry {
  readonly id: number;
  readonly time: string;
  readonly type: EntryType;
  readonly description: string;
  readonly amount: number;
}

export interface DayCash {
  readonly status: CashStatus;
  readonly entries: readonly Entry[];
}

const EMPTY_DAY: DayCash = { status: 'nao-aberto', entries: [] };

const SEED: Record<string, DayCash> = {
  LOJA01: {
    status: 'aberto',
    entries: [
      { id: 1, time: '09:12', type: 'entrada', description: 'Troca de óleo', amount: 180 },
      { id: 2, time: '10:40', type: 'saida', description: 'Compra de filtros', amount: 90 },
      { id: 3, time: '11:25', type: 'entrada', description: 'Alinhamento e balanceamento', amount: 220 },
    ],
  },
  LOJA02: {
    status: 'aberto',
    entries: [
      { id: 4, time: '08:50', type: 'entrada', description: 'Revisão completa', amount: 650 },
      { id: 5, time: '11:05', type: 'saida', description: 'Peças', amount: 320 },
    ],
  },
  LOJA03: {
    status: 'fechado',
    entries: [
      { id: 6, time: '09:30', type: 'entrada', description: 'Troca de pastilhas', amount: 2300 },
      { id: 7, time: '15:10', type: 'saida', description: 'Fornecedor de peças', amount: 200 },
    ],
  },
};

@Injectable({ providedIn: 'root' })
export class EntriesApiService {
  private nextId = 100;
  private readonly state = signal<Record<string, DayCash>>(SEED);

  readonly days = this.state.asReadonly();

  day(storeId: string): DayCash {
    return this.state()[storeId] ?? EMPTY_DAY;
  }

  openCash(storeId: string): void {
    this.update(storeId, () => ({ status: 'aberto', entries: [] }));
  }

  closeCash(storeId: string): void {
    this.update(storeId, (day) => ({ ...day, status: 'fechado' }));
  }

  register(storeId: string, type: EntryType, amount: number, description: string): void {
    const time = new Date().toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
    const entry: Entry = { id: this.nextId++, time, type, description, amount };
    this.update(storeId, (day) => ({ ...day, entries: [...day.entries, entry] }));
  }

  private update(storeId: string, change: (day: DayCash) => DayCash): void {
    this.state.update((current) => ({ ...current, [storeId]: change(current[storeId] ?? EMPTY_DAY) }));
  }
}
