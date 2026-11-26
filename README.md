# Fluxo de Caixa

Sistema de fluxo de caixa para uma empresa com três lojas, dividido em dois serviços independentes: *Lançamentos* (abre e fecha o caixa e registra débitos e créditos) e *Consolidado* (mantém o saldo diário por loja e responde às consultas).

A arquitetura, os requisitos e as decisões estão em [docs/arquitetura.md](docs/arquitetura.md) e nos ADRs em [docs/adr/](docs/adr/).

## Tecnologias

| Componente | Tecnologia | Por quê |
| --- | --- | --- |
| APIs | .NET 11, ASP.NET Core | Um serviço para cada domínio, com módulos próprios sobre um `Cashflow.Core` compartilhado |
| Escrita | EF Core 10 | Gravação com transação nos dois serviços (ADR 004) |
| Leitura | Dapper | SQL direto e visível nas consultas (ADR 004) |
| Banco | PostgreSQL 17 | Um servidor de banco por serviço (ADR 001) e RLS por loja (ADR 005) |
| Mensageria | RabbitMQ 4 | Os serviços só se falam por eventos, com outbox no *Lançamentos* (ADR 003) |
| Identidade | Keycloak 26 | OIDC, papéis e lojas por grupo (ADR 005) |
| Front-end | Angular 22, Taiga UI 5, keycloak-js | Telas de caixa, saldo e relatório, servidas por nginx |
| Observabilidade | OpenTelemetry, Aspire Dashboard | Um trace liga o lançamento ao saldo atualizado |
| Documentação da API | OpenAPI e Scalar | Apenas em `Development` |
| Testes | xUnit v3, NetArchTest, Testcontainers, k6 | Unidade, arquitetura, integração e carga |

## Como rodar

Pré-requisitos: Docker com Compose.

```bash
cp .env.example .env
docker compose up -d --build
```

Valores de portas e senhas vêm do `.env.example`.

| Serviço | URL |
| --- | --- |
| Front-end | <http://localhost:24200> |
| API de *Lançamentos* | <http://localhost:25001> (Scalar em `/scalar`) |
| API de *Consolidado* | <http://localhost:25002> (Scalar em `/scalar`) |
| Keycloak | <http://localhost:28080> |
| RabbitMQ (gerenciamento) | <http://localhost:25673> |
| Aspire Dashboard | <http://localhost:28888> |
| PostgreSQL de *Lançamentos* | localhost:25432 |
| PostgreSQL de *Consolidado* | localhost:25433 |
| RabbitMQ (AMQP) | localhost:25672 |

Para parar: `docker compose down`. Para zerar também os bancos, a fila e o realm do Keycloak: `docker compose down -v`.

## Usuários de teste

Criados pelo realm `cashflow` em [docker/keycloak/cashflow-realm.json](docker/keycloak/cashflow-realm.json). A senha de todos é `cashflow`. Servem somente para o ambiente local.

| Usuário | Papel | Loja |
| --- | --- | --- |
| `caixa.loja01` | caixa | LOJA01 |
| `caixa.loja02` | caixa | LOJA02 |
| `auxiliar.loja01` | auxiliar | LOJA01 |
| `proprietario` | proprietario | LOJA01, LOJA02 e LOJA03 |

## Como testar o fluxo na mão

1. Obter o token do caixa da loja 01 (o client `cashflow-web` aceita o grant por senha no realm local):

   ```bash
   TOKEN=$(curl -s -X POST http://localhost:28080/realms/cashflow/protocol/openid-connect/token \
     -d grant_type=password -d client_id=cashflow-web \
     -d username=caixa.loja01 -d password=cashflow \
     | sed 's/.*"access_token":"\([^"]*\)".*/\1/')
   ```

2. Abrir o caixa e lançar um crédito e um débito (`Credit` ou `Debit`, descrição de 5 a 200 caracteres):

   ```bash
   curl -X POST http://localhost:25001/api/v1/stores/LOJA01/cash-register/open -H "Authorization: Bearer $TOKEN"

   curl -X POST http://localhost:25001/api/v1/stores/LOJA01/entries \
     -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
     -d '{"type":"Credit","amount":100,"description":"Venda no balcão"}'

   curl -X POST http://localhost:25001/api/v1/stores/LOJA01/entries \
     -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
     -d '{"type":"Debit","amount":30,"description":"Compra de material"}'
   ```

3. Consultar o saldo com um usuário que lê o *Consolidado* (`auxiliar.loja01` ou `proprietario`; o caixa recebe 403). Repita o passo 1 trocando o usuário e use o novo token:

   ```bash
   DAY=$(date +%F)
   curl "http://localhost:25002/api/v1/daily-balances?startDate=$DAY&endDate=$DAY" -H "Authorization: Bearer $TOKEN"
   curl "http://localhost:25002/api/v1/company/daily-balances?startDate=$DAY&endDate=$DAY" -H "Authorization: Bearer $TOKEN"
   ```

