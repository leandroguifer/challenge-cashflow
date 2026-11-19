import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TuiAmountPipe } from '@taiga-ui/addon-commerce';
import { TuiNotificationService, TuiButton, TuiDialog, TuiTextfield } from '@taiga-ui/core';
import { TuiInputNumber, TuiStatus, TuiTextarea } from '@taiga-ui/kit';
import { TuiBlockStatus, TuiCardLarge } from '@taiga-ui/layout';
import { DayCash, EntriesApiService, EntryType, summarize } from '../../core/api/entries-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { EntriesTable, EntryRow } from '../../shared/entries-table/entries-table';
import { STATUS_COLOR, STATUS_LABEL } from '../../shared/status';

const EMPTY_DAY: DayCash = { status: 'nao-aberto', entries: [] };

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
  private readonly notifications = inject(TuiNotificationService);

  protected readonly statusLabel = STATUS_LABEL;
  protected readonly statusColor = STATUS_COLOR;
  protected readonly today = new Date().toLocaleDateString('pt-BR');

  protected readonly store = computed(() => this.auth.stores()[0] ?? { id: '', name: '' });
  protected readonly day = computed(() => this.api.days()[this.store().id] ?? null);
  protected readonly summary = computed(() => summarize(this.day() ?? EMPTY_DAY));
  protected readonly rows = computed<readonly EntryRow[]>(() =>
    (this.day()?.entries ?? []).map((entry) => ({ ...entry, store: this.store().name })),
  );

  protected readonly busy = signal(false);
  protected readonly dialogType = signal<EntryType | null>(null);
  protected amount: number | null = null;
  protected description = '';

  constructor() {
    void this.run(() => this.api.load(this.store().id));
  }

  protected get dialogOpen(): boolean {
    return this.dialogType() !== null;
  }

  protected set dialogOpen(open: boolean) {
    if (!open) {
      this.dialogType.set(null);
    }
  }

  protected get canRegister(): boolean {
    return (this.amount ?? 0) > 0 && this.description.trim().length > 0 && !this.busy();
  }

  protected open(): void {
    void this.run(() => this.api.openCash(this.store().id));
  }

  protected close(): void {
    void this.run(() => this.api.closeCash(this.store().id));
  }

  protected startEntry(type: EntryType): void {
    this.amount = null;
    this.description = '';
    this.dialogType.set(type);
  }

  protected async register(): Promise<void> {
    const type = this.dialogType();
    if (!type || !this.canRegister) {
      return;
    }
    const done = await this.run(() =>
      this.api.register(this.store().id, type, this.amount ?? 0, this.description.trim()),
    );
    if (done) {
      this.dialogType.set(null);
    }
  }

  private async run(action: () => Promise<void>): Promise<boolean> {
    this.busy.set(true);
    try {
      await action();
      return true;
    } catch (error) {
      const message = error instanceof HttpErrorResponse ? error.error?.title : null;
      this.notifications.open(message ?? 'Não foi possível concluir a operação.', { appearance: 'negative' }).subscribe();
      return false;
    } finally {
      this.busy.set(false);
    }
  }
}
