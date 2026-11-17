# Arquitetura do Sistema

Este documento é o guia de referência da arquitetua do sistema de **Fluxo de Caixa**, tomando como exemplo o cado da **Fernandes Mecânica**. Nele estaremos detalhar o o contexto de negócio e os demais detalhamentos que envovem projeto.

## Índice

1. [Visão Geral](docs/visao-geral.md)
2. [Contexto](docs/context.md)
3. [Requisitos](docs/requisitos.md)
4. [Arquitetura alvo](docs/arquitetura-alvo.md)
5. [Decisões](docs/decisoes.md)
6. [Segurança e LGPD](docs/seguranca-lgpd.md)
7. [Qualidade e observabilidade](docs/qualidade-observabilidade.md)
8. [Riscos e impactos](docs/riscos-impactos.md)
9. [Custos](docs/custos.md)
10. [Transição](docs/transicao.md)
11. [Resumo das decisões](docs/resumo.md)
12. [Evoluções futuras](docs/evolucoes-futuras.md)


## 1. Visão geral
O **Sistema de Fluxo de Caixa** está sendo elaborado para atender a **Fernandes Mecânica** com o intuido te subistituir o processo que hoje é feito em planilhas e de mandeira descentralizada, para um modelo em SaaS que vai proporcinar a unificação das informações das lojas.
### 1.1 Contexto
* O que o sistema faz
   * Lançamento de entradas (créditos) e saídas (débitos) por empresa
   * Emite um relatório do saldo diário e lançamentos consolidado por empresa 
* O que o sistema **não faz**
   * Controle de estoque
   * Controle de ordens de serviço
   * Realizar orçamentos e vendas de produtos
   * Emissão de nota fiscal

### 1.2 Atores
* **Proprietário**
* **Auxiliar Administrativo**
* **Caixa**