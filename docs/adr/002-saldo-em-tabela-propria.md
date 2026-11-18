# ADR 002: Saldo diário em tabela própria e atualizado por eventos

* **Data:** 18/11/2026
* **Status:** Aceito
* **Autores:** Leandro Fernandes

## Contexto
O acesso para consulta pelo proprietário e auxiliar administrativo é diário, podendo gerar um pico de 50 requisições por segundo, apontado no RNF002. A cada consulta do saldo, terá que ser feito a soma de todos os lançamentos do dia.

## Decisão
A decisão a cada lançamento escrito na tabela de lançamentos, disparar um evento que calcula o saldo e grava em um banco e tabela próprios.

## Alternativas consideradas
* **Opção A:** **tabela única.** Uma única tabela simplificaria bastante e eliminaria a necessidade de manter as informações sincronizadas em dois lugares, mas custaria o RNF001 enem o RNF002.
* **Opção B:** **cálculo agendado**. Em determinados intevalos de tempo o saldo seria calculado, mas sempre defasado, até a próxima atualização, além de também ficar recalculando necessidade.

## Consequências
* **Positivas:** o *Consolidado* pode ter várias instâncias e a leitura é muito rápida
* **Negativas / Trade-offs:** a atualização do saldo pode ter insconsistências em realação ao último lançamento, ficando defasado por alguns segundos. É preciso garantir que o evento não se repita.