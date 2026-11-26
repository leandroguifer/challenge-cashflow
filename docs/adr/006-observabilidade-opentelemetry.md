# ADR 006: Observabilidade com OpenTelemetry


* **Status:** Aceito
* **Autores:** Leandro Fernandes

## Contexto
Um lançamento passa por dois serviços, de forma assíncrona. É preciso acompanhar esse caminho para investigar falhas e atrasos na atualização do saldo (RNF03). Sem isso, um atraso só seria percebido quando o proprietário notasse o saldo desatualizado.

## Decisão
As duas APIs são instrumentadas com OpenTelemetry (traces, métricas e logs) e enviam os dados por OTLP. O contexto do trace é gravado no outbox e segue nos cabeçalhos da mensagem no RabbitMQ, para que um só trace ligue o lançamento à atualização do saldo. Na demonstração, os dados vão para o Aspire Dashboard. Em produção, a proposta é Prometheus e Grafana, com alerta quando a fila acumula por mais de 1 minuto.

## Alternativas consideradas
* **Opção A (descartada): biblioteca própria de um fornecedor de monitoramento.** A integração é direta, mas amarra o código a esse fornecedor, e trocar de ferramenta exige mudar a aplicação.
* **Opção B (descartada): uma biblioteca para cada tipo de dado.** Permite escolher a ferramenta de cada um, mas soma configurações diferentes, e ligar logs e métricas ao trace fica por conta do projeto.

## Consequências
* **Positivas:** o formato de envio é um padrão aberto, o que permite trocar a ferramenta de destino sem mudar a instrumentação. Um só trace mostra o caminho do lançamento até o saldo. Uma só configuração cobre métricas, traces e logs, com instrumentação pronta para o ASP.NET Core, as chamadas HTTP e o Npgsql.
* **Negativas / trade-offs:** o Prometheus guarda só métricas, e os traces e logs precisam de outro destino em produção, ainda a escolher. As aplicações não publicam métrica do atraso no outbox nem do tamanho da fila, e o alerta depende de métrica coletada do RabbitMQ. Nenhum teste automatizado garante que o trace atravessa o RabbitMQ.
