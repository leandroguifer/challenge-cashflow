# Arquitetura do Sistema

Este documento é o guia de referência da arquitetua do sistema de **Fluxo de Caixa**, tomando como exemplo o cado da **Fernandes Mecânica**. Nele estaremos detalhar o o contexto de negócio e os demais detalhamentos que envovem projeto.

## 1. Visão geral
O **Sistema de Fluxo de Caixa** está sendo elaborado para atender a **Fernandes Mecânica** com o intuido te subistituir o processo que hoje é feito em planilhas e de mandeira descentralizada, para um modelo em SaaS que vai proporcinar a unificação das informações das lojas.
### 1.1 Contexto
* O que o sistema faz
   * Lançamento de entradas (créditos) e saídas (débitos) por loja
   * Emite um relatório do saldo diário e lançamentos consolidado por loja
* O que o sistema **não faz**
   * Controle de estoque
   * Controle de ordens de serviço
   * Realizar orçamentos e vendas de produtos
   * Emissão de nota fiscal

### 1.2 Atores
* **Proprietário**: vê e confere o caixas de todas as lojas todos os dias
* **Auxiliar Administrativo**: vê e confere o caixa da loja que trabalha
* **Caixa**: opera o caixa da loja que trabalha