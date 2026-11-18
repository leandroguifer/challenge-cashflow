# ADR 001: Separar  *Lançamentos*  e *Consolidado* em dois serviços

* **Data:** 18/11/2026
* **Status:** Aceito
* **Autores:** Leandro Fernandes

## Contexto
O RNF001 pede que hava "Isolamento de Falha" e o caixa realiza lançamenteos durante o dia, enquanto o proprietário e o auxiliar administrativo consultam, podendo chegar a um pico de 50 requisições por segundo (RNF02). E o requisito exige que os lançamentos não fiquem indisponíveis, caso o *Consolidado* caia durante o pico exigido.

## Decisão
A decisão foi de separar em dois serviços de deploy e bases diferentes.

## Alternativas consideradas
* **Opção A:** **um serviço único.** A sua construção e implantação seria muito mais simples, porém não vai garantir o isolamento de falha, podendo a *consulta consolidada*, quebrar a aplicação ou o banco.
* **Opção B:** *serviços separados e isolados mas no mesmo banco**. Acaba que usar o mesmo banco para os dois, não garantiria o isolamento necessário, pois iria funcionar somente para a aplicação, mas se o banco parar, o isolamento é quebrado.

## Consequências
* **Positivas:** Garante que mesmo que o *Consolidado* pare, os lançamentos irão continuar
* **Negativas / Trade-offs:** Como duplicamos serviços e bancos, aumenta o trablhalho para implantar o operar e não temos o caixa atualizando o *Consolidado* na hora