4. Abrir o Aspire Dashboard (<http://localhost:28888>), ir em Rastreamentos (traces) e abrir o trace `POST /api/v1/stores/{storeId}/entries`. Ele mostra a gravação no banco, a publicação do evento e o processamento no *Consolidado*.

O front-end faz o mesmo pelo navegador em <http://localhost:24200>.

## Testes

São 42 métodos de teste, que somam 46 casos executados, porque quatro testes de arquitetura rodam uma vez para cada serviço. Cada um com `Trait("Requirement", ...)` e o requisito no nome de exibição.

| Projeto | Teste | Requisito |
| --- | --- | --- |
| Entries.Tests | `OpenCreatesOpenCashRegister` | RF01 |
| Entries.Tests | `OpenTwiceConflicts` | RF01 |
| Entries.Tests | `PostEntryOpenCashRegisterCreatesEntry` | RF02, RF03 |
| Entries.Tests | `PostCreditPersistsEntryAndEvent` | RF02, RNF05 |
| Entries.Tests | `PostEntryClosedCashRegisterFails` | RF04 |
| Entries.Tests | `PostClosedCashRegisterFails` | RF04 |
| Entries.Tests | `CloseOpenCashRegisterClosesDay` | RF04 |
| Entries.Tests | `PostOtherStoreIsForbidden` | RF10 |
| Entries.Tests | `StoresAreTheLastSegmentOfTheGroup` | RF10 |
| Entries.Tests | `RolesComeFromRolesClaim` | RF09 |
| Balance.Tests | `BalanceIsCreditsMinusDebits` | RF05, RF06 |
| Balance.Tests | `NextEventsAccumulate` | RF05 |
| Balance.Tests | `DifferentStoresAndDaysAreSeparate` | RF05 |
| Balance.Tests | `RepeatedEventDoesNotChangeBalance` | RNF05 |
| Balance.Tests | `SameDataWithDifferentEventIdCountsAgain` | RNF05 |
| Architecture.Tests | `DomainHasNoInfrastructureDependency` | Arquitetura |
| Architecture.Tests | `ServicesDoNotReferenceEachOther` | RNF01 |
| Architecture.Tests | `CoreDoesNotDependOnServices` | RNF01 |
| Architecture.Tests | `QueryHandlersDoNotUseDbContextOrRepositories` | RNF02 |
| Architecture.Tests | `ConcreteTypesAreInternalSealed` | Arquitetura |
| Integration.Tests | `EntriesRequireAuthentication` | RF09 |
| Integration.Tests | `BalanceRequiresAuthenticationAndRole` | RF09, RF11 |
| Integration.Tests | `OnlyTheCashierOperatesTheCashRegister` | RF11 |
| Integration.Tests | `CashierCannotOperateAnotherStore` | RF10 |
| Integration.Tests | `ReadersSeeOnlyTheirStores` | RF10 |
| Integration.Tests | `AppRoleSeesOnlyTheStoresInTheSession` (Entries) | RF10 |
| Integration.Tests | `CannotInsertEntryOfAnotherStore` | RF10 |
| Integration.Tests | `EntriesAreImmutableForTheAppRole` | RF02 |
| Integration.Tests | `ReaderSeesOnlyTheStoresInTheSession` (Balance) | RF10 |
| Integration.Tests | `WithoutStoreContextNothingIsVisible` (Balance) | RF10 |
| Integration.Tests | `BalanceIsUpdatedWithinFiveSeconds` | RNF03 |
| Integration.Tests | `CreditsAndDebitsProduceTheDailyBalance` | RF05, RF07 |
| Integration.Tests | `EntriesOfTheDayCanBeListed` | RF08 |
| Integration.Tests | `EntriesKeepWorkingWhileBalanceIsDown` | RNF01, RNF03 |
| Integration.Tests | `PendingOutboxMessagesArePublished` | RNF05 |
| Integration.Tests | `RepeatedEventDoesNotChangeTheBalance` | RNF05 |
| Integration.Tests | `InvalidMessagesGoToTheDeadLetterQueue` | RNF05 |
| Integration.Tests | `RepeatedFailureGoesToTheDeadLetterQueue` | RNF05 |
| Integration.Tests | `TemporaryFailureDoesNotCountTowardsTheLimit` | RNF05 |
| Integration.Tests | `EntryAndOutboxMessageAreStoredTogether` | RNF05 |
| Integration.Tests | `OutboxFailureLeavesNoEntry` | RNF05 |
| Integration.Tests | `ConcurrentEntriesDoNotExhaustDatabaseConnections` | RNF01 |

Os testes de arquitetura marcados como "Arquitetura" não correspondem a um RF ou RNF do documento. O RNF04 (disponibilidade) é de monitoramento e não tem teste. O workflow [.github/workflows/ci.yml](.github/workflows/ci.yml) roda build, unidade, arquitetura e integração do .NET, além do build e dos testes do frontend. Ele roda em pull requests, exceto nos que alteram somente documentação (`docs/` e arquivos `.md`), `tests/load/` ou `.gitignore`, e também pode ser disparado manualmente. Uma mudança só no frontend também executa os jobs .NET.

Comandos:

```bash
dotnet test --project tests/Cashflow.Entries.Tests
dotnet test --project tests/Cashflow.Balance.Tests
dotnet test --project tests/Cashflow.Architecture.Tests
dotnet test --project tests/Cashflow.Integration.Tests
```

Os de integração sobem PostgreSQL e RabbitMQ com Testcontainers e exigem Docker. Sem Docker, defina `CASHFLOW_TESTS_SKIP_DOCKER=1` e eles são pulados com mensagem. O RabbitMQ dos testes usa o próprio `docker/rabbitmq/definitions.json`.

### Carga (RNF02)

`tests/load/run.sh` roda o k6 em Docker na rede do compose (a stack precisa estar de pé). Ele popula o banco do *Consolidado* com saldos de 3 lojas por 401 dias, pede o token do `proprietario` ao Keycloak, com as renovações espalhadas no tempo, e faz `GET /api/v1/daily-balances` de 30 dias a 50 req/s constantes por 5 minutos. O teste só passa se houver menos de 5% de falhas, se 99% das respostas levarem menos de 500 ms e se nenhuma requisição for perdida por falta de capacidade. O resumo é salvo em `tests/load/results/`, junto com as medições com horário (`.csv`, fora do Git).

Resultado da última rodada, medido na máquina de desenvolvimento com serviços e bancos em Docker no mesmo host (`tests/load/results/balance-20261126T184019Z.txt` e `.json`). Não representa produção.

| Métrica (endpoint de saldo) | Valor |
| --- | --- |
| Requisições a 50 req/s por 5 minutos | 15.001 |
| Falhas | 0 de 15.111 requisições HTTP (0,00%), incluindo token e verificação inicial |
| Requisições perdidas por falta de capacidade | 0 |
| Média | 2,00 ms |
| Mediana | 1,88 ms |
| 95% das respostas levaram até | 2,67 ms |
| 99% das respostas levaram até | 3,79 ms |
| Máximo | 186,46 ms |

Os três critérios foram atendidos, então o RNF02 foi cumprido nesta rodada. Antes dela, no mesmo dia, duas rodadas foram reprovadas por requisições perdidas: 49, com um build na mesma máquina, e 21 (`balance-20261126T173944Z` e `balance-20261126T180529Z`). Os usuários virtuais renovavam o token quase ao mesmo tempo, e o roteiro passou a espalhar as renovações. Uma rodada sem essa mudança, com medições detalhadas e sem resumo salvo no repositório, também passou, por isso a causa das perdas não está confirmada. O resultado de 19/11, anterior à separação dos bancos, foi mantido como referência (`balance-20261119T213138Z`).

## Decisões e limites

- **Outbox e RabbitMQ:** o lançamento e o evento são gravados na mesma transação. Um serviço em segundo plano publica os eventos pendentes com confirmação do broker. Se o RabbitMQ cair, os eventos esperam no outbox (ADR 003).
- **Consistência eventual:** o saldo é atualizado de forma assíncrona. O consumidor ignora eventos já processados e envia para a fila `balance.entry-posted.dlq` as mensagens inválidas e as que falham 5 vezes. Falhas temporárias do banco não contam para esse limite.
- **Isolamento por loja:** RLS no PostgreSQL com `FORCE ROW LEVEL SECURITY`. O usuário da aplicação não é dono das tabelas, e sem a loja informada nenhuma linha é devolvida. Lançamentos são imutáveis também no banco: o papel da aplicação não tem `UPDATE` nem `DELETE` em `entries`.
- **Papéis:** só o caixa abre, fecha e lança. Caixa, auxiliar e proprietário consultam os lançamentos. Auxiliar e proprietário consultam o saldo.
- **Mensageria com permissão mínima:** o usuário do *Lançamentos* só publica no exchange e o do *Consolidado* só consome a fila, definidos em `docker/rabbitmq/definitions.json`.
- **Dia comercial:** o fuso padrão é `America/Sao_Paulo`, configurável em `Business:TimeZone`.
- **Demonstração:** o `.env.example` define `ASPNETCORE_ENVIRONMENT=Development` para expor o Scalar e o OpenAPI. O Keycloak roda em `start-dev`, o Aspire Dashboard sem autenticação e tudo em HTTP. Nada disso serve para produção.
- **CORS:** as origens permitidas vêm de `CORS_ORIGINS`, separadas por vírgula.
- **Frontend:** a tela de caixa usa `GET /cash-register/today`. O relatório lista os lançamentos do último dia do período selecionado, com `GET /entries`. Com todas as lojas selecionadas, o total da empresa no relatório e na tela de saldo vem de `GET /company/daily-balances`. Os testes do frontend usam Vitest, e o do relatório cobre RF07 e RF08.
