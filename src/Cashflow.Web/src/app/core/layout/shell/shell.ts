import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TuiItem } from '@taiga-ui/cdk';
import { TuiButton, TuiDataList, TuiDropdown, TuiIcon } from '@taiga-ui/core';
import { TuiAvatar, TuiBreadcrumbs, TuiChevron } from '@taiga-ui/kit';
import { TuiNavigation } from '@taiga-ui/layout';
import { filter, map, startWith } from 'rxjs';
import { AuthService } from '../../auth/auth.service';
import { ROLE_LABEL } from '../../auth/roles';
import { ALL_STORES, StoreContext } from '../../store/store-context';
import { MENU } from '../menu';

@Component({
  selector: 'app-shell',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    TuiNavigation,
    TuiButton,
    TuiIcon,
    TuiAvatar,
    TuiBreadcrumbs,
    TuiItem,
    TuiChevron,
    TuiDropdown,
    TuiDataList,
  ],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Shell {
  private readonly router = inject(Router);
  protected readonly auth = inject(AuthService);
  protected readonly storeContext = inject(StoreContext);
  protected readonly allStores = ALL_STORES;

  protected readonly theme = 'light';
  protected readonly expanded = signal(true);
  protected readonly storeMenuOpen = signal(false);

  protected readonly roleLabel = computed(() =>
    this.auth
      .roles()
      .map((role) => ROLE_LABEL[role])
      .join(', '),
  );

  protected readonly initials = computed(() =>
    this.auth
      .name()
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0].toUpperCase())
      .join(''),
  );

  protected readonly menu = computed(() =>
    MENU.map((group) => ({
      ...group,
      items: group.items.filter((item) => this.auth.hasAnyRole(item.roles)),
    })).filter((group) => group.items.length > 0),
  );

  protected readonly crumbs = toSignal(
    this.router.events.pipe(
      filter((event) => event instanceof NavigationEnd),
      startWith(null),
      map(() => {
        let snapshot = this.router.routerState.snapshot.root;
        while (snapshot.firstChild) {
          snapshot = snapshot.firstChild;
        }
        const { section, title } = snapshot.data as { section?: string; title?: string };
        return [section, title].filter((value): value is string => !!value);
      }),
    ),
    { initialValue: [] as string[] },
  );

  protected selectStore(id: string): void {
    this.storeContext.select(id);
    this.storeMenuOpen.set(false);
  }

  protected logout(): void {
    void this.auth.logout();
  }
}
