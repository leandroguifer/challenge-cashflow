import { inject, Injectable } from '@angular/core';
import { CashStatus, DayCash, EntriesApiService } from './entries-api.service';

export interface DailyBalance {
  readonly credits: number;
  readonly debits: number;
  readonly balance: number;
  readonly status: CashStatus;
}

@Injectable({ providedIn: 'root' })
export class BalanceApiService {
  private readonly entries = inject(EntriesApiService);

  daily(storeId: string): DailyBalance {
    return summarize(this.entries.day(storeId));
  }
}

export function summarize(day: DayCash): DailyBalance {
  const credits = day.entries.filter((e) => e.type === 'entrada').reduce((sum, e) => sum + e.amount, 0);
  const debits = day.entries.filter((e) => e.type === 'saida').reduce((sum, e) => sum + e.amount, 0);
  return { credits, debits, balance: credits - debits, status: day.status };
}
