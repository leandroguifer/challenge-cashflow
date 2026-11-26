# ADR 004: CQRS leve, com EF Core na escrita e Dapper na leitura


* **Status:** Aceito
* **Autores:** Leandro Fernandes

## Contexto
O *Lançamentos* aplica regras de negócio e grava dados com transação. O *Consolidado* grava o saldo ao consumir os eventos, e a consulta do saldo precisa atender 50 requisições por segundo com no máximo 5% de falha (RNF02). Escrita e leitura têm necessidades diferentes.

## Decisão
Nos dois serviços, a escrita usa EF Core, e as consultas usam Dapper, com SQL direto. No *Consolidado*, a consulta do saldo lê a tabela de saldo diário.

## Alternativas consideradas
* **Opção A (descartada): EF Core para escrita e leitura.** É a mais simples e tem uma só forma de acesso a dados, mas a leitura passa por uma camada de mapeamento que ela não precisa, e o SQL fica menos explícito.
* **Opção B (descartada): CQRS completo com Event Sourcing.** Guardaria todos os eventos como fonte da verdade, mas é bem mais complexo e não é necessário para o escopo atual. Fica como evolução futura.

## Consequências
* **Positivas:** a leitura é simples e o SQL é visível, o que facilita medir e ajustar no teste de carga (RNF02). A escrita mantém as regras e a transação no EF Core.
* **Negativas / trade-offs:** o projeto tem duas formas de acesso a dados, e o mapeamento da leitura é feito à mão.