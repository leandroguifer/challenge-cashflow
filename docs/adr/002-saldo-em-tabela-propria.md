# ADR 002: Saldo diário em tabela própria e atualizado por eventos

* **Data:** 18/11/2026
* **Status:** Aceito
* **Autores:** Leandro Fernandes

## Contexto
O acesso para consulta pelo proprietário e auxiliar administrativo é diário, podendo gerar um pico de 50 requisições por segundo, apontado no  RNF02. A cada consulta do saldo, terá que ser feito a soma de todos os lançamentos do dia.

## Decisão
A cada lançamento gravado, o *Lançamentos* dispara um evento. O *Consolidado* recebe o evento, calcula o saldo do dia da loja e o grava em um banco e uma tabela próprios.

## Alternativas consideradas

* **Opção A:** **tabela única.** Uma única tabela simplificaria bastante e eliminaria a necessidade de manter as informações sincronizadas em dois lugares, mas comprometeria o  RNF01 enem o  RNF02.
* **Opção B: cálculo agendado.** O saldo seria recalculado em intervalos de tempo (por exemplo, a cada hora). Seria simples de implementar, mas o saldo ficaria defasado até o próximo cálculo, o que não atende ao RNF03 (saldo em até 5 segundos), e dias inteiros seriam recalculados mesmo quando nada mudou.

## Consequências
* **Positivas:** o *Consolidado* pode ter várias instâncias e a leitura é muito rápida
* **Negativas / Trade-offs:** a atualização do saldo pode ter insconsistências em realação ao último lançamento, ficando defasado por alguns segundos. É preciso garantir que o evento não se repita.