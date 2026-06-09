# Backlog e Tarefas Futuras (TO-DO)

Este documento guarda funcionalidades, refinamentos e ideias que foram mapeadas durante o desenvolvimento, mas que foram postergadas para não interromper o fluxo atual.

## 🐾 Sistema de Pets (Resíduo do Plano 2)

**[ ] Progressão e Level Up de Pets usando Poeira Estelar (Stardust)**
- **Regras Matemáticas:** Definir uma fórmula ou curva de custo de Poeira Estelar necessária para subir cada nível do Pet (ex: escalar com base na raridade/estrelas).
- **Escalonamento de Status:** Criar a lógica que aumenta os status randômicos do pet (HP, ATK, DEF, SPD, CRIT, ACC) a cada level up.
- **Interface de Usuário (UI):** 
  - Atualizar o `PetManageModal` (e seu script gerador `UIBuilder_PetManagementUI`) para incluir um botão de "Evoluir" ou "Level Up".
  - O botão deve mostrar o custo em Poeira Estelar e ficar desabilitado caso o jogador não tenha saldo suficiente.
- **Persistência de Dados:** Garantir que o consumo de Poeira Estelar da conta e o incremento do `CurrentLevel` do `RuntimePetData` sejam devidamente salvos no `AccountManager`.