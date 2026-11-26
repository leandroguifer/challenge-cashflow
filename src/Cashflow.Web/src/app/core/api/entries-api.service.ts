import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AppConfigService } from '../config/app-config';

export type EntryType = 'entrada' | 'saida';
export type CashStatus = 'nao-aberto' | 'aberto' | 'fechado';

export interface Entry {
  readonly id: string;
  readonly time: string;
  readonly type: EntryType;
  readonly description: string;
  readonly amount: number;
}

export interface DayCash {
  readonly status: CashStatus;
  readonly entries: readonly Entry[];
}

interface CashRegisterDto {
  readonly status: 'Open' | 'Closed';
}

interface EntryDto {
  readonly id: string;
  readonly type: 'Credit' | 'Debit';
  readonly amount: number;
  readonly description: string | null;
  readonly createdAt: string;
}

interface CashRegisterDayDto {
  readonly cashRegister: CashRegisterDto | null;
  readonly entries: readonly EntryDto[];
}

const TYPE_TO_API: Record<EntryType, EntryDto['type']> = { entrada: 'Credit', saida: 'Debit' };

function toEntry(dto: EntryDto): Entry {
  return {
    id: dto.id,
    time: new Date(dto.createdAt).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' }),
    type: dto.type === 'Credit' ? 'entrada' : 'saida',
    description: dto.description ?? '',
    amount: dto.amount,
  };
}

function toDay(dto: CashRegisterDayDto): DayCash {
  if (!dto.cashRegister) {
    return { status: 'nao-aberto', entries: [] };
  }
  return {
    status: dto.cashRegister.status === 'Open' ? 'aberto' : 'fechado',
    entries: dto.entries.map(toEntry),
  };
}

@Injectable({ providedIn: 'root' })
export class EntriesApiService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(AppConfigService);
  private readonly state = signal<Record<string, DayCash>>({});

  readonly days = this.state.asReadonly();

  async load(storeId: string): Promise<void> {
    const dto = await firstValueFrom(this.http.get<CashRegisterDayDto>(`${this.url(storeId)}/cash-register/today`));
    this.state.update((current) => ({ ...current, [storeId]: toDay(dto) }));
  }

  async list(storeId: string, date: string): Promise<readonly Entry[]> {
    const items = await firstValueFrom(
      this.http.get<readonly EntryDto[]>(`${this.url(storeId)}/entries`, { params: { date } }),
    );
    return items.map(toEntry);
  }

  async openCash(storeId: string): Promise<void> {
    await firstValueFrom(this.http.post(`${this.url(storeId)}/cash-register/open`, null));
    await this.load(storeId);
  }

  async closeCash(storeId: string): Promise<void> {
    await firstValueFrom(this.http.post(`${this.url(storeId)}/cash-register/close`, null));
    await this.load(storeId);
  }

  async register(storeId: string, type: EntryType, amount: number, description: string): Promise<void> {
    await firstValueFrom(
      this.http.post(`${this.url(storeId)}/entries`, { type: TYPE_TO_API[type], amount, description }),
    );
    await this.load(storeId);
  }

  private url(storeId: string): string {
    return `${this.config.config.entriesApiUrl}/api/v1/stores/${encodeURIComponent(storeId)}`;
  }
}

export interface DaySummary {
  readonly credits: number;
  readonly debits: number;
  readonly balance: number;
}

export function summarize(day: DayCash): DaySummary {
  const credits = day.entries.filter((e) => e.type === 'entrada').reduce((sum, e) => sum + e.amount, 0);
  const debits = day.entries.filter((e) => e.type === 'saida').reduce((sum, e) => sum + e.amount, 0);
  return { credits, debits, balance: credits - debits };
}
