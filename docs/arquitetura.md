# Arquitetura do Sistema

Este documento é o guia de referência da arquitetura do sistema de **Fluxo de Caixa**, tomando como exemplo o caso da **Fernandes Mecânica**. Nele detalhamos o contexto de negócio e os demais pontos que envolvem o projeto.

## Índice

1. [Visão geral](#1)
   - 1.1 [Escopo](#1_1)
2. [Contexto](#2)
   - 2.1 [Atores](#2_1)
   - 2.2 [Domínios](#2_2)
   - 2.3 [Considerações](#2_3)
3. [Requisitos](#3)
   - 3.1 [Requisitos funcionais](#3_1)
   - 3.2 [Requisitos não funcionais](#3_2)
4. [Arquitetura alvo](#4)
5. [Decisões](#5)
6. [Segurança e LGPD](#6)
7. [Qualidade e observabilidade](#7)
8. [Riscos e impactos](#8)
9. [Custos](#9)
10. [Transição](#10)
11. [Resumo das decisões](#11)
12. [Evoluções futuras](#12)

<a id="1"></a>
## 1. Visão geral

O comerciante precisa controlar seu fluxo de caixa diário, com lançamentos de débito e crédito, e consultar o saldo diário consolidado.

O **Sistema de Fluxo de Caixa** está sendo pensado para atender a **Fernandes Mecânica**, com o intuito de substituir o processo que hoje é feito em planilhas, de maneira descentralizada, por um modelo SaaS multi-tenant que fará a unificação das informações das lojas.

A solução é dividida em dois serviços independentes. O *serviço de lançamentos* registra cada débito e crédito e responde ao caixa na hora. A cada lançamento, ele avisa o outro serviço por meio de um evento. O *serviço de consolidado* recebe esses eventos e mantém o saldo diário de cada loja já calculado, pronto para consulta. Se o consolidado ficar fora do ar, os lançamentos continuam sendo aceitos e os eventos esperam na fila, e o saldo é atualizado quando o serviço voltar.

<a id="1_1"></a>
### 1.1 Escopo

* O que o sistema faz
   * Lançamento de entradas (créditos) e saídas (débitos) por loja
   * Emissão de relatório do saldo diário e dos lançamentos, consolidado por loja
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
| **Proprietário** | Vê e confere o caixa de todas as lojas todos os dias, através do relatório consolidado |
| **Auxiliar administrativo** | Vê e confere o caixa da loja em que trabalha, através do relatório consolidado |
| **Caixa** | Opera o caixa da loja em que trabalha, registra as entradas e saídas e fecha o caixa no final do dia |

<a id="2_2"></a>
### 2.2 Domínios

| Domínio | Ações |
| --- | --- |
| Lançamentos | Abrir caixa, Registrar entrada, Registrar saída, Fechar caixa e Validar fechamento |
| Consolidado | Consolidar o saldo diário por loja, Consultar o saldo de uma loja, Consultar o saldo da empresa e Disponibilizar o relatório |
| Transversal | Controlar o acesso por papel e por loja |

<a id="2_3"></a>
### 2.3 Considerações

* O saldo diário é por loja: o proprietário vê o de cada loja e o total das três.
* Os dados pertencem à empresa, e cada loja é uma divisão dela. O acesso é controlado por loja: o auxiliar administrativo e o caixa veem somente a própria loja.
* Cada loja tem um caixa.
* Ao fechar o caixa, o dia é encerrado e o proprietário pode ver o fechamento através do consolidado.

<a id="3"></a>
## 3. Requisitos

<a id="3_1"></a>
### 3.1 Requisitos funcionais

| Código | Domínio | Descrição |
| --- | --- | --- |
| RF01 | Lançamentos | Abrir o caixa da loja no início do dia. Cada loja tem um caixa aberto por dia. |
| RF02 | Lançamentos | Registrar uma *entrada* com valor, data e descrição, somente com o caixa aberto. Lançamentos são imutáveis. |
| RF03 | Lançamentos | Registrar uma *saída* com valor, data e descrição, somente com o caixa aberto. Lançamentos são imutáveis. |
| RF04 | Lançamentos | Fechar o caixa. O dia da loja é encerrado e não aceita mais lançamentos. |
| RF05 | Lançamentos | Validar o fechamento do caixa da loja. Depois de fechado, o caixa não reabre no mesmo dia. |
| RF06 | Consolidado | Atualizar o saldo diário da loja a cada lançamento registrado. Saldo do dia = créditos − débitos daquele dia, por loja. |
| RF07 | Consolidado | Consultar o saldo diário de uma loja. |
| RF08 | Consolidado | Consultar o saldo diário da empresa, somando as lojas. Saldo da empresa = soma dos saldos do dia de cada loja. |
| RF09 | Consolidado | Disponibilizar o relatório do dia, com lançamentos e saldo, por loja e para a empresa. |
| RF10 | Transversal | Autenticar o usuário e identificar seu papel e sua loja. |
| RF11 | Transversal | Restringir o acesso por loja: caixa e auxiliar veem só a própria, o proprietário vê todas. |
| RF12 | Transversal | Restringir as ações por papel: só o caixa lança, abre e fecha; só o proprietário valida; o auxiliar só consulta. |

<a id="3_2"></a>
### 3.2 Requisitos não funcionais

| Código | Requisito | Meta | Como verificar |
| --- | --- | --- | --- |
| RNF01 | Isolamento de falha | Com o Consolidado parado, o Lançamentos aceita 100% dos lançamentos válidos. | Derrubar o Consolidado, enviar lançamentos e conferir que nenhum falhou. |
| RNF02 | Desempenho do consolidado | 50 req/s durante 5 minutos, com no máximo 5% de falha e resposta em até 500 ms. | Teste de carga. |
| RNF03 | Atraso do saldo | Até 5 segundos depois do lançamento. Depois de uma parada, até 5 minutos após o retorno. | Medir o tempo entre o lançamento e o saldo atualizado. |
| RNF04 | Disponibilidade | Lançamentos 99,5%. Consolidado 99%. | Monitoramento de disponibilidade. |
| RNF05 | Durabilidade e sem duplicidade | Nenhum lançamento aceito se perde. Um evento repetido não altera o saldo duas vezes. | Teste de integração. |

<a id="4"></a>
## 4. Arquitetura alvo

...

<a id="5"></a>
## 5. Decisões

...

<a id="6"></a>
## 6. Segurança e LGPD

...

<a id="7"></a>
## 7. Qualidade e observabilidade

...

<a id="8"></a>
## 8. Riscos e impactos

...

<a id="9"></a>
## 9. Custos

...

<a id="10"></a>
## 10. Transição

...

<a id="11"></a>
## 11. Resumo das decisões

...

<a id="12"></a>
## 12. Evoluções futuras

...