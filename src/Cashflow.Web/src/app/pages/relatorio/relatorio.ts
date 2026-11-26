import { ChangeDetectionStrategy, Component, computed, inject, resource, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TuiDay, TuiDayRange } from '@taiga-ui/cdk';
import { TuiAmountPipe } from '@taiga-ui/addon-commerce';
import { TuiTable } from '@taiga-ui/addon-table';
import { TuiButton, TuiDropdown, TuiTextfield } from '@taiga-ui/core';
import { TuiCalendarRange, TuiInputDateRange } from '@taiga-ui/kit';
import { TuiBlockStatus, TuiCardLarge } from '@taiga-ui/layout';
import { BalanceApiService } from '../../core/api/balance-api.service';
import { EntriesApiService } from '../../core/api/entries-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { ALL_STORES, StoreContext } from '../../core/store/store-context';
import { isoDate } from '../../shared/date';
import { EntriesTable, EntryRow } from '../../shared/entries-table/entries-table';

@Component({
  selector: 'app-relatorio',
  imports: [
    EntriesTable,
    FormsModule,
    TuiAmountPipe,
    TuiButton,
    TuiBlockStatus,
    TuiCalendarRange,
    TuiCardLarge,
    TuiDropdown,
    TuiInputDateRange,
    TuiTable,
    TuiTextfield,
  ],
  templateUrl: './relatorio.html',
  styleUrl: './relatorio.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Relatorio {
  private readonly api = inject(BalanceApiService);
  private readonly entriesApi = inject(EntriesApiService);
  private readonly auth = inject(AuthService);
  private readonly storeContext = inject(StoreContext);

  protected readonly storeLabel = this.storeContext.label;
  protected readonly columns = ['date', 'store', 'credits', 'debits', 'balance', 'entries'];
  protected readonly period = signal(new TuiDayRange(TuiDay.currentLocal().append({ day: -6 }), TuiDay.currentLocal()));

  protected readonly report = resource({
    params: () => ({ store: this.storeContext.selected(), period: this.period() }),
    loader: async ({ params }) => {
      const start = isoDate(params.period.from.toLocalNativeDate());
      const end = isoDate(params.period.to.toLocalNativeDate());
      const allStores = params.store === ALL_STORES;
      const [items, company] = await Promise.all([
        this.api.list(start, end, allStores ? undefined : params.store),
        allStores ? this.api.listCompany(start, end) : null,
      ]);
      return { items, totals: company ?? items };
    },
  });

  protected readonly entriesDay = computed(() =>
    this.period().to.toLocalNativeDate().toLocaleDateString('pt-BR'),
  );
  protected readonly showStore = computed(() => this.storeContext.visible().length > 1);

  protected readonly entries = resource({
    params: () => ({
      stores: this.storeContext.visible(),
      date: isoDate(this.period().to.toLocalNativeDate()),
    }),
    loader: async ({ params }): Promise<readonly EntryRow[]> => {
      const byStore = await Promise.all(
        params.stores.map(async (store) => {
          const entries = await this.entriesApi.list(store.id, params.date);
          return entries.map((entry) => ({ ...entry, store: store.name }));
        }),
      );
      return byStore.flat();
    },
  });

  protected readonly rows = computed(() =>
    (this.report.value()?.items ?? []).map((item) => ({
      ...item,
      date: item.businessDate.split('-').reverse().join('/'),
      store: this.auth.stores().find((store) => store.id === item.storeId)?.name ?? item.storeId,
    })),
  );

  protected readonly summary = computed(() => {
    const totals = this.report.value()?.totals ?? [];
    return {
      credits: totals.reduce((sum, item) => sum + item.credits, 0),
      debits: totals.reduce((sum, item) => sum + item.debits, 0),
      balance: totals.reduce((sum, item) => sum + item.balance, 0),
    };
  });

  protected get range(): TuiDayRange {
    return this.period();
  }

  protected set range(value: TuiDayRange | null) {
    if (value) {
      this.period.set(value);
    }
  }
}
