# ADR 003: RabbitMQ com outbox e consumidor sem duplicidade

* **Data:** 18/11/2026
* **Status:** Aceito
* **Autores:** Leandro Fernandes

## Contexto
O serviço de *Consolidado* tem que receber cada lançamento do serviço de lançamento* e nenhum pode ser perdido. Podem haver eventos repetidos e esses não podem alterar o saldo duas vezes, conforme RNF005. 

## Decisão
O serviço de Lançamentos grava o lançamento e o evento correspondente no mesmo banco, na mesma transação (outbox). Um processo em segundo plano publica os eventos pendentes no RabbitMQ. O serviço de *Consolidado* consome os eventos e registra os já processados, para ignorar repetições. Mensagens que falham de forma repetida vão para uma fila de erros.

## Alternativas consideradas
* **Opção A: o *Consolidado* ler direto o banco de Lançamentos.** É a mais simples, mas acopla os dois serviços ao mesmo banco: se ele parar, os dois param. Não atende ao RNF01.
* **Opção B: entrega por HTTP, de um serviço para o outro.** Evita o broker, mas exigiria implementar tentativas de reenvio e retomada depois de uma parada, que o RabbitMQ já oferece.

## Consequências
* **Positivas:** o Lançamentos grava só no próprio banco e continua funcionando com o *Consolidado* parado. Os eventos esperam na fila e são entregues quando o serviço volta.
* **Negativas / trade-offs:** o evento pode chegar mais de uma vez, o que exige o consumidor sem duplicidade. Uma mensagem com erro precisa de tratamento (fila de erros). O broker é mais uma peça para operar, e, se ele cair, os eventos esperam no outbox até ele voltar.