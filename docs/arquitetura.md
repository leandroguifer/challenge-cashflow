# Arquitetura do Sistema

Este documento é o guia de referência da arquitetua do sistema de **Fluxo de Caixa**, tomando como exemplo o cado da **Fernandes Mecânica**. Nele detalhamos o contexto de negócio e os demais pontos que envovem projeto.

## 1. Visão geral

O comerciante precisa controlar seu fluxo de caixa diário, com lançamentos de débito e crédito, e consultar o saldo diário consolidado.

O **Sistema de Fluxo de Caixa** está sendo pensado para atender a **Fernandes Mecânica** com o intuito de substituir o processo que hoje é feito em planilhas, de mandeira descentralizada, pora um modelo em SaaS multi-tenant fará a unificação das informações das lojas.

A solução é dividida em dois serviços independentes. O *serviço de lançamentos* registra cada débito e crédito e responde ao caixa na hora. A cada lançamento, ele avisa o outro serviço por meio de um evento. O *serviço de consolidado* recebe esses eventos e mantém o saldo diário de cada loja já calculado, pronto para consulta. Se o consolidado ficar fora do ar, os lançamentos continuam sendo aceitos e os eventos esperam na fila, e o saldo é atualizado quando o serviço voltar.

### 1.1 Escopo
* O que o sistema faz
   * Lançamento de entradas (créditos) e saídas (débitos) por loja
   * Emite um relatório do saldo diário e lançamentos consolidado por loja
* O que o sistema **não faz**
   * Controle de estoque
   * Controle de ordens de serviço
   * Realizar orçamentos e vendas de produtos
   * Emissão de nota fiscal

# 2 Contexto

### 2.1 Atores
| Ator | Função |
| --- | --- |
| **Proprietário** | vê e confere o caixas de todas as lojas todos os dias através do relatório consolidado |
| **Auxiliar Administrativo** | vê e confere o caixa da loja que trabalha através do relatório consolidado |
| **Caixa** | opera o caixa da loja que trabalha, registrar as entradas e saídas e fechar o caixa no final do dia |

## 2.2 Domínios

| Domínio | Ações |
| --- | --- |
| Lançamentos | Abrir caixa, Registrar entrada, Registrar saída,  Fechar caixa  e Validar Fechamento |
| Consolidado | Consolidar o saldo diário por loja, Conulstar o saldo de uma loja, Consultar saldo do grupo e Disponibilizar relatório |
| Transversal | Controlar o acesso por papel por loja |


## 2.3 Considerações
* O saldo diário é por loja, sendo que o proprietário vê de cada loja e o total das 3
* Os dados pertencem a empresa, cada loja é a divisão dela e o acesso é controlado por loja. O auxiliar administrativo e o caixa veêm somente a própria loja
* Cada loja tem um caixa
* Ao fechar o caixa, o dia é encerrado e o proprietário pode ver o fechamento através do consolidado