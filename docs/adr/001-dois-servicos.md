# ADR 001: Separar  *Lançamentos* e *Consolidado* em dois serviços


* **Status:** Aceito
* **Autores:** Leandro Fernandes

## Contexto
O RNF01 pede que haja "Isolamento de Falha". O caixa realiza lançamentos durante o dia, enquanto o proprietário e o auxiliar administrativo consultam, podendo chegar a um pico de 50 requisições por segundo (RNF02). Os lançamentos não podem ficar indisponíveis caso o *Consolidado* caia durante esse pico. Com o *Consolidado* parado, o *Lançamentos* deve aceitar 100% dos lançamentos válidos.

## Decisão
Separar em dois serviços, com implantação e bancos próprios, cada banco em seu servidor PostgreSQL: dois serviços no `docker compose` na demonstração e dois servidores em produção.

## Alternativas consideradas
* **Opção A (descartada):** **um serviço único.** A construção e a implantação seriam muito mais simples, porém não garantiria o isolamento de falha: a *consulta consolidada* poderia quebrar a aplicação ou o banco.
* **Opção B (descartada):** **serviços separados, mas com um único servidor de banco**, mesmo com um banco para cada serviço. O isolamento funcionaria somente para a aplicação: se o servidor de banco parar, os dois serviços param.

## Consequências
* **Positivas:** a parada do *Consolidado* ou do servidor do banco dele não impede os lançamentos.
* **Negativas / Trade-offs:** há mais serviços e servidores de banco para implantar, operar e pagar, e o saldo do *Consolidado* não é atualizado na hora. O isolamento não cobre componentes que continuam compartilhados, como a aplicação web, o servidor das APIs e a autenticação.