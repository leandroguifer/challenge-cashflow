import { ChangeDetectionStrategy, Component, computed, inject, resource } from '@angular/core';
import { TuiAmountPipe } from '@taiga-ui/addon-commerce';
import { TuiButton } from '@taiga-ui/core';
import { TuiBlockStatus, TuiCardLarge } from '@taiga-ui/layout';
import { BalanceApiService } from '../../core/api/balance-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { ALL_STORES, StoreContext } from '../../core/store/store-context';
import { isoDate } from '../../shared/date';

@Component({
  selector: 'app-saldo',
  imports: [TuiAmountPipe, TuiButton, TuiBlockStatus, TuiCardLarge],
  templateUrl: './saldo.html',
  styleUrl: './saldo.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Saldo {
  private readonly api = inject(BalanceApiService);
  private readonly auth = inject(AuthService);
  private readonly storeContext = inject(StoreContext);

  protected readonly today = new Date().toLocaleDateString('pt-BR');

  protected readonly balances = resource({
    params: () => this.storeContext.selected(),
    loader: ({ params }) => {
      const date = isoDate(new Date());
      return this.api.list(date, date, params === ALL_STORES ? undefined : params);
    },
  });

  protected readonly cards = computed(() => {
    const items = this.balances.value() ?? [];
    return this.storeContext.visible().map((store) => {
      const item = items.find((entry) => entry.storeId === store.id);
      return {
        store,
        credits: item?.credits ?? 0,
        debits: item?.debits ?? 0,
        balance: item?.balance ?? 0,
        entryCount: item?.entryCount ?? 0,
      };
    });
  });

  protected readonly total = computed(() => this.cards().reduce((sum, card) => sum + card.balance, 0));
  protected readonly showTotal = computed(() => this.auth.hasAnyRole(['proprietario']));
}
