# Arquitetura do Sistema


* **Autores:** Leandro Fernandes

Este documento é o guia de referência da arquitetura do sistema de **Fluxo de Caixa**, tomando como exemplo o caso da **Fernandes Mecânica**. Nele detalhamos o contexto de negócio e os demais pontos que envolvem o projeto.

## Índice

- [Arquitetura do Sistema](#arquitetura-do-sistema)
  - [Índice](#índice)
  - [1. Visão geral](#1-visão-geral)
    - [1.1 Escopo](#11-escopo)
  - [2. Contexto](#2-contexto)
    - [2.1 Atores](#21-atores)
    - [2.2 Domínios](#22-domínios)
    - [2.3 Considerações](#23-considerações)
  - [3. Requisitos](#3-requisitos)
    - [3.1 Requisitos funcionais](#31-requisitos-funcionais)
    - [3.2 Requisitos não funcionais](#32-requisitos-não-funcionais)
  - [4. Arquitetura alvo](#4-arquitetura-alvo)
    - [4.1 Contexto](#41-contexto)
    - [4.2 Contêineres](#42-contêineres)
    - [4.3 Fluxo de um lançamento](#43-fluxo-de-um-lançamento)
    - [4.4 Como a arquitetura atende os requisitos](#44-como-a-arquitetura-atende-os-requisitos)
  - [5. Decisões](#5-decisões)
  - [6. Segurança](#6-segurança)
  - [7. Qualidade e observabilidade](#7-qualidade-e-observabilidade)
    - [7.1 Atributos de qualidade](#71-atributos-de-qualidade)
      - [Escalabilidade](#escalabilidade)
      - [Alta disponibilidade](#alta-disponibilidade)
      - [Segurança](#segurança)
      - [Observabilidade](#observabilidade)
  - [8. Riscos e impactos](#8-riscos-e-impactos)
  - [9. Custos](#9-custos)
  - [10. Transição](#10-transição)
  - [11. Evoluções futuras](#11-evoluções-futuras)

<a id="1"></a>
## 1. Visão geral

O comerciante precisa controlar seu fluxo de caixa diário, com lançamentos de débito e crédito, e consultar o saldo diário *Consolidado*.

O **Sistema de Fluxo de Caixa** está sendo pensado para atender a **Fernandes Mecânica**, com o intuito de substituir o processo que hoje é feito em planilhas, de maneira descentralizada, por um sistema único que fará a unificação das informações das lojas.

A solução é dividida em dois serviços independentes. O serviço de *Lançamentos* registra cada débito e crédito e responde ao caixa na hora. A cada lançamento, ele avisa o outro serviço por meio de um evento. O serviço de *Consolidado* recebe esses eventos e mantém o saldo diário de cada loja já calculado, pronto para consulta. Se o *Consolidado* ficar fora do ar, os lançamentos continuam sendo aceitos e os eventos esperam na fila, e o saldo é atualizado quando o serviço voltar.

<a id="1_1"></a>
### 1.1 Escopo

* O que o sistema faz
   * Lançamento de entradas (créditos) e saídas (débitos) por loja
   * Emissão de relatório do saldo diário e dos lançamentos, *Consolidado* por loja e empresa
* O que o sistema **não faz**
   * Controle de estoque
   * Controle de ordens de serviço
   * Orçamentos e vendas de produtos
   * Emissão de nota fiscal

<a id="2"></a>
## 2. Contexto

<a id="2_1"></a>
### 2.1 Atores

| Ator | Função |
| --- | --- |
| **Proprietário** | Vê e confere o caixa de todas as lojas todos os dias, através do relatório *Consolidado* |
| **Auxiliar administrativo** | Vê e confere o caixa da loja em que trabalha, através do relatório *Consolidado* |
| **Caixa** | Opera o caixa da loja em que trabalha, registra as entradas e saídas e fecha o caixa no final do dia |

<a id="2_2"></a>
### 2.2 Domínios

| Domínio | Ações |
| --- | --- |
| *Lançamentos* | Abrir caixa, Registrar entrada, Registrar saída, Fechar caixa |
| *Consolidado* | Consolidar o saldo diário por loja, Consultar o saldo de uma loja, Consultar o saldo da empresa e Disponibilizar o relatório |
| Transversal | Controlar o acesso por papel e por loja |

<a id="2_3"></a>
### 2.3 Considerações

* O saldo diário é por loja: o proprietário vê o de cada loja e o total das três.
* Os dados pertencem à empresa, e cada loja é uma divisão dela. O acesso é controlado por loja: o auxiliar administrativo e o caixa veem somente a própria loja.
* Cada loja tem um caixa.
* Ao fechar o caixa, o dia é encerrado e o proprietário pode ver o fechamento através do *Consolidado*.

<a id="3"></a>
## 3. Requisitos

<a id="3_1"></a>
### 3.1 Requisitos funcionais

| Código | Domínio | Descrição |
| --- | --- | --- |
| RF01 | *Lançamentos* | Abrir o caixa da loja no início do dia. Cada loja tem um caixa aberto por dia. |
| RF02 | *Lançamentos* | Registrar uma *entrada* com valor, data e descrição, somente com o caixa aberto. *Lançamentos* são imutáveis. |
| RF03 | *Lançamentos* | Registrar uma *saída* com valor, data e descrição, somente com o caixa aberto. *Lançamentos* são imutáveis. |
| RF04 | *Lançamentos* | Fechar o caixa. O dia da loja é encerrado e não aceita mais lançamentos. |
| RF05 | *Consolidado* | Atualizar o saldo diário da loja a cada lançamento registrado. Saldo do dia = créditos − débitos daquele dia, por loja. |
| RF06 | *Consolidado* | Consultar o saldo diário de uma loja. |
| RF07 | *Consolidado* | Consultar o saldo diário da empresa, somando as lojas. Saldo da empresa = soma dos saldos do dia de cada loja. |
| RF08 | *Consolidado* | Disponibilizar o relatório do dia, com lançamentos e saldo, por loja e para a empresa. |
| RF09 | Transversal | Autenticar o usuário e identificar seu papel e sua loja. |
| RF10 | Transversal | Restringir o acesso por loja: caixa e auxiliar veem só a própria, o proprietário vê todas. |
| RF11 | Transversal | Restringir as ações por papel: só o caixa lança, abre e fecha; o proprietário e o auxiliar só consultam. |

<a id="3_2"></a>
### 3.2 Requisitos não funcionais

| Código | Requisito | Meta | Como verificar |
| --- | --- | --- | --- |
| RNF01 | Isolamento de falha | Com o *Consolidado* parado, o *Lançamentos* aceita 100% dos lançamentos válidos. | Derrubar o *Consolidado*, enviar lançamentos e conferir que nenhum falhou. |
| RNF02 | Desempenho do *Consolidado* | 50 req/s durante 5 minutos, com no máximo 5% de falha e 99% das respostas em até 500 ms. | Teste de carga. |
| RNF03 | Atraso do saldo | Até 5 segundos depois do lançamento. Depois de uma parada, até 5 minutos após o retorno. | Medir o tempo entre o lançamento e o saldo atualizado. |
| RNF04 | Disponibilidade | *Lançamentos* 99,5%. *Consolidado* 99%. | Monitoramento de disponibilidade. |
| RNF05 | Durabilidade e sem duplicidade | Nenhum lançamento aceito se perde. Um evento repetido não altera o saldo duas vezes. | Teste de integração. |

<a id="4"></a>
## 4. Arquitetura alvo

A arquitetura é descrita no modelo C4, em dois níveis: o contexto (quem usa o sistema e com o que ele conversa) e os contêineres (as peças que rodam). Nos diagramas, o azul é o que construímos e o cinza é o que é externo.

<a id="4_1"></a>
### 4.1 Contexto

```mermaid
C4Context
title Nível 1 - Contexto: sistema de fluxo de caixa
Person(proprietario, "Proprietário", "Vê todas as lojas e confere o caixa todos os dias")
Person(caixa, "Caixa", "Abre o caixa, registra entradas e saídas e fecha o caixa da loja")
Person(auxiliar, "Auxiliar administrativo", "Consulta o saldo e o relatório da própria loja")
System(fluxo, "Sistema de fluxo de caixa", "Controla lançamentos e consolida o saldo diário por loja")
System_Ext(keycloak, "Keycloak", "Autenticação e papéis (OIDC)")
Rel(proprietario, fluxo, "Consulta")
Rel(caixa, fluxo, "Lança e fecha")
Rel(auxiliar, fluxo, "Consulta")
Rel(fluxo, keycloak, "Autentica", "OIDC")
UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="1")
```

Os três atores usam o sistema conforme o seu papel (seção 2.1). A autenticação e os papéis ficam fora dele, em um provedor de identidade (Keycloak), o que mantém o sistema focado no negócio.

<a id="4_2"></a>
### 4.2 Contêineres

```mermaid
C4Container
title Nível 2 - Contêineres: sistema de fluxo de caixa
System_Ext(keycloak, "Keycloak", "Autenticação e papéis (OIDC)")
Person(usuario, "Usuário", "Proprietário, Caixa ou Auxiliar administrativo")
System_Boundary(sistema, "Sistema de fluxo de caixa") {
  Container(apiLanc, "API de *Lançamentos*", "C#, EF Core, Dapper", "Registra débitos e créditos, abre e fecha o caixa")
  Container(spa, "Front-end", "Angular, Taiga UI", "Tela de lançar e ver o saldo")
  Container(apiCons, "API de *Consolidado*", "C#, EF Core, Dapper", "Consome eventos e responde o saldo diário")
  ContainerDb(dbLanc, "Banco de *Lançamentos*", "PostgreSQL, servidor próprio", "Lançamentos e outbox")
  ContainerQueue(fila, "Fila de eventos", "RabbitMQ, servidor próprio", "Transporta os eventos de lançamento")
  ContainerDb(dbCons, "Banco de *Consolidado*", "PostgreSQL, servidor próprio", "Saldo diário e eventos já processados")
}
Rel(usuario, spa, "Usa")
Rel(spa, keycloak, "Login", "OIDC")
Rel(spa, apiLanc, "Lança", "HTTPS")
Rel(spa, apiCons, "Consulta", "HTTPS")
Rel(apiLanc, dbLanc, "Grava", "SQL")
Rel(apiLanc, fila, "Publica", "AMQP")
Rel(fila, apiCons, "Entrega", "AMQP")
Rel(apiCons, dbCons, "Atualiza e lê", "SQL")
UpdateLayoutConfig($c4ShapeInRow="3", $c4BoundaryInRow="1")
```

| Contêiner | Responsabilidade |
| --- | --- |
| Front-end | Tela de lançar e de ver o saldo. Obtém o token no Keycloak e chama as duas APIs. |
| API de *Lançamentos* | Abre e fecha o caixa e registra entradas e saídas. Grava o lançamento e o evento no mesmo banco, na mesma transação (outbox). |
| Banco de *Lançamentos* | Guarda os lançamentos e os eventos ainda não publicados. |
| Fila de eventos | Transporta os eventos de lançamento. Quem publica não espera quem consome. |
| API de *Consolidado* | Consome os eventos, ignora os repetidos, atualiza o saldo diário e responde às consultas e ao relatório. |
| Banco de *Consolidado* | Guarda o saldo diário por loja e os eventos já processados. |

Cada serviço tem o seu próprio banco, em servidor próprio, o RabbitMQ fica em outro e os dois serviços só se comunicam por eventos. Os dados são da empresa, e o acesso é limitado por papel e por loja (detalhes na seção 6).

<a id="4_3"></a>
### 4.3 Fluxo de um lançamento

1. O caixa registra uma entrada ou saída no front-end, que chama a API de *Lançamentos*.
2. A API grava o lançamento e o evento correspondente no Banco de *Lançamentos*, em uma única transação, e responde ao caixa.
3. Em segundo plano, o evento é publicado na fila.
4. A API de *Consolidado* recebe o evento e verifica se ele já foi processado. Se não foi, atualiza o saldo do dia da loja e registra o evento como processado.
5. O proprietário e o auxiliar consultam o saldo já calculado, sem somar lançamentos na hora.

<a id="4_4"></a>
### 4.4 Como a arquitetura atende os requisitos

| Requisito | Como é atendido |
| --- | --- |
| RNF01 Isolamento de falha | O *Lançamentos* só grava no próprio banco, que fica em servidor próprio. Se o *Consolidado* parar, os eventos esperam na fila. |
| RNF02 Desempenho do *Consolidado* | O saldo já está calculado, e a leitura não soma lançamentos. Os limites para usar várias instâncias estão na seção 7.1. |
| RNF03 Atraso do saldo | A atualização é assíncrona e contínua. Depois de uma parada, o consumidor esvazia a fila acumulada. |
| RNF04 Disponibilidade | Serviços e bancos independentes: a queda de um não derruba o outro. Frontend e Keycloak são compartilhados (seção 7.1). |
| RNF05 Durabilidade e sem duplicidade | O outbox garante que o evento nasce junto com o lançamento. O registro dos eventos já processados impede contar duas vezes. |


<a id="5"></a>
## 5. Decisões

Para a escolha da arquitetura foram tomadas algumas decisões que estão registradas nos ADRs a seguir. Cada uma é baseada nos [requisitos não funcionais](#3_2) da solução e registra o contexto, a escolha, a alternativa descartada e as consequências.

| ADR | Decisão |
| --- | --- |
| [001](adr/001-dois-servicos.md) | Separar *Lançamentos* e *Consolidado* em dois serviços |
| [002](adr/002-saldo-em-tabela-propria.md) | Saldo diário em tabela própria, atualizada por eventos |
| [003](adr/003-rabbitmq-outbox.md) | RabbitMQ com outbox e consumidor sem duplicidade |
| [004](adr/004-cqrs-leve.md) | CQRS leve, com EF Core na escrita e Dapper na leitura |
| [005](adr/005-acesso-por-empresa-e-loja.md) | Acesso por empresa e loja com Keycloak e RLS no PostgreSQL |
| [006](adr/006-observabilidade-opentelemetry.md) | Observabilidade com OpenTelemetry |

<a id="6"></a>
## 6. Segurança

* **Autenticação e autorização:** toda chamada exige token do Keycloak (OIDC). A API confere assinatura, emissor, audiência e validade. O papel define a ação (RF11) e o grupo define a loja (RF10).
* **Isolamento dos dados:** RLS por loja no banco. Sem a loja informada, nenhuma linha é devolvida (ADR 005).
* **Transporte e segredos:** O ambiente local usa HTTP; em produção as APIs devem usar HTTPS. Segredos fora do repositório, com apenas um `.env.example` de valores fictícios.
* **Mensageria:** cada serviço tem seu usuário no RabbitMQ, com permissão mínima: o *Lançamentos* só publica e o *Consolidado* só consome.


<a id="7"></a>
## 7. Qualidade e observabilidade

* **Testes:** unitários (regra do saldo diário e evento repetido), de integração com PostgreSQL e RabbitMQ em contêineres, de arquitetura (xUnit), de isolamento do RLS, de resiliência (derrubar o *Consolidado*, RNF01) e de carga (50 req/s por 5 minutos, RNF02).
* **Execução:** os testes unitários, de arquitetura e de integração (incluindo isolamento e resiliência) rodam no GitHub Actions em pull requests que alteram código. O teste de carga é executado manualmente.
* **Observabilidade:** as APIs são instrumentadas com OpenTelemetry (métricas, traces e logs). Na demonstração, os dados vão para o Aspire Dashboard, que sobe junto no `docker compose`. Em produção, a proposta é Prometheus e Grafana, com alerta quando a fila acumula por mais de 1 minuto. Um trace acompanha o lançamento até o saldo atualizado.


<a id="7_1"></a>
### 7.1 Atributos de qualidade

#### Escalabilidade

* **Hoje:** o saldo é calculado antes da consulta. Em um teste, duas instâncias do *Consolidado* dividiram o processamento de 96 eventos, e o saldo conferiu com os lançamentos.
* **Limites:** não há balanceador, e a distribuição das consultas não foi testada, nem duas instâncias de *Lançamentos* ou a subida simultânea de instâncias com o banco vazio. Cada instância de *Lançamentos* pode usar até 50 conexões, e a do *Consolidado*, até 30. O PostgreSQL aceita 100, com algumas reservadas ao administrador: duas instâncias de *Lançamentos* podem ultrapassar o limite.
* **Produção (proposta):** balanceador na frente das APIs, e limites de conexão revistos conforme a quantidade de instâncias.

#### Alta disponibilidade

* **Hoje:** cada serviço tem o próprio banco. Com o *Consolidado* ou o banco dele parado, o *Lançamentos* continua aceitando lançamentos. As filas são duráveis, o publicador do outbox tenta novamente e o consumidor se reconecta ao RabbitMQ.
* **Limites:** há uma só instância de cada componente. Frontend e Keycloak são compartilhados. A API que sobe sem o banco encerra, e o `docker compose` não a reinicia. O `/health` das APIs não verifica as dependências. Não há cópia de segurança, e as metas de disponibilidade (RNF04) não são medidas.
* **Produção (proposta):** réplicas dos bancos, cluster do RabbitMQ, mais de uma instância das aplicações e do Keycloak, reinício automático, verificação das dependências e cópia de segurança com restauração testada.

#### Segurança

| Tema | Demonstração | Produção (proposta) |
| --- | --- | --- |
| Transporte | HTTP | HTTPS em todas as chamadas |
| Contêineres das APIs e do frontend | Executam como `root` | Usuário sem privilégios |
| Limite de requisições | Não há | Limite nas APIs ou no balanceador |
| Hosts e CORS | `AllowedHosts` aceita qualquer host. O CORS restringe a origem, mas não os métodos e cabeçalhos | Restringir hosts, métodos e cabeçalhos |
| Frontend | nginx sem cabeçalhos de segurança, e informa a versão | Adicionar os cabeçalhos e ocultar a versão |
| Keycloak | Modo de desenvolvimento, sem exigir HTTPS e sem proteção contra força bruta | Modo de produção, com HTTPS e proteção ativada |
| Login | O frontend usa PKCE. O client também aceita login direto com usuário e senha, para os testes manuais | Desativar o login direto |
| Banco | Usuários separados por função, com a mesma senha | Uma senha para cada usuário, em cofre de segredos |
| Aspire Dashboard | Sem autenticação | Acesso autenticado |

#### Observabilidade

* **Hoje:** traces, métricas e logs com OpenTelemetry, no Aspire Dashboard. Um trace acompanha o lançamento pelo RabbitMQ até o saldo.
* **Limites:** não há alertas nem métrica própria do atraso no outbox ou do tamanho da fila.
* **Produção (proposta):** Prometheus e Grafana para métricas e alertas, e um destino para traces e logs, ainda a escolher.


<a id="8"></a>
## 8. Riscos e impactos

| Risco | Mitigação |
| --- | --- |
| RabbitMQ fora do ar | O outbox guarda os eventos e reenvia quando ele volta |
| Evento repetido | O *Consolidado* registra os eventos já processados (RNF05) |
| Mensagem com falha repetida | Depois de 5 tentativas, vai para a fila de erros. Falha temporária do banco não conta. A contagem fica na memória do *Consolidado*, e não há rotina de reprocessamento. Evolução: reprocessar a fila de erros. Em produção, a proposta é ter um alerta para a fila de erros |
| Pico acima de 50 req/s | Saldo já calculado. Evolução: mais instâncias do *Consolidado* atrás de um balanceador, respeitando o limite de conexões do banco |
| Keycloak fora do ar | Não há login nem renovação de token, e o caixa para de lançar quando o token expira (no máximo 5 minutos no ambiente local). A duração curta limita o tempo em que um acesso retirado continua valendo. Em produção, a proposta é ter mais de uma instância do Keycloak e banco próprio com redundância |
| Lançamento errado, sem edição | Fora do escopo. Evolução: lançamento de ajuste |
| Saldo diferente dos lançamentos, sem rotina de reconciliação | Os lançamentos continuam gravados no *Lançamentos* e permitem conferir o saldo. Evolução: reconciliação e reconstrução do saldo |
| Mais de uma empresa no mesmo sistema, sem separação entre elas | Hoje o sistema atende uma só empresa. Evolução: isolamento por empresa no banco |
| Fila ou outbox acumulando sem ninguém perceber | Hoje não há alerta nem métrica própria do outbox. Em produção, a proposta é alertar quando a fila acumula por mais de 1 minuto. Evolução: métrica do atraso no outbox |
| Perda do disco do servidor do RabbitMQ | Eventos publicados e ainda não consumidos se perdem na fila. Eles continuam no outbox, marcados como publicados, mas hoje não há rotina de reenvio. Evolução: cluster com filas replicadas e rotina de reenvio |
| Conexões do banco esgotadas com muitos pedidos simultâneos | Cada conexão tem limite no pool, definido no .env.example. Toda conexão nova, instância a mais ou ambiente novo precisa ter o limite revisto, porque a soma deve ficar abaixo do máximo do PostgreSQL |


<a id="9"></a>
## 9. Custos

- **Premissas:** 3 lojas e cerca de 1.000 lançamentos por dia (estimativa ainda não validada; o teste de carga mede só a consulta do saldo).
- **Licenças:** sem custo (tudo open source).
- **Infraestrutura:** VPS Hostinger, plano de 24 meses.
- **Fora da estimativa:** domínio, cópias de segurança fora dos servidores, servidor de monitoramento, banco do Keycloak e sua redundância, horas de implantação e operação, e margem para imprevistos.

| Cenário | Servidores | Por mês, no plano de 24 meses | Por mês, após a renovação |
| --- | --- | --- | --- |
| Produção inicial | 1 × KVM 4 (aplicações) <br/> 2 × KVM 2 (um para cada banco) <br/> 1 × KVM 1 (RabbitMQ) | R$ 175,96 | R$ 365,96 |
| Alta disponibilidade | 2 × KVM 4 (aplicações) <br/> 4 × KVM 2 (principal e réplica de cada banco) <br/> 3 × KVM 1 (cluster RabbitMQ) <br/> 1 × KVM 1 (balanceador Nginx) | R$ 411,90 | R$ 851,90 |

Valores promocionais, com pagamento do período completo na contratação, à vista ou parcelado. O tamanho dos servidores não foi validado em teste.

Fonte: [https://www.hostinger.com/br/precos/vps-hosting#plan-selector](https://www.hostinger.com/br/precos/vps-hosting#plan-selector), consultada em 26/11/2026.

<a id="10"></a>
## 10. Transição

Hoje o caixa é controlado em planilhas. A migração é gradual:

1. **Preparação:** cadastro das lojas e dos usuários, e treinamento.
2. **Convivência:** uma loja piloto (Rio Verde) usa o sistema e a planilha em paralelo, e o proprietário compara os saldos todo dia. Como a planilha continua completa, voltar a ela não perde nada.
3. **Corte por loja:** com os saldos batendo, a loja passa a usar só o sistema, e depois seguem as outras duas.
4. **Encerramento:** as planilhas são arquivadas.

É a lógica de migrações incrementais que já conduzi em outros sistemas: conviver com o antigo, comparar e só então desligar.

<a id="11"></a>
## 11. Evoluções futuras

* Lançamento de ajuste para corrigir erros, e reabertura do caixa
* Saldo acumulado entre os dias
* CQRS completo com Event Sourcing
* Cache nas consultas do *Consolidado*
* Atualização em tempo real da tela de saldo
* No relatório, um seletor de data para escolher o dia dos lançamentos, entre os dias que têm registros
* Alta disponibilidade dos bancos, do RabbitMQ e das aplicações
* Reconciliação do saldo com os lançamentos, e reconstrução em caso de diferença
* Atender mais de uma empresa, com isolamento por empresa no banco
* Reprocessamento das mensagens da fila de erros
