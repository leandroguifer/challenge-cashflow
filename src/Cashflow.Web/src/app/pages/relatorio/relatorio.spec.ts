import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideTaiga } from '@taiga-ui/core';
import { BalanceApiService } from '../../core/api/balance-api.service';
import { EntriesApiService } from '../../core/api/entries-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { ALL_STORES, StoreContext } from '../../core/store/store-context';
import { Relatorio } from './relatorio';

describe('Relatorio', () => {
  it('RF07 e RF08: mostra os lançamentos de cada loja e o total da empresa vindo da API', async () => {
    window.matchMedia = vi.fn().mockReturnValue({
      matches: false,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    });
    const stores = [
      { id: 'LOJA01', name: 'Loja 01' },
      { id: 'LOJA02', name: 'Loja 02' },
    ];
    const total = {
      businessDate: '2026-01-10',
      credits: 500,
      debits: 100,
      balance: 400,
      entryCount: 2,
    };
    const balanceApi = {
      list: vi.fn().mockResolvedValue([]),
      listCompany: vi.fn().mockResolvedValue([total]),
    };
    const entriesApi = {
      list: vi.fn((storeId: string) =>
        Promise.resolve([
          {
            id: storeId,
            time: '10:00',
            type: 'entrada',
            description: `Venda ${storeId}`,
            amount: 250,
          },
        ]),
      ),
    };
    TestBed.configureTestingModule({
      imports: [Relatorio],
      providers: [
        provideTaiga(),
        { provide: BalanceApiService, useValue: balanceApi },
        { provide: EntriesApiService, useValue: entriesApi },
        { provide: AuthService, useValue: { stores: signal(stores) } },
        {
          provide: StoreContext,
          useValue: {
            selected: signal(ALL_STORES),
            visible: signal(stores),
            label: signal('Todas as lojas'),
          },
        },
      ],
    });

    const fixture = TestBed.createComponent(Relatorio);
    await fixture.whenStable();
    fixture.detectChanges();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(entriesApi.list).toHaveBeenCalledTimes(2);
    expect(balanceApi.listCompany).toHaveBeenCalledOnce();
    expect(text).toContain('Venda LOJA01');
    expect(text).toContain('Venda LOJA02');
    expect(text).toContain('400');
  });
});
