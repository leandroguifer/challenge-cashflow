import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { TuiAmountPipe } from '@taiga-ui/addon-commerce';
import { TuiBlockStatus, TuiCardLarge } from '@taiga-ui/layout';
import { summarize } from '../../core/api/balance-api.service';
import { EntriesApiService } from '../../core/api/entries-api.service';
import { StoreContext } from '../../core/store/store-context';
import { EntriesTable, EntryRow } from '../../shared/entries-table/entries-table';

@Component({
  selector: 'app-relatorio',
  imports: [TuiAmountPipe, TuiBlockStatus, TuiCardLarge, EntriesTable],
  templateUrl: './relatorio.html',
  styleUrl: './relatorio.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Relatorio {
  private readonly api = inject(EntriesApiService);
  private readonly storeContext = inject(StoreContext);

  protected readonly today = new Date().toLocaleDateString('pt-BR');
  protected readonly storeLabel = this.storeContext.label;

  protected readonly rows = computed<readonly EntryRow[]>(() =>
    this.storeContext
      .visible()
      .flatMap((store) => this.api.days()[store.id]?.entries.map((entry) => ({ ...entry, store: store.name })) ?? [])
      .sort((a, b) => a.time.localeCompare(b.time)),
  );

  protected readonly showStore = computed(() => this.storeContext.visible().length > 1);
  protected readonly summary = computed(() => summarize({ status: 'aberto', entries: this.rows() }));
}
