import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TuiButton } from '@taiga-ui/core';
import { TuiBlockStatus } from '@taiga-ui/layout';

@Component({
  selector: 'app-acesso-negado',
  imports: [RouterLink, TuiButton, TuiBlockStatus],
  template: `
    <tui-block-status>
      <h2>Acesso negado</h2>
      <p>Seu perfil não tem permissão para acessar esta tela.</p>
      <button tuiButton size="m" routerLink="/">Voltar ao início</button>
    </tui-block-status>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AcessoNegado {}
