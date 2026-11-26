# Evidências

* **Autores:** Leandro Fernandes

Registros de execução do sistema e do CI. As telas e os comandos são do ambiente local, com dados de teste. A relação entre requisitos, código, testes e evidências está em [rastreabilidade.md](rastreabilidade.md).

## Integração contínua

| ID | O que comprova | Requisito | Imagem | PR |
| --- | --- | --- | --- | --- |
| E01a | Uma PR que altera somente documentação não executa o CI | — | [ver](evidencias/E01a-ci-filtro-pr-sem-checks.png) | [#3](https://github.com/leandroguifer/challenge-cashflow/pull/3) |
| E01b | Essa PR não criou uma nova execução no Actions | — | [ver](evidencias/E01b-ci-filtro-actions-sem-run-novo.png) | [#3](https://github.com/leandroguifer/challenge-cashflow/pull/3) |
| E02a | Três jobs aprovados, com o frontend no CI | — | [ver](evidencias/E02a-ci-frontend-tres-jobs.png) | [#4](https://github.com/leandroguifer/challenge-cashflow/pull/4) |

## Saldo e relatório

| ID | O que comprova | Requisito | Imagem | PR |
| --- | --- | --- | --- | --- |
| E04f | Saldo do dia de cada loja e total da empresa | RF06, RF07 | [ver](evidencias/E04f-relatorio-saldo-total-empresa.png) | [#5](https://github.com/leandroguifer/challenge-cashflow/pull/5) |
| E04l | Relatório com duas lojas, os lançamentos do dia e o total da empresa | RF07, RF08 | [ver](evidencias/E04l-relatorio-duas-lojas-total-empresa.png) | [#5](https://github.com/leandroguifer/challenge-cashflow/pull/5) |
| E04n | Testes do frontend aprovados no CI, incluindo o do relatório | RF07, RF08 | [ver](evidencias/E04n-relatorio-ci-testes.png) | [#5](https://github.com/leandroguifer/challenge-cashflow/pull/5) |

## Isolamento de falha

Cenário: o servidor de banco do *Consolidado* é parado, um lançamento é registrado e o banco é religado.

| ID | O que comprova | Requisito | Imagem | PR |
| --- | --- | --- | --- | --- |
| E05b | O banco do *Consolidado* está parado, e o *Lançamentos* e o banco dele seguem no ar | RNF01 | [ver](evidencias/E05b-banco-separado-banco-consolidado-parado.png) | [#7](https://github.com/leandroguifer/challenge-cashflow/pull/7) |
| E05c | O lançamento é aceito durante a parada | RF02, RNF01 | [ver](evidencias/E05c-banco-separado-lancamento-com-banco-parado.png) | [#7](https://github.com/leandroguifer/challenge-cashflow/pull/7) |
| E05d | Depois do retorno, o saldo inclui o lançamento feito durante a parada | RF05, RF06 | [ver](evidencias/E05d-banco-separado-saldo-atualizado.png) | [#7](https://github.com/leandroguifer/challenge-cashflow/pull/7) |
| E05f | O trace mostra o lançamento gravado, as tentativas com falha e o processamento após o retorno do banco | RNF01, RNF05 | [ver](evidencias/E05f-banco-separado-trace-tentativas-ate-banco-voltar.png) | [#7](https://github.com/leandroguifer/challenge-cashflow/pull/7) |
| E05g | Jobs do CI aprovados na separação dos bancos | — | [ver](evidencias/E05g-banco-separado-ci-tres-jobs.png) | [#7](https://github.com/leandroguifer/challenge-cashflow/pull/7) |

## Observabilidade

| ID | O que comprova | Requisito | Imagem | PR |
| --- | --- | --- | --- | --- |
| E06a | Um só trace vai do lançamento ao saldo, passando pelo RabbitMQ. A duração de 1,17 s é uma medição isolada | RNF03 | [ver](evidencias/E06a-observabilidade-trace-lancamento-ate-saldo.png) | — |

## Limite de tentativas

Cenário: o mesmo de isolamento de falha, com o limite de tentativas no consumidor.

| ID | O que comprova | Requisito | Imagem | PR |
| --- | --- | --- | --- | --- |
| E07a | Com o banco parado, a mensagem aguarda na fila e a fila de erros está vazia | RNF05 | [ver](evidencias/E07a-limite-tentativas-banco-parado-fila-erros-vazia.png) | [#8](https://github.com/leandroguifer/challenge-cashflow/pull/8) |
| E07c | Saída registrada durante a parada do banco | RF03, RNF01 | [ver](evidencias/E07c-limite-tentativas-lancamento-com-banco-parado.png) | [#8](https://github.com/leandroguifer/challenge-cashflow/pull/8) |
| E07b | Depois do retorno, o relatório inclui a saída | RF05, RNF05 | [ver](evidencias/E07b-limite-tentativas-saldo-apos-banco-voltar.png) | [#8](https://github.com/leandroguifer/challenge-cashflow/pull/8) |
| E07d | Três jobs do CI aprovados | — | [ver](evidencias/E07d-limite-tentativas-ci-tres-jobs.png) | [#8](https://github.com/leandroguifer/challenge-cashflow/pull/8) |

## Limite de conexões

| ID | O que comprova | Requisito | Imagem | PR |
| --- | --- | --- | --- | --- |
| E08a | Três jobs do CI aprovados na PR que inclui o teste de 100 lançamentos simultâneos | RNF01 | [ver](evidencias/E08a-pool-conexoes-ci-tres-jobs.png) | [#9](https://github.com/leandroguifer/challenge-cashflow/pull/9) |

## Teste de carga

| ID | O que comprova | Requisito | Imagem | PR |
| --- | --- | --- | --- | --- |
| E09a | Rodada reprovada: 49 requisições perdidas | RNF02 | [ver](evidencias/E09a-carga-saldo-resultado.png) | [#10](https://github.com/leandroguifer/challenge-cashflow/pull/10) |
| E09e | Rodada aprovada nos três critérios, depois da mudança no roteiro | RNF02 | [ver](evidencias/E09e-carga-saldo-apos-correcao.png) | [#10](https://github.com/leandroguifer/challenge-cashflow/pull/10) |
| E09f | Três jobs do CI aprovados. O CI não executa o teste de carga | — | [ver](evidencias/E09f-carga-saldo-ci-tres-jobs.png) | [#10](https://github.com/leandroguifer/challenge-cashflow/pull/10) |

Resultados completos, em [tests/load/results/](../tests/load/results/):

| Resultado | Situação |
| --- | --- |
| [balance-20261119T213138Z](../tests/load/results/balance-20261119T213138Z.txt) | Referência anterior à separação dos bancos |
| [balance-20261126T173944Z](../tests/load/results/balance-20261126T173944Z.txt) | Reprovado: 49 requisições perdidas |
| [balance-20261126T180529Z](../tests/load/results/balance-20261126T180529Z.txt) | Reprovado: 21 requisições perdidas |
| [balance-20261126T184019Z](../tests/load/results/balance-20261126T184019Z.txt) | Aprovado |

A análise das reprovações está no [README](../README.md#carga-rnf02).

## Observações

* Nas imagens E07d, E08a e E09f, o ícone no topo da página aparece em andamento, e os três jobs aparecem aprovados. O resultado pode ser conferido no link da PR.
* Em E07c, a descrição digitada no lançamento cita o *balance* parado. O componente parado nesse cenário é o servidor de banco do *Consolidado* (E07a).
* As imagens E05b, E07a, E09a e E09e foram recortadas para mostrar somente o terminal.
