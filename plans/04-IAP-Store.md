# Plano 6: Compras In-App (IAP) e Play Store

Implementação de monetização segura e validação de recibos.

## 1. Catálogo de Produtos
- **Consumíveis:** Star Maps (Pacotes de 100, 500, 1000).
- **Não-Consumíveis:** "Remover Anúncios" ou "Passe de Batalha".
- **Assinaturas:** (Opcional) Plano mensal de recursos.

## 2. Segurança (Anti-Hack)
- **Validação de Recibo:** Usar o serviço de validação da Unity no Cloud Code. O item só é adicionado à `Account` se o Google confirmar que o dinheiro caiu.
- **Fluxo de Devolução:** Lógica para remover itens caso uma compra seja estornada (Refund).

## 3. UI de Loja
- Carregamento dinâmico de preços (ex: R$ 10,90 vs $ 1.99) usando as strings localizadas vindas da loja.
- Botão "Restaurar Compras" (Obrigatório para iOS, boa prática no Android).

## 4. Testes
- Usar o **IAP Catalog** local para testar compras com sucesso/falha sem gastar dinheiro real durante o desenvolvimento.
