# ADR 004: CQRS leve, com EF Core na escrita e Dapper na leitura

* **Data:** 18/11/2026
* **Status:** Aceito
* **Autores:** Leandro Fernandes

## Contexto
O *Lançamentos* aplica regras de negócio e grava dados com transação. O *Consolidado* só lê o saldo, e precisa atender 50 requisições por segundo com no máximo 5% de falha (RNF02). Escrita e leitura têm necessidades diferentes.

## Decisão
O *Lançamentos* usa EF Core na escrita. O *Consolidado* usa Dapper na leitura, com SQL direto sobre a tabela de saldo diário. Comandos e consultas ficam em classes separadas.

## Alternativas consideradas
* **Opção A: EF Core para escrita e leitura.** É a mais simples e tem uma só forma de acesso a dados, mas a leitura passa por uma camada de mapeamento que ela não precisa, e o SQL fica menos explícito.
* **Opção B: CQRS completo com Event Sourcing.** Guardaria todos os eventos como fonte da verdade, mas é bem mais complexo e não é necessário para o escopo atual. Fica como evolução futura.

## Consequências
* **Positivas:** a leitura é simples e o SQL é visível, o que facilita medir e ajustar no teste de carga (RNF02). A escrita mantém as regras e a transação no EF Core.
* **Negativas / trade-offs:** o projeto tem duas formas de acesso a dados, e o mapeamento da leitura é feito à mão.