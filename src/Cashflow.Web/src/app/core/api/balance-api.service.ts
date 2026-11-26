import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AppConfigService } from '../config/app-config';

export interface DailyBalance {
  readonly storeId: string;
  readonly businessDate: string;
  readonly credits: number;
  readonly debits: number;
  readonly balance: number;
  readonly entryCount: number;
}

export type CompanyDailyBalance = Omit<DailyBalance, 'storeId'>;

interface DailyBalanceDto {
  readonly storeId: string;
  readonly businessDate: string;
  readonly totalCredits: number;
  readonly totalDebits: number;
  readonly balance: number;
  readonly entryCount: number;
}

@Injectable({ providedIn: 'root' })
export class BalanceApiService {
  private readonly http = inject(HttpClient);
  private readonly config = inject(AppConfigService);

  async list(startDate: string, endDate: string, storeId?: string): Promise<readonly DailyBalance[]> {
    let params = new HttpParams().set('startDate', startDate).set('endDate', endDate);
    if (storeId) {
      params = params.set('storeId', storeId);
    }

    const items = await firstValueFrom(
      this.http.get<readonly DailyBalanceDto[]>(`${this.config.config.balanceApiUrl}/api/v1/daily-balances`, { params }),
    );

    return items.map((dto) => ({
      storeId: dto.storeId,
      businessDate: dto.businessDate,
      credits: dto.totalCredits,
      debits: dto.totalDebits,
      balance: dto.balance,
      entryCount: dto.entryCount,
    }));
  }

  async listCompany(startDate: string, endDate: string): Promise<readonly CompanyDailyBalance[]> {
    const params = new HttpParams().set('startDate', startDate).set('endDate', endDate);

    const items = await firstValueFrom(
      this.http.get<readonly Omit<DailyBalanceDto, 'storeId'>[]>(
        `${this.config.config.balanceApiUrl}/api/v1/company/daily-balances`,
        { params },
      ),
    );

    return items.map((dto) => ({
      businessDate: dto.businessDate,
      credits: dto.totalCredits,
      debits: dto.totalDebits,
      balance: dto.balance,
      entryCount: dto.entryCount,
    }));
  }
}
