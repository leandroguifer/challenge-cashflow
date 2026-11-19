import { computed, inject, Injectable, signal } from '@angular/core';
import { AuthService, Store } from '../auth/auth.service';

export const ALL_STORES = 'all';

@Injectable({ providedIn: 'root' })
export class StoreContext {
  private readonly auth = inject(AuthService);
  private readonly chosen = signal<string | null>(null);

  readonly canChoose = computed(() => this.auth.hasAnyRole(['proprietario']));

  readonly selected = computed(
    () => this.chosen() ?? (this.canChoose() ? ALL_STORES : (this.auth.stores()[0]?.id ?? ALL_STORES)),
  );

  readonly visible = computed<readonly Store[]>(() =>
    this.selected() === ALL_STORES
      ? this.auth.stores()
      : this.auth.stores().filter((store) => store.id === this.selected()),
  );

  readonly label = computed(() =>
    this.selected() === ALL_STORES ? 'Todas as lojas' : (this.visible()[0]?.name ?? ''),
  );

  select(id: string): void {
    this.chosen.set(id);
  }
}
