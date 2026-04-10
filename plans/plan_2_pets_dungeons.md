# Plano 2: Sistema de Masmorras para Pets, Status Aleat�rios e Poeira Estelar

## 1. Vis�o Geral
Este fluxo introduz os Pets no ecossistema do jogo. Diferentes dos personagens fixos, os pets caem de masmorras com a mesma aleatoriedade de atributos dos artefatos. A progress�o deles � governada por uma fogueira de sacrif�cios (venda de duplicatas) em troca do recurso crucial: **Poeira Estelar**.

## 2. Modelagem de Dados (ScriptableObjects)
### 2.1. O Pet Base (PetSpeciesSO)
O que diferencia um tipo de pet de outro � a sua esp�cie base.
*   **Identidade e Skill:** Cont�m o Nome, Prefab3D ou �cone 2D e a Refer�ncia para a Skill �nica que o pet prov� ao grupo/personagem equipado.
*   **Status Ranges (Min-Max):** Um intervalo matem�tico definindo os limites dos status base (Ex: Vida [100 a 150], Defesa [10 a 25]). Esses limites funcionam como teto/piso na gera��o RNG e s�o destravados pelas estrelas.

### 2.2. O Cat�logo de Masmorra (PetDungeonLootSO)
*   **Habitat:** Lista de PetSpeciesSO que podem ser capturados/dropados nesta fase espec�fica.
*   **Chance de Drop (Estrelas):** Pesos configur�veis que ditam as chances de o Pet vir com 1 a 5 estrelas. Masmorras mais dif�ceis removem a chance de dropar pets de baixa estrela.

## 3. L�gica de Gera��o em Runtime 
Ao completar a masmorra e sortear um pet:
1.  **Sele��o da Esp�cie:** O sistema pesca uma das esp�cies cadastradas no PetDungeonLootSO.
2.  **Sorteio de Estrelas:** Rola a % para determinar com quantas estrelas esse indiv�duo vir�.
3.  **Forma��o dos Status:** Baseado nos Status Ranges da esp�cie e fortalecido/multiplicado pelas estrelas sorteadas, os valores finais de Vida/Ataque/Etc do *indiv�duo* s�o decididos atrav�s de m�todos como Random.Range().
4.  **Runtime Instantiation:** Cria-se o RuntimePetData com propriedades salv�veis, garantindo um ID �nico (UUID) para permitir ter 10 vers�es da mesma esp�cie, cada uma com atributos rand�micos num mesmo invent�rio.

## 4. O Sistema de Descarte e a Poeira Estelar
*   **Novo Recurso (Poeira Estelar):** Moeda restrita ao Hub de Pets.
*   **Descarte Funcional:** Jogadores vendem/libertam pets n�o-desejados (geralmente os mal-rollados ou sobressalentes) para acumular Poeira Estelar.
*   **Custo x Benef�cio:** A convers�o de Pet -> Poeira estelar escala absurdamente com o n�mero de estrelas e n�vel do pet sacrificado.

## 5. Progress�o: Upgrade de Pets
No painel do Pet:
*   **Level Up:** O jogador investe a Poeira Estelar acumulada na conta para subir o n�vel do Pet alvo.
*   **Impacto no Gameplay:** Subir o n�vel aumenta os status principais roletados do pet (potencializando os "bons rolls") e talvez destravar upgrades passivos na sua Skill Exclusiva.

---

## 6. Passos para Implementa��o (Roadmap T�cnico)
1.  **Classes de Dados do Pet:** Codificar PetSpeciesSO para os limites de status Min-Max e associ�-lo a uma l�gica vazia de Skills.
2.  **Gerador Procedural:** Desenvolver a f�brica (Factory) que gera o RuntimePetData com stats �nicos e UUID baseados na esp�cie e nas estrelas recebidas do drop.
3.  **Moedas do SaveData:** Adicionar inteiros/floats de StardustAmount ao perfil global do jogador.
4.  **UI de Invent�rio Clone:** Modificar o visualizador de caixa (Inventory) para agrupar e listar N vers�es da mesma esp�cie de pet, destacando os Rolls de atributos em UI.
5.  **L�gica Trash-to-Dust:** Um modo de gerenciamento no qual o player marca m�ltiplos pets e aperta "Soltar", somando e creditando a Poeira Estelar correspondente de todos os UUIDs sumariados.
6.  **Sistema de N�vel:** L�gica matem�tica de absor��o de Poeira para encarecer/aumentar o Level de um RuntimePetData.
