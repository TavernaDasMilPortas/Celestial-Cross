using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Sirenix.OdinInspector;
using CelestialCross.Gacha;

namespace CelestialCross.Gacha.UI
{
    public class GachaAnimationController : MonoBehaviour
    {
        [Title("UI Elements")]
        public GameObject backgroundPanel;
        public Transform stickerSpawnArea; 
        public Transform stampsSpawnArea;
        [Tooltip("Objeto que será ativado DEPOIS do flash branco.")]
        public GameObject objectToActivateAfterFlash;
        public CanvasGroup whiteFlashPanel;

        [Title("Prefabs")]
        public GameObject starStickerPrefab;
        public GameObject uiLinePrefab;
        public GameObject prizeStampPrefab;

        [Title("Effects")]
        public ParticleSystem climaxParticles;
        public AudioClip sfxSlap;
        public AudioClip sfxSinoLight;
        public AudioClip sfxCrescendo;
        public AudioClip sfxClimax;
        public AudioClip sfxCarimbo;

        [Title("Buttons")]
        public Button btnContinue;
        public Button btnSkip;

        [Title("Animation Settings")]
        public enum LineDrawDirection { OldToNew, NewToOld }
        [Tooltip("Direção do preenchimento da linha nas constelações")]
        public LineDrawDirection lineDirection = LineDrawDirection.OldToNew;
        [Tooltip("Se ativo, as estrelas realizarão fade-in lentamente acompanhando a chegada da linha")]
        public bool fadeInStars = true;
        [Tooltip("Tempo total (em segundos) que a constelação inteira leva para ser desenhada")]
        public float totalConstellationTime = 2.0f;

        private AudioSource audioSource;
        private global::System.Action onSequenceFinished;
        private List<GameObject> activeStickers = new List<GameObject>();
        private List<GameObject> activeLines = new List<GameObject>();
        private List<GameObject> activeStamps = new List<GameObject>();
        private List<GachaRewardEntry> currentResults;
        private CelestialCross.Data.BannerPullVisualConfigSO activeBannerConfig;
        private bool isFinished = false;

        private Vector2[] constellationPositions = new Vector2[]
        {
            new Vector2(-300, 100), new Vector2(-150, 250), new Vector2(100, 200),
            new Vector2(300, 50), new Vector2(250, -150), new Vector2(100, -250),
            new Vector2(-150, -200), new Vector2(-250, -50), new Vector2(-100, 50),
            new Vector2(50, -50)
        };

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            if (btnContinue) btnContinue.onClick.AddListener(FinishAnimation);
            if (btnSkip) btnSkip.onClick.AddListener(SkipAnimation);
        }

        public void PlayGachaSequence(List<GachaRewardEntry> results, CelestialCross.Data.BannerPullVisualConfigSO bannerConfig, global::System.Action onFinished)
        {
            this.currentResults = results;
            this.activeBannerConfig = bannerConfig;
            this.onSequenceFinished = onFinished;
            this.isFinished = false;
            
            gameObject.SetActive(true);
            if (btnContinue) btnContinue.gameObject.SetActive(false);
            if (btnSkip) btnSkip.gameObject.SetActive(true);
            if (backgroundPanel) backgroundPanel.SetActive(true);
            if (whiteFlashPanel) { whiteFlashPanel.alpha = 0; whiteFlashPanel.gameObject.SetActive(false); }
            if (stickerSpawnArea) stickerSpawnArea.gameObject.SetActive(true);
            if (stampsSpawnArea) stampsSpawnArea.gameObject.SetActive(false);
            if (objectToActivateAfterFlash) objectToActivateAfterFlash.SetActive(false);
            
            ClearBoard();
            StartCoroutine(SequenceRoutine());
        }

