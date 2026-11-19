import { ChangeDetectionStrategy, Component, computed, inject, resource, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TuiDay, TuiDayRange } from '@taiga-ui/cdk';
import { TuiAmountPipe } from '@taiga-ui/addon-commerce';
import { TuiTable } from '@taiga-ui/addon-table';
import { TuiButton, TuiDropdown, TuiTextfield } from '@taiga-ui/core';
import { TuiCalendarRange, TuiInputDateRange } from '@taiga-ui/kit';
import { TuiBlockStatus, TuiCardLarge } from '@taiga-ui/layout';
import { BalanceApiService } from '../../core/api/balance-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { ALL_STORES, StoreContext } from '../../core/store/store-context';
import { isoDate } from '../../shared/date';

@Component({
  selector: 'app-relatorio',
  imports: [
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
  private readonly auth = inject(AuthService);
  private readonly storeContext = inject(StoreContext);

  protected readonly storeLabel = this.storeContext.label;
  protected readonly columns = ['date', 'store', 'credits', 'debits', 'balance', 'entries'];
  protected readonly period = signal(new TuiDayRange(TuiDay.currentLocal().append({ day: -6 }), TuiDay.currentLocal()));

  protected readonly report = resource({
    params: () => ({ store: this.storeContext.selected(), period: this.period() }),
    loader: ({ params }) =>
      this.api.list(
        isoDate(params.period.from.toLocalNativeDate()),
        isoDate(params.period.to.toLocalNativeDate()),
        params.store === ALL_STORES ? undefined : params.store,
      ),
  });

  protected readonly rows = computed(() =>
    (this.report.value() ?? []).map((item) => ({
      ...item,
      date: item.businessDate.split('-').reverse().join('/'),
      store: this.auth.stores().find((store) => store.id === item.storeId)?.name ?? item.storeId,
    })),
  );

  protected readonly summary = computed(() => {
    const items = this.report.value() ?? [];
    const credits = items.reduce((sum, item) => sum + item.credits, 0);
    const debits = items.reduce((sum, item) => sum + item.debits, 0);
    return { credits, debits, balance: credits - debits };
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
