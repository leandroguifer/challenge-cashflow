# ADR 002: Saldo diário em tabela própria e atualizado por eventos

* **Status:** Aceito
* **Autores:** Leandro Fernandes

## Contexto
O proprietário e o auxiliar administrativo consultam o saldo diariamente, podendo gerar um pico de 50 requisições por segundo (RNF02). Se o saldo fosse calculado na consulta, cada uma teria que somar todos os lançamentos do dia.

## Decisão
A cada lançamento gravado, o *Lançamentos* dispara um evento. O *Consolidado* recebe o evento, soma o valor ao saldo do dia da loja e o grava em um banco e uma tabela próprios.

## Alternativas consideradas
* **Opção A (descartada):** **tabela única.** Uma única tabela simplificaria bastante e eliminaria a necessidade de manter as informações sincronizadas em dois lugares, mas comprometeria o RNF01 e o RNF02.
* **Opção B (descartada):** **cálculo agendado.** O saldo seria recalculado em intervalos de tempo (por exemplo, a cada hora). Seria simples de implementar, mas o saldo ficaria defasado até o próximo cálculo, o que não atende ao RNF03 (saldo em até 5 segundos), e dias inteiros seriam recalculados mesmo quando nada mudasse.

## Consequências
* **Positivas:** a consulta lê o saldo já calculado, o que a torna rápida.
* **Negativas / Trade-offs:** o saldo fica defasado em relação ao último lançamento, por mais tempo se o *Consolidado* ou a mensageria pararem. O mesmo evento pode chegar mais de uma vez, por isso o *Consolidado* registra os eventos processados e ignora os repetidos.