        private IEnumerator SequenceRoutine()
        {
            int pullCount = currentResults.Count;
            GachaRewardEntry bestReward = currentResults.OrderByDescending(r => (int)r.Rarity).First();
            Color bestColor = GetRarityColor(bestReward.Rarity);

            // Phase 1: Spawn da Constelacao (Em branco)
            // Mapeando posições
            List<Vector2> spawnedPositions = new List<Vector2>();
            
            float stepDelay = totalConstellationTime / Mathf.Max(1, pullCount);

            for (int i = 0; i < pullCount; i++)
            {
                var reward = currentResults[i];
                Vector2 pos = Vector2.zero;

                if (pullCount > 1) 
                {
                    if (activeBannerConfig != null && activeBannerConfig.pullPositions.Count > i)
                    {
                        pos = activeBannerConfig.pullPositions[i].position;
                    }
                    else
                    {
                        pos = constellationPositions[i % constellationPositions.Length];
                    }
                }
                
                spawnedPositions.Add(pos);

                float waitTime = stepDelay;

                // Iniciar animação das linhas vindo das estrelas anteriores para esta
                if (pullCount > 1)
                {
                    float lineAnimDuration = waitTime > 0 ? waitTime : 0.15f;
                    
                    // Se o FadeInStars ligado, nascemos a Estrela junto com o começo da trajetória da linha (mas não a primeira, que não tem linha chegando)
                    if (fadeInStars && i > 0)
                    {
                        StartCoroutine(SpawnSticker(pos, Color.white, lineAnimDuration));
                    }
                    
                    if (activeBannerConfig != null && activeBannerConfig.connectionIndices != null && activeBannerConfig.connectionIndices.Length > 0)
                    {
                        // Checar se a estrela atual (i) conecta com alguma já spawnada (j < i)
                        for (int c = 0; c < activeBannerConfig.connectionIndices.Length; c += 2)
                        {
                            int idx1 = activeBannerConfig.connectionIndices[c];
                            int idx2 = activeBannerConfig.connectionIndices[c+1];
                            
                            if ((idx1 == i && idx2 < i) || (idx2 == i && idx1 < i))
                            {
                                int otherIdx = (idx1 == i) ? idx2 : idx1;
                                StartCoroutine(DrawLineAnimated(spawnedPositions[otherIdx], pos, lineAnimDuration));
                            }
                        }
                    }
                    else if (i > 0) 
                    {
                        // Fallback (liga em sequencia)
                        StartCoroutine(DrawLineAnimated(spawnedPositions[i-1], pos, lineAnimDuration));
                    }
                }

                if (waitTime > 0 && pullCount > 1 && i > 0)
                {
                    yield return new WaitForSeconds(waitTime);
                }
                
                // Dispara o SpawnSticker se o fadeIn estiver desligado (comportamento original ao fim do trajeto da linha),  
                // ou se for a primeiríssima estrela gerada, ou se for Single Pull
                if (!fadeInStars || pullCount == 1 || i == 0)
                {
                    yield return StartCoroutine(SpawnSticker(pos, Color.white, 0.15f));
                }
            }

            yield return new WaitForSeconds(0.4f);

            // Phase 2: Transição para a cor do Tier Máximo
            PlaySound(sfxClimax);
            if (climaxParticles != null) climaxParticles.Play();
            
            float colorTransitionTime = 0.5f;
            float tColor = 0;
            while (tColor < colorTransitionTime)
            {
                tColor += Time.deltaTime;
                float pct = tColor / colorTransitionTime;
                foreach (var st in activeStickers)
                {
                    if (st != null)
                    {
                        var img = st.GetComponent<Image>();
                        if (img) img.color = Color.Lerp(Color.white, bestColor, pct);
                    }
                }
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            // Phase 3: Piscam e revelam o tier de cada item
            PlaySound(sfxSinoLight);
            yield return StartCoroutine(PulseAllStickers(1.15f, 0.2f));
            
            PlaySound(sfxCrescendo);
            yield return StartCoroutine(PulseAllStickers(1.25f, 0.2f));

            yield return StartCoroutine(FinalRevealColors(1.5f, 0.5f));

            // Phase 4: Flash Branco
            if (whiteFlashPanel)
            {
                whiteFlashPanel.gameObject.SetActive(true);
                float t = 0;
                while (t < 0.2f) {
                    t += Time.deltaTime;
                    whiteFlashPanel.alpha = Mathf.Lerp(0, 1f, t / 0.2f);
                    yield return null;
                }
                whiteFlashPanel.alpha = 1f;
                
                // Ocultar Estrelas e Mostrar StampsArea
                if (stickerSpawnArea) stickerSpawnArea.gameObject.SetActive(false);
                if (stampsSpawnArea) stampsSpawnArea.gameObject.SetActive(true);
                
                t = 0;
                while (t < 0.3f) {
                    t += Time.deltaTime;
                    whiteFlashPanel.alpha = Mathf.Lerp(1f, 0f, t / 0.3f);
                    yield return null;
                }
                whiteFlashPanel.alpha = 0f;
                whiteFlashPanel.gameObject.SetActive(false);
            }
            else
            {
                if (stickerSpawnArea) stickerSpawnArea.gameObject.SetActive(false);
                if (stampsSpawnArea) stampsSpawnArea.gameObject.SetActive(true);
            }

            if (objectToActivateAfterFlash) objectToActivateAfterFlash.SetActive(true);

            // Phase 5: Spawn dos Selos/Stamps um por um
            var sortedResults = currentResults.OrderBy(r => (int)r.Rarity).ToList();
            for (int i = 0; i < sortedResults.Count; i++)
            {
                var reward = sortedResults[i];
                float delayAntesDeColar = (reward.Rarity >= GachaRarity.Epic) ? 0.6f : 0.1f;
                yield return new WaitForSeconds(delayAntesDeColar);
                
                yield return StartCoroutine(SpawnStamp(reward));
            }

            FinishSequenceVisuals();
        }

        private IEnumerator SpawnSticker(Vector2 pos, Color initColor, float fadeDuration = 0.15f)
        {
            var sticker = Instantiate(starStickerPrefab, stickerSpawnArea);
            var rect = sticker.GetComponent<RectTransform>();
            
            // Forçamos a âncora a ficar extamente no meio para que a posição do SO reflita puramente do centro da tela para fora (igual ao nosso Editor)
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            
            sticker.transform.localScale = Vector3.one * 2f;
            sticker.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
            
            var img = sticker.GetComponent<Image>();
            if (img != null) { Color c = initColor; c.a = 0; img.color = c; }
            
            // Aqui as estrelas soh pintam, nao recebem icon nem texto. Entao escondemos se estivem la
            var icon = sticker.transform.Find("Icon")?.gameObject;
            var txt = sticker.transform.Find("NameText")?.gameObject;
            if(icon) icon.SetActive(false);
            if(txt) txt.SetActive(false);

            activeStickers.Add(sticker);

            float t = 0;
            while(t < fadeDuration)
            {
                t += Time.deltaTime;
                float pct = t / fadeDuration;
                
                // Se a duracao for longa (como no uso junto da linha), evita o bounce enorme q daria no scale e mantem normal
                float scale = fadeDuration > 0.2f ? Mathf.Lerp(0f, 1f, pct) : (2f - Mathf.Sin(pct * Mathf.PI * 0.5f));
                sticker.transform.localScale = Vector3.one * scale;
                
                if (img != null) { Color c = initColor; c.a = pct; img.color = c; }
                yield return null;
            }
            sticker.transform.localScale = Vector3.one;
            PlaySound(sfxSlap);
        }

        private IEnumerator SpawnStamp(GachaRewardEntry reward)
        {
            var stamp = Instantiate(prizeStampPrefab, stampsSpawnArea);
            var rect = stamp.GetComponent<RectTransform>();
            
            // O componente Layout Group no objeto 'stampsSpawnArea' cuidará do positionamento,
            // então não usamos mais rect.anchoredPosition.
            
            stamp.transform.localScale = Vector3.one * 2f;
            stamp.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
            stamp.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
            
            activeStamps.Add(stamp);

            // Popula os dados do selo
            var iconImg = stamp.transform.Find("Icon")?.GetComponent<Image>();
            var txtName = stamp.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var bgImg = stamp.GetComponent<Image>();

            if (bgImg != null) bgImg.color = GetRarityColor(reward.Rarity);
            
            if (txtName != null) txtName.text = GetRewardName(reward);

            if (iconImg != null)
            {
                Sprite iSprite = GetRewardIcon(reward);
                if (iSprite != null) {
                    iconImg.sprite = iSprite;
                    iconImg.color = Color.white;
                } else {
                    iconImg.color = new Color(0,0,0,0);
                }
            }

            float t = 0;
            while(t < 0.15f)
            {
                t += Time.deltaTime;
                float pct = t / 0.15f;
                float scale = 2f - Mathf.Sin(pct * Mathf.PI * 0.5f);
                stamp.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            stamp.transform.localScale = Vector3.one;
            PlaySound(sfxCarimbo);
            
            if (reward.Rarity >= GachaRarity.Epic)
            {
                PlaySound(sfxSinoLight);
            }
        }

        private IEnumerator DrawLineAnimated(Vector2 start, Vector2 end, float duration)
        {
            if (uiLinePrefab == null) yield break;

            if (lineDirection == LineDrawDirection.NewToOld)
            {
                Vector2 temp = start;
                start = end;
                end = temp;
            }

            GameObject line = Instantiate(uiLinePrefab, stickerSpawnArea);
            line.transform.SetAsFirstSibling();
            
            RectTransform rect = line.GetComponent<RectTransform>();
            
            // Forçamos o Pivot para o canto esquerdo da linha (X=0), assim, 
            // quando a largura crescer via sizeDelta, ela esticará unicamente na direção do alvo
            rect.pivot = new Vector2(0f, 0.5f);
            
            rect.anchoredPosition = start;
            Vector2 dir = end - start;
            float finalMagnitude = dir.magnitude;
            
            rect.sizeDelta = new Vector2(0f, 4f);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rect.localRotation = Quaternion.Euler(0, 0, angle);
            
            // Cor/Material agora pertencem exclusivamente ao Prefab, comentado acesso forçado
            // var img = line.GetComponent<Image>();
            // if(img) img.color = color;
            
            activeLines.Add(line);

            if (duration <= 0) duration = 0.1f;
            
            float t = 0;
            while(t < duration)
            {
                t += Time.deltaTime;
                rect.sizeDelta = new Vector2(Mathf.Lerp(0, finalMagnitude, t / duration), 4f);
                yield return null;
            }
            rect.sizeDelta = new Vector2(finalMagnitude, 4f);
        }

        private IEnumerator PulseAllStickers(float maxScale, float duration)
        {
            float half = duration / 2f;
            for(float t=0; t<half; t+=Time.deltaTime) {
                float s = Mathf.Lerp(1f, maxScale, t/half);
                foreach(var st in activeStickers) if(st) st.transform.localScale = Vector3.one * s;
                yield return null;
            }
            for(float t=0; t<half; t+=Time.deltaTime) {
                float s = Mathf.Lerp(maxScale, 1f, t/half);
                foreach(var st in activeStickers) if(st) st.transform.localScale = Vector3.one * s;
                yield return null;
            }
            foreach(var st in activeStickers) if(st) st.transform.localScale = Vector3.one;
        }

        private IEnumerator FinalRevealColors(float maxScale, float duration)
        {
            float half = duration / 2f;
            for(float t=0; t<half; t+=Time.deltaTime) {
                float s = Mathf.Lerp(1f, maxScale, t/half);
                foreach(var st in activeStickers) if(st) st.transform.localScale = Vector3.one * s;
                yield return null;
            }

            for (int i = 0; i < activeStickers.Count; i++) {
                var st = activeStickers[i];
                var reward = currentResults[i];
                Color tierColor = GetRarityColor(reward.Rarity);
                var img = st.GetComponent<Image>();
                if(img) img.color = tierColor;
            }

            for(float t=0; t<half; t+=Time.deltaTime) {
                float s = Mathf.Lerp(maxScale, 1f, t/half);
                foreach(var st in activeStickers) if(st) st.transform.localScale = Vector3.one * s;
                yield return null;
            }
            foreach(var st in activeStickers) if(st) st.transform.localScale = Vector3.one;
        }

        public void SkipAnimation()
        {
            if (isFinished) return;
            StopAllCoroutines();
            ClearBoard();

            if (whiteFlashPanel) { whiteFlashPanel.alpha = 0f; whiteFlashPanel.gameObject.SetActive(false); }
            if (stickerSpawnArea) stickerSpawnArea.gameObject.SetActive(false);
            if (stampsSpawnArea) stampsSpawnArea.gameObject.SetActive(true);
            if (objectToActivateAfterFlash) objectToActivateAfterFlash.SetActive(true);

            var sortedResults = currentResults.OrderBy(r => (int)r.Rarity).ToList();
            for (int i = 0; i < sortedResults.Count; i++)
            {
                var reward = sortedResults[i];
                
                var stamp = Instantiate(prizeStampPrefab, stampsSpawnArea);
                var rect = stamp.GetComponent<RectTransform>();
                
                stamp.transform.localScale = Vector3.one;
                
                var img = stamp.GetComponent<Image>();
                if(img) img.color = GetRarityColor(reward.Rarity);

                var iconImg = stamp.transform.Find("Icon")?.GetComponent<Image>();
                var txtName = stamp.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                
                if (txtName != null) txtName.text = GetRewardName(reward);

                if (iconImg != null)
                {
                    Sprite iSprite = GetRewardIcon(reward);
                    if (iSprite != null) {
                        iconImg.sprite = iSprite;
                        iconImg.color = Color.white;
                    } else {
                        iconImg.color = new Color(0,0,0,0);
                    }
                }

                activeStamps.Add(stamp);
            }

            FinishSequenceVisuals();
        }

        private void FinishSequenceVisuals()
        {
            isFinished = true;
            if (btnSkip) btnSkip.gameObject.SetActive(false);
            if (btnContinue) btnContinue.gameObject.SetActive(true);
        }

        public void FinishAnimation()
        {
            ClearBoard();
            gameObject.SetActive(false);
            onSequenceFinished?.Invoke();
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
        }

        private Sprite GetRewardIcon(GachaRewardEntry r)
        {
            if (r.RewardType == GachaRewardType.Unit && r.UnitData != null) return r.UnitData.icon;
            if (r.RewardType == GachaRewardType.Pet && r.PetSpeciesData != null) return r.PetSpeciesData.Icon;
            if (r.RewardType == GachaRewardType.Artifact && r.ArtifactSet != null) {
                if (r.ArtifactSet.slotIcons != null && r.ArtifactSet.slotIcons.Count > 0)
                    return r.ArtifactSet.slotIcons[0].icon;
            }
            return null;
        }

        private string GetRewardName(GachaRewardEntry r)
        {
            if (r.RewardType == GachaRewardType.Unit) return r.UnitData != null ? r.UnitData.displayName : "???";
            if (r.RewardType == GachaRewardType.Pet) return r.PetSpeciesData != null ? r.PetSpeciesData.SpeciesName : "???";
            if (r.RewardType == GachaRewardType.Artifact && r.ArtifactSet != null) return r.ArtifactSet.setName;
            return r.Rarity.ToString();
        }

        private Color GetRarityColor(GachaRarity rarity)
        {
            switch(rarity) {
                case GachaRarity.Uncommon: return new Color(0.2f, 0.8f, 0.2f);
                case GachaRarity.Rare: return new Color(0.2f, 0.5f, 1f);
                case GachaRarity.Epic: return new Color(0.8f, 0.2f, 1f);
                case GachaRarity.Legendary: return new Color(1f, 0.8f, 0.2f);
                case GachaRarity.Supreme: return new Color(1f, 0.2f, 0.2f);
                default: return Color.white;
            }
        }

        private void ClearBoard() { 
            foreach(var s in activeStickers) if(s) Destroy(s); 
            activeStickers.Clear(); 
            foreach(var s in activeLines) if(s) Destroy(s); 
            activeLines.Clear(); 
            foreach(var s in activeStamps) if(s) Destroy(s);
            activeStamps.Clear();
        }
    }
}