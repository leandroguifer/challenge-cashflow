# ADR 005: Acesso por empresa e loja com Keycloak e RLS no PostgreSQL


* **Status:** Aceito
* **Autores:** Leandro Fernandes

## Contexto
O sistema precisa autenticar o usuário, restringir o acesso por loja e limitar as ações por papel (RF09 a RF11). Uma loja não pode ver os dados de outra, nem por erro de código.

## Decisão
O Keycloak autentica (OIDC). A empresa é um grupo pai, e cada loja é um subgrupo. Os papéis definem as ações permitidas. Há três clients (front-end, API de Lançamentos e API de Consolidado), ligados por audiência. A API valida assinatura, emissor, audiência e validade do token, lê as lojas do claim `groups`, recusa o pedido de uma loja que não está nele e, a cada transação, informa essas lojas ao PostgreSQL. O banco aplica RLS (com `FORCE ROW LEVEL SECURITY`) e, sem essa informação, não retorna nenhuma linha. O usuário do banco usado pela aplicação não é dono das tabelas. O publicador do outbox e o consumidor usam usuários próprios, com políticas explícitas.

## Alternativas consideradas
* **Opção A (descartada): um banco ou schema por empresa.** Isola bem, mas multiplica migrações e operação, o que não se justifica para poucas lojas. O isolamento por loja continuaria necessário dentro de cada banco.
* **Opção B (descartada): filtro só na aplicação.** É simples, mas um erro de código (uma consulta sem o filtro) expõe dados de outra loja.

## Consequências
* **Positivas:** o banco garante o isolamento mesmo se uma consulta esquecer o filtro por loja.
* **Negativas / trade-offs:** o sistema depende do Keycloak: sem ele não há login nem renovação de token, e o caixa para de lançar quando o token expira. Em produção, a proposta é ter mais de uma instância do Keycloak e banco próprio com redundância. Uma mudança de grupo não altera um token já emitido: até ele expirar, vale o acesso antigo. A RLS precisa de teste próprio (isolamento entre lojas). O isolamento é por loja: o banco não separa empresas, e o sistema atende uma só.