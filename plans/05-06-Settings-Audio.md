# Plano 05: User Settings & Profile Management

Este plano foca na criação de uma infraestrutura robusta para preferências do usuário, dados cadastrais e personalização de perfil, incluindo uma cena dedicada de configurações.

## 1. Estrutura de Dados (Account & Settings)
- **SettingsData**: Classe serializável para salvar volumes (Mestre, Música, SFX), Idioma, Qualidade Gráfica e Notificações.
- **UserProfile**: Extensão do `PlayerAccount` para incluir:
    - `BirthDate` (para conformidade legal/aniversários).
    - `ProfileImageID` (referência a um SO de Sprites).
    - `PlayerName` (com sistema de validação).
    - `Language` (integração com Localization).

## 2. Cena de Configurações (SettingsScene)
- **Interface de Cadastro**: Formulário para Nome e Data de Nascimento (apenas se não preenchidos).
- **Galeria de Avatares**: Sistema de seleção de ícones desbloqueáveis (ScriptableObjects).
- **Abas de Ajustes**:
    - **Áudio**: Sliders para volumes.
    - **Gráficos**: Toggle de FPS e Qualidade.
    - **Conta**: Exibição do ID da Unity, opção de Logout/Link Google.

## 3. Persistência
- Integração com `AccountManager` para garantir que as preferências sejam salvas no `CloudSave`.
- Carregamento automático das configurações no Boot do jogo.

---

# Plano 06: Sistema de Áudio (Music & SFX)

Implementação de um motor de áudio centralizado para gerenciar a sonoridade do jogo.

## 1. AudioManager Core
- **Singleton**: `AudioManager` persistente em todas as cenas.
- **Buses (Mixer)**: Uso do `AudioMixer` da Unity para controle preciso de grupos.
- **Pool de Áudio**: Sistema de Object Pooling para SFX rápidos (tiros, cliques, notificações) para evitar overhead de memória.

## 2. Tipos de Áudio
- **MusicManager**: Suporte para Cross-fade entre faixas (transições suaves entre Hub e Combate).
- **AmbientManager**: Sons de fundo (clima, atmosfera da ilha).
- **SFXSystem**: Métodos estáticos/estilo evento para tocar sons 2D e 3D.

## 3. Data Integration
- **AudioEventSO**: ScriptableObjects que contêm o AudioClip, Volume, Pitch Randomness e Mixer Group. Isso permite que designers troquem sons sem mexer em código.

## 4. Implementação de Volume
- Vincular os Sliders do Plano 05 diretamente aos parâmetros do `AudioMixer`.
