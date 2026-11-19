import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TuiAmountPipe } from '@taiga-ui/addon-commerce';
import { TuiButton, TuiDialog, TuiTextfield } from '@taiga-ui/core';
import { TuiInputNumber, TuiStatus, TuiTextarea } from '@taiga-ui/kit';
import { TuiBlockStatus, TuiCardLarge } from '@taiga-ui/layout';
import { summarize } from '../../core/api/balance-api.service';
import { EntriesApiService, EntryType } from '../../core/api/entries-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { EntriesTable, EntryRow } from '../../shared/entries-table/entries-table';
import { STATUS_COLOR, STATUS_LABEL } from '../../shared/status';

@Component({
  selector: 'app-caixa',
  imports: [
    FormsModule,
    TuiAmountPipe,
    TuiButton,
    TuiDialog,
    TuiTextfield,
    TuiInputNumber,
    TuiTextarea,
    TuiStatus,
    TuiBlockStatus,
    TuiCardLarge,
    EntriesTable,
  ],
  templateUrl: './caixa.html',
  styleUrl: './caixa.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Caixa {
  private readonly api = inject(EntriesApiService);
  private readonly auth = inject(AuthService);

  protected readonly statusLabel = STATUS_LABEL;
  protected readonly statusColor = STATUS_COLOR;
  protected readonly today = new Date().toLocaleDateString('pt-BR');

  protected readonly store = computed(() => this.auth.stores()[0] ?? { id: '', name: '' });
  protected readonly day = computed(() => this.api.days()[this.store().id] ?? this.api.day(this.store().id));
  protected readonly summary = computed(() => summarize(this.day()));
  protected readonly rows = computed<readonly EntryRow[]>(() =>
    this.day().entries.map((entry) => ({ ...entry, store: this.store().name })),
  );

  protected readonly dialogType = signal<EntryType | null>(null);
  protected amount: number | null = null;
  protected description = '';

  protected get dialogOpen(): boolean {
    return this.dialogType() !== null;
  }

  protected set dialogOpen(open: boolean) {
    if (!open) {
      this.dialogType.set(null);
    }
  }

  protected get canRegister(): boolean {
    return (this.amount ?? 0) > 0 && this.description.trim().length > 0;
  }

  protected open(): void {
    this.api.openCash(this.store().id);
  }

  protected close(): void {
    this.api.closeCash(this.store().id);
  }

  protected startEntry(type: EntryType): void {
    this.amount = null;
    this.description = '';
    this.dialogType.set(type);
  }

  protected register(): void {
    const type = this.dialogType();
    if (type && this.canRegister) {
      this.api.register(this.store().id, type, this.amount ?? 0, this.description.trim());
      this.dialogType.set(null);
    }
  }
}
