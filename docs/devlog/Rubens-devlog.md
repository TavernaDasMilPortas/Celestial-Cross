# Devlog Celestial Cross - Rubens

Este log detalha a evolução cronológica do projeto, abrangendo mudanças estruturais, visuais e correções técnicas implementadas recentemente.

---

## 1. Revitalização Visual e Novos Tilesets
**Data: 08/05/2026 - 10/05/2026**
### **Problema:**
A estética inicial de lineart precisava ser substituída por um estilo mais polido e detalhado para elevar a qualidade visual do jogo.

### **Solução:**
*   **Transição de Assets:** Substituição completa dos tilesets antigos por modelos de alta fidelidade.
*   **Suporte a Camadas:** Melhoria no interpretador de mapas para suportar múltiplas camadas de sprites e transparências, permitindo mapas mais ricos.

---

## 2. Interface de Gacha e Loja
**Data: 11/05/2026**
### **Problema:**
O sistema de invocação e a loja precisavam de uma experiência de usuário (UX) mais fluida e atrativa.

### **Solução:**
*   **Gacha Visuals:** Adicionadas animações e materiais especiais para a sequência de invocação de unidades.
*   **Arrumação da UI:** Refatoração completa do layout da loja para facilitar a navegação em dispositivos mobile.

---

## 3. Sistema de Tutoriais e Spotlight Shader
**Data: 12/05/2026**
### **Problema:**
Dificuldade de novos jogadores em entender as mecânicas sem um guia visual direto.

### **Solução:**
*   **Spotlight System:** Desenvolvimento de um shader que destaca botões ou unidades específicas, escurecendo o restante da tela.
*   **Módulos Interativos:** Fluxos de tutorial que bloqueiam inputs externos até que o jogador execute a ação ensinada.

---

## 4. Otimização de Performance: Mesh-based Tiles
**Data: 13/05/2026**
### **Problema:**
Uso excessivo de GameObjects para o grid estava impactando o FPS (Draw Calls altas).

### **Solução:**
*   **Conversão para Mesh:** O grid foi otimizado para renderizar tiles via Mesh instanciada, reduzindo drasticamente o processamento necessário para o cenário.

---

## 5. Implementação da Heroína: Leidell
**Data: 14/05/2026**
### **Problema:**
Necessidade de testar o sistema de habilidades com uma unidade de alta complexidade.

### **Solução:**
*   **Abilities Setup:** Configuração das habilidades da Leidell via grafos, o que serviu como base para identificar gargalos no interpretador e no sistema de buffs.

---

## 6. Refatoração do Sistema de Atributos (Status Dinâmicos)
**Data: 14/05/2026**
### **Problema:**
Buffs e debuffs eram aplicados de forma estática, não atualizando o dano ou a UI em tempo real após a aplicação.

### **Solução:**
*   **Stats Dinâmicos:** A propriedade `Unit.Stats` agora recalcula bônus em tempo real consultando o `PassiveManager`.
*   **Cálculo Transparente:** Implementação de bônus planos e percentuais que se somam corretamente.

---

## 7. Otimização do CombatLogger e Debug Visual
**Data: 14/05/2026**
### **Problema:**
Falta de clareza sobre o estado interno das unidades durante o combate.

### **Solução:**
*   **Status Monitor (Live):** Adicionado monitor de atributos (ATK, DEF, SPD, HP) em tempo real no Inspector do Logger.
*   **Logs Granulares:** Detalhamento de cada etapa de execução dos grafos de habilidade.

---

## 8. Correções no Interpretador de Grafos (Distância e Facção)
**Data: 14/05/2026**
### **Problema:**
Bugs de desserialização faziam com que valores de distância fossem lidos como zero.

### **Solução:**
*   **Sincronização Editor/Runtime:** Alinhamento dos campos de dados entre a interface visual e o código de execução.
*   **Lógica de Facção:** Integração de filtros de Ally/Enemy diretamente no nó de distância.

---

## 9. Robustez na Seleção de Áreas e Habilidades
**Data: 14/05/2026**
### **Problema:**
Áreas de efeito de habilidades anteriores persistiam no grid após a troca de ação.

### **Solução:**
*   **Cleanup Agressivo:** O executor de habilidades agora garante a destruição de qualquer seletor antigo antes de iniciar o próximo.

---

## 10. Projeção de Popups de Dano (Suporte a RenderTexture)
**Data: 14/05/2026**
### **Problema:**
Popups 3D não apareciam corretamente quando o jogo era exibido através de uma RawImage.

### **Solução:**
*   **World-to-UI Projection:** Sistema que projeta o dano do mundo 3D diretamente para o espaço de tela da UI, ajustando escala e posição para garantir nitidez.

---

## Próximos Passos
*   Finalizar a transição dos popups de dano para o sistema projetado (Overlay).
*   Refinar a transição de foco de câmera entre múltiplas ações do mesmo turno.
*   Testar a persistência das passivas da Leidell em batalhas de longa duração.
