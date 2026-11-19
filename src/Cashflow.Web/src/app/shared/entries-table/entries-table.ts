import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { TuiTable } from '@taiga-ui/addon-table';
import { TuiAmountPipe } from '@taiga-ui/addon-commerce';
import { TuiStatus } from '@taiga-ui/kit';
import { Entry } from '../../core/api/entries-api.service';

export interface EntryRow extends Entry {
  readonly store: string;
}

@Component({
  selector: 'app-entries-table',
  imports: [TuiTable, TuiAmountPipe, TuiStatus],
  templateUrl: './entries-table.html',
  styles: ':host { display: block } table { inline-size: 100% }',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EntriesTable {
  readonly rows = input.required<readonly EntryRow[]>();
  readonly showStore = input(false);

  protected readonly columns = computed(() =>
    this.showStore()
      ? ['store', 'time', 'type', 'description', 'amount']
      : ['time', 'type', 'description', 'amount'],
  );
}
