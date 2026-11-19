import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { TuiAmountPipe } from '@taiga-ui/addon-commerce';
import { TuiStatus } from '@taiga-ui/kit';
import { TuiCardLarge } from '@taiga-ui/layout';
import { BalanceApiService } from '../../core/api/balance-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { StoreContext } from '../../core/store/store-context';
import { STATUS_COLOR, STATUS_LABEL } from '../../shared/status';

@Component({
  selector: 'app-saldo',
  imports: [TuiAmountPipe, TuiStatus, TuiCardLarge],
  templateUrl: './saldo.html',
  styleUrl: './saldo.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Saldo {
  private readonly api = inject(BalanceApiService);
  private readonly auth = inject(AuthService);
  private readonly storeContext = inject(StoreContext);

  protected readonly statusLabel = STATUS_LABEL;
  protected readonly statusColor = STATUS_COLOR;
  protected readonly today = new Date().toLocaleDateString('pt-BR');

  protected readonly cards = computed(() =>
    this.storeContext.visible().map((store) => ({ store, balance: this.api.daily(store.id) })),
  );

  protected readonly total = computed(() => this.cards().reduce((sum, card) => sum + card.balance.balance, 0));
  protected readonly showTotal = computed(() => this.auth.hasAnyRole(['proprietario']));
}
