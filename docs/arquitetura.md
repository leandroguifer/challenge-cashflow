# Arquitetura do Sistema

Este documento é o guia de referência da arquitetua do sistema de **Fluxo de Caixa**, tomando como exemplo o cado da **Fernandes Mecânica**. Nele estaremos detalhar o o contexto de negócio e os demais detalhamentos que envovem projeto.

## 1. Visão geral

O comerciante precisa controlar seu fluxo de caixa diário, com lançamentos de débito e crédito, e consultar o saldo diário consolidado.

O **Sistema de Fluxo de Caixa** está sendo elaborado para atender a **Fernandes Mecânica** com o intuido te subistituir o processo que hoje é feito em planilhas e de mandeira descentralizada, para um modelo em SaaS multi-tenant que vai proporcinar a unificação das informações das lojas.

### 1.1 Escopo
* O que o sistema faz
   * Lançamento de entradas (créditos) e saídas (débitos) por loja
   * Emite um relatório do saldo diário e lançamentos consolidado por loja
* O que o sistema **não faz**
   * Controle de estoque
   * Controle de ordens de serviço
   * Realizar orçamentos e vendas de produtos
   * Emissão de nota fiscal

### 1.2 Atores
* **Proprietário**: vê e confere o caixas de todas as lojas todos os dias através do relatório consolidado
* **Auxiliar Administrativo**: vê e confere o caixa da loja que trabalha através do relatório consolidado
* **Caixa**: opera o caixa da loja que trabalha, registrar as entradas e saídas e fechar o caixa no final do dia

## 2 Domínios

| Domínio | Ações |
| --- | --- |
| Lançamentos | Abrir caixa, Registrar entrada, Registrar saída,  Fechar caixa  e Validar Fechamento |
| Consolidado | Consolidar o saldo diário por loja, Conulstar o saldo de uma loja, Consultar saldo do grupo e Disponibilizar relatório |
| Transversal | Controlar o acesso por papel por loja |


## 3 Considerações
* O saldo diário é por loja, sendo que o proprietário vê de cada loja e o total das 3
* Os dados pertencem a empresa, cada loja é a divisão dela e o acesso é controlado por loja. O auxiliar administrativo e o caixa veêm somente a própria loja
* Cada loja tem um caixa
* Ao fechar o caixa, o dia é encerrado e o proprietário pode ver o fechamento através do consolidado



