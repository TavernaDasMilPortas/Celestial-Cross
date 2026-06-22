using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Sirenix.OdinInspector;
using CelestialCross.Gacha;
using CelestialCross.Audio;
using DG.Tweening;

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

        [Title("Supreme Reveal")]
        [Tooltip("Container pai de todo o reveal. Deve ter CanvasGroup.")]
        public RectTransform supremeRevealContainer;
        [Tooltip("Image que exibe a silhueta (usa Material UI/Silhouette)")]
        public Image supremeSilhouetteImage;
        [Tooltip("Image que exibe a splash art real (Material default)")]
        public Image supremeSplashImage;
        [Tooltip("Texto com o nome do personagem Supreme")]
        public TextMeshProUGUI supremeNameText;
        [Tooltip("Material com o shader UI/Silhouette para geração automática de silhueta")]
        public Material silhouetteMaterial;

        [Title("Reward Modals")]
        public GameObject rewardDetailModal; // Fallback
        public Image modalIcon;
        public TextMeshProUGUI modalName;
        public TextMeshProUGUI modalRarityText;
        public TextMeshProUGUI modalDesc;
        public Button modalCloseButton;

        [Title("Complex Modals")]
        public CelestialCross.Scenes.Unit.ArtifactActionModal artifactActionModal;
        public CelestialCross.Giulia_UI.ArtifactUpgradeModal artifactUpgradeModal;
        public GachaUnitRewardModal gachaUnitRewardModal;

        [Title("Prefabs")]
        public GameObject starStickerPrefab;
        public GameObject uiLinePrefab;
        public GameObject prizeStampPrefab;

        [Title("Effects")]
        public ParticleSystem climaxParticles;
        public SoundKey sfxSlap = SoundKey.GachaSlap;
        public SoundKey sfxSinoLight = SoundKey.GachaBell;
        public SoundKey sfxCrescendo = SoundKey.GachaCrescendo;
        public SoundKey sfxClimax = SoundKey.GachaClimax;
        public SoundKey sfxCarimbo = SoundKey.GachaStamp;

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

        [Title("Animation Settings — DOTween")]
        public float backgroundFadeInDuration = 0.5f;
        public float stampShakeIntensity = 5f;
        public float epicStampShakeIntensity = 12f;

        // private AudioSource audioSource; // Removido: Usando AudioManager central
        private global::System.Action onSequenceFinished;
        private List<GameObject> activeStickers = new List<GameObject>();
        private List<GameObject> activeLines = new List<GameObject>();
        private List<GameObject> activeStamps = new List<GameObject>();
        private List<RuntimeGachaResult> currentResults;
        private CelestialCross.Data.BannerPullVisualConfigSO activeBannerConfig;
        private GachaBannerSO activeBanner; 
        private bool isFinished = false;
        private Sequence _masterSequence;
        private GameObject currentSelectedStamp;
        private Tween currentStampTween;
        private bool sequenceFinished = false;

        private Vector2[] constellationPositions = new Vector2[]
        {
            new Vector2(-300, 100), new Vector2(-150, 250), new Vector2(100, 200),
            new Vector2(300, 50), new Vector2(250, -150), new Vector2(100, -250),
            new Vector2(-150, -200), new Vector2(-250, -50), new Vector2(-100, 50),
            new Vector2(50, -50)
        };

        void Awake()
        {
            DOTween.Init(recycleAllByDefault: true);
            
            // audioSource = GetComponent<AudioSource>(); // Usando AudioManager central

            if (btnContinue) btnContinue.onClick.AddListener(FinishAnimation);
            if (btnSkip) btnSkip.onClick.AddListener(SkipAnimation);
        }

        private void OnDestroy()
        {
            _masterSequence?.Kill();
        }

        public void PlayGachaSequence(List<RuntimeGachaResult> results, CelestialCross.Data.BannerPullVisualConfigSO bannerConfig, global::System.Action onFinished)
        {
            this.currentResults = results;
            this.activeBannerConfig = bannerConfig;
            this.onSequenceFinished = onFinished;
            this.isFinished = false;
            this.sequenceFinished = false;
            
            ShopSceneUI shopUI = FindObjectOfType<ShopSceneUI>();
            if (shopUI != null && shopUI.availableBanners != null && shopUI.availableBanners.Count > 0)
            {
                this.activeBanner = shopUI.availableBanners.Find(b => b.pullVisualConfig == bannerConfig);
                if (this.activeBanner == null) this.activeBanner = shopUI.availableBanners[0];
            }
            
            gameObject.SetActive(true);
            if (btnContinue) btnContinue.gameObject.SetActive(false);
            if (btnSkip) btnSkip.gameObject.SetActive(true);
            
            if (backgroundPanel) 
            {
                backgroundPanel.SetActive(true);
                var cg = backgroundPanel.GetComponent<CanvasGroup>();
                if (!cg) cg = backgroundPanel.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
            }
            
            if (whiteFlashPanel) { whiteFlashPanel.alpha = 0; whiteFlashPanel.gameObject.SetActive(false); }
            if (stickerSpawnArea) stickerSpawnArea.gameObject.SetActive(true);
            if (stampsSpawnArea) stampsSpawnArea.gameObject.SetActive(true); 
            if (objectToActivateAfterFlash) objectToActivateAfterFlash.SetActive(false);
            
            if (supremeRevealContainer) 
            {
                supremeRevealContainer.gameObject.SetActive(false);
                var cg = supremeRevealContainer.GetComponent<CanvasGroup>();
                if (cg) cg.alpha = 0f;
            }

            ClearBoard();
            PlayFullSequenceDOTween();
        }

        private void PlayFullSequenceDOTween()
        {
            _masterSequence?.Kill();
            _masterSequence = DOTween.Sequence()
                .SetRecyclable(true)
                .SetAutoKill(true);

            BuildPhase0_BackgroundFadeIn(_masterSequence);
            BuildPhase1_Constellation(_masterSequence);
            
            bool hasSupreme = currentResults.Any(r => r.Entry.Rarity == GachaRarity.Supreme);
            
            if (hasSupreme)
            {
                BuildPhase4_5_SupremeReveal(_masterSequence);
            }
            else
            {
                BuildPhase2_ColorTransition(_masterSequence);
                BuildPhase3_PulseReveal(_masterSequence);
                BuildPhase4_Flash(_masterSequence);
            }

            BuildPhase5_StampSlap(_masterSequence);
            
            _masterSequence.AppendCallback(() => {
                sequenceFinished = true;
                FinishSequenceVisuals();
            });
        }

        private void BuildPhase0_BackgroundFadeIn(Sequence seq)
        {
            if (!backgroundPanel) return;
            
            var cg = backgroundPanel.GetComponent<CanvasGroup>();
            var bgTransform = backgroundPanel.transform;
            
            bgTransform.localScale = Vector3.one * 1.05f;
            
            seq.Append(cg.DOFade(1f, backgroundFadeInDuration).SetEase(Ease.OutCubic));
            seq.Join(bgTransform.DOScale(1f, backgroundFadeInDuration).SetEase(Ease.OutCubic));
        }

        private void BuildPhase1_Constellation(Sequence seq)
        {
            int pullCount = currentResults.Count;
            if (pullCount == 0) return;

            List<Vector2> spawnedPositions = new List<Vector2>();
            float stepDelay = totalConstellationTime / Mathf.Max(1, pullCount);

            for (int i = 0; i < pullCount; i++)
            {
                var result = currentResults[i];
                var reward = result.Entry;
                Vector2 pos = Vector2.zero;

                if (pullCount > 1) 
                {
                    if (activeBannerConfig != null && activeBannerConfig.pullPositions.Count > i)
                        pos = activeBannerConfig.pullPositions[i].position;
                    else
                        pos = constellationPositions[i % constellationPositions.Length];
                }
                
                spawnedPositions.Add(pos);
                float waitTime = stepDelay;

                if (pullCount > 1)
                {
                    float lineAnimDuration = waitTime > 0 ? waitTime : 0.15f;
                    
                    if (fadeInStars && i > 0)
                    {
                        seq.Join(SpawnStickerSeq(pos, Color.white, lineAnimDuration));
                    }
                    
                    if (activeBannerConfig != null && activeBannerConfig.connectionIndices != null && activeBannerConfig.connectionIndices.Length > 0)
                    {
                        for (int c = 0; c < activeBannerConfig.connectionIndices.Length; c += 2)
                        {
                            int idx1 = activeBannerConfig.connectionIndices[c];
                            int idx2 = activeBannerConfig.connectionIndices[c+1];
                            
                            if ((idx1 == i && idx2 < i) || (idx2 == i && idx1 < i))
                            {
                                int otherIdx = (idx1 == i) ? idx2 : idx1;
                                seq.Join(DrawLineAnimatedSeq(spawnedPositions[otherIdx], pos, lineAnimDuration));
                            }
                        }
                    }
                    else if (i > 0) 
                    {
                        seq.Join(DrawLineAnimatedSeq(spawnedPositions[i-1], pos, lineAnimDuration));
                    }
                }

                if (waitTime > 0 && pullCount > 1 && i > 0)
                {
                    seq.AppendInterval(waitTime);
                }
                
                if (!fadeInStars || pullCount == 1 || i == 0)
                {
                    seq.Append(SpawnStickerSeq(pos, Color.white, 0.15f));
                }
            }
            seq.AppendInterval(0.4f);
        }

        private void BuildPhase2_ColorTransition(Sequence seq)
        {
            GachaRewardEntry bestReward = currentResults.OrderByDescending(r => (int)r.Entry.Rarity).First().Entry;
            Color bestColor = GetRarityColor(bestReward.Rarity);

            seq.AppendCallback(() => {
                PlaySound(sfxClimax);
                if (climaxParticles != null) climaxParticles.Play();
            });

            float colorTransitionTime = 0.5f;
            foreach (var st in activeStickers)
            {
                if (st != null)
                {
                    var img = st.GetComponent<Image>();
                    if (img) seq.Join(img.DOColor(bestColor, colorTransitionTime).SetEase(Ease.InOutSine));
                }
            }
            seq.AppendInterval(0.5f);
        }

        private void BuildPhase3_PulseReveal(Sequence seq)
        {
            seq.AppendCallback(() => PlaySound(sfxSinoLight));
            seq.Append(PulseTween(1.15f, 0.2f));
            
            seq.AppendCallback(() => PlaySound(sfxCrescendo));
            seq.Append(PulseTween(1.25f, 0.2f));

            seq.Append(PulseTween(1.5f, 0.5f));
            seq.InsertCallback(seq.Duration() - 0.25f, () => {
                for (int i = 0; i < activeStickers.Count; i++) {
                    var st = activeStickers[i];
                    var result = currentResults[i];
                    var reward = result.Entry;
                    var img = st.GetComponent<Image>();
                    if(img) img.color = GetRarityColor(reward.Rarity);
                }
            });
        }

        private void BuildPhase4_Flash(Sequence seq)
        {
            if (whiteFlashPanel)
            {
                seq.AppendCallback(() => whiteFlashPanel.gameObject.SetActive(true));
                seq.Append(whiteFlashPanel.DOFade(1f, 0.2f).SetEase(Ease.OutQuart));
                seq.AppendCallback(() => {
                    if (stickerSpawnArea) stickerSpawnArea.gameObject.SetActive(false);
                    if (stampsSpawnArea) stampsSpawnArea.gameObject.SetActive(true);
                    
                    foreach(var st in activeStickers) if(st) st.SetActive(false);
                    foreach(var ln in activeLines) if(ln) ln.SetActive(false);
                    
                    if (objectToActivateAfterFlash) objectToActivateAfterFlash.SetActive(true);
                });
                seq.Append(whiteFlashPanel.DOFade(0f, 0.3f).SetEase(Ease.InOutQuad));
                seq.AppendCallback(() => whiteFlashPanel.gameObject.SetActive(false));
            }
            else
            {
                seq.AppendCallback(() => {
                    if (stickerSpawnArea) stickerSpawnArea.gameObject.SetActive(false);
                    if (stampsSpawnArea) stampsSpawnArea.gameObject.SetActive(true);
                    
                    foreach(var st in activeStickers) if(st) st.SetActive(false);
                    foreach(var ln in activeLines) if(ln) ln.SetActive(false);
                    
                    if (objectToActivateAfterFlash) objectToActivateAfterFlash.SetActive(true);
                });
            }
        }

        private void BuildPhase4_5_SupremeReveal(Sequence seq)
        {
            if (!supremeRevealContainer || !supremeSilhouetteImage || !supremeSplashImage || !supremeNameText) return;

            var supremeReward = currentResults.First(r => r.Entry.Rarity == GachaRarity.Supreme).Entry;
            Sprite revealSprite = GetSupremeRevealSprite(supremeReward);
            Sprite silhouetteSprite = GetSilhouetteSprite(activeBanner, supremeReward, revealSprite);

            var cg = supremeRevealContainer.GetComponent<CanvasGroup>();

            if (backgroundPanel)
            {
                var bgImg = backgroundPanel.GetComponent<Image>();
                if (bgImg) seq.Append(bgImg.DOColor(Color.black, 0.3f));
            }
            if (stampsSpawnArea && stampsSpawnArea.gameObject.activeSelf)
            {
                var stampCg = stampsSpawnArea.GetComponent<CanvasGroup>();
                if (!stampCg) stampCg = stampsSpawnArea.gameObject.AddComponent<CanvasGroup>();
                seq.Join(stampCg.DOFade(0f, 0.3f));
            }
            if (stickerSpawnArea && stickerSpawnArea.gameObject.activeSelf)
            {
                var stickerCg = stickerSpawnArea.GetComponent<CanvasGroup>();
                if (!stickerCg) stickerCg = stickerSpawnArea.gameObject.AddComponent<CanvasGroup>();
                seq.Join(stickerCg.DOFade(0f, 0.3f));
            }

            seq.AppendCallback(() => {
                supremeRevealContainer.gameObject.SetActive(true);
                cg.alpha = 0f;
                supremeSplashImage.gameObject.SetActive(false);
                
                supremeSilhouetteImage.gameObject.SetActive(true);
                supremeSilhouetteImage.sprite = silhouetteSprite;
                
                if (activeBanner != null && activeBanner.Silhouette != null) {
                    supremeSilhouetteImage.material = null; 
                    // A cor da imagem agora usa exatamente o que você configurou no Inspector da Unity!
                } else if (silhouetteMaterial != null) {
                    supremeSilhouetteImage.material = silhouetteMaterial;
                    supremeSilhouetteImage.color = Color.white;
                    silhouetteMaterial.SetFloat("_RevealProgress", 0f);
                    silhouetteMaterial.SetFloat("_EdgeGlow", 0f);
                }

                supremeNameText.gameObject.SetActive(false);
                
                supremeSilhouetteImage.rectTransform.localScale = Vector3.one * 3.0f;
                supremeSilhouetteImage.rectTransform.localRotation = Quaternion.identity;
            });

            seq.Append(cg.DOFade(1f, 0.4f).SetEase(Ease.OutExpo));
            seq.Join(supremeSilhouetteImage.rectTransform.DOScale(1.2f, 0.4f).SetEase(Ease.OutExpo));
            seq.Join(supremeSilhouetteImage.rectTransform.DORotate(new Vector3(0, 0, Random.Range(-3f, 3f)), 0.4f));

            seq.AppendCallback(() => PlaySound(sfxCrescendo));
            seq.Append(supremeSilhouetteImage.rectTransform.DOScale(1.15f, 0.5f).SetEase(Ease.InOutSine));
            if (silhouetteMaterial != null && (activeBanner == null || activeBanner.Silhouette == null)) {
                seq.Join(DOTween.To(() => silhouetteMaterial.GetFloat("_EdgeGlow"), x => silhouetteMaterial.SetFloat("_EdgeGlow", x), 1.5f, 0.5f));
            }
            seq.Append(supremeSilhouetteImage.rectTransform.DOScale(1.2f, 0.5f).SetEase(Ease.InOutSine));
            if (silhouetteMaterial != null && (activeBanner == null || activeBanner.Silhouette == null)) {
                seq.Join(DOTween.To(() => silhouetteMaterial.GetFloat("_EdgeGlow"), x => silhouetteMaterial.SetFloat("_EdgeGlow", x), 0.5f, 0.5f));
            }

            seq.AppendCallback(() => PlaySound(sfxClimax));
            seq.Append(supremeSilhouetteImage.rectTransform.DOScale(1.3f, 0.3f).SetEase(Ease.InQuad));
            if (silhouetteMaterial != null && (activeBanner == null || activeBanner.Silhouette == null)) {
                seq.Join(DOTween.To(() => silhouetteMaterial.GetFloat("_EdgeGlow"), x => silhouetteMaterial.SetFloat("_EdgeGlow", x), 3.0f, 0.3f));
                seq.Join(silhouetteMaterial.DOColor(Color.white, "_EdgeGlowColor", 0.3f));
            }

            if (whiteFlashPanel)
            {
                seq.AppendCallback(() => {
                    whiteFlashPanel.gameObject.SetActive(true);
                    whiteFlashPanel.GetComponent<Image>().color = new Color(1f, 0.9f, 0.6f, 1f); 
                });
                seq.Append(whiteFlashPanel.DOFade(1f, 0.2f).SetEase(Ease.OutQuart));
                
                seq.AppendCallback(() => {
                    supremeSilhouetteImage.gameObject.SetActive(false);
                    
                    supremeSplashImage.gameObject.SetActive(true);
                    supremeSplashImage.sprite = revealSprite;
                    supremeSplashImage.material = null;
                    
                    var splImgColor = supremeSplashImage.color;
                    splImgColor.a = 1f;
                    supremeSplashImage.color = splImgColor;
                    
                    supremeSplashImage.rectTransform.localScale = Vector3.one * 1.1f;
                    supremeSplashImage.rectTransform.localRotation = supremeSilhouetteImage.rectTransform.localRotation;
                });
            }

            if (whiteFlashPanel)
            {
                seq.Append(whiteFlashPanel.DOFade(0f, 0.6f).SetEase(Ease.InOutQuad));
                seq.AppendCallback(() => {
                    whiteFlashPanel.gameObject.SetActive(false);
                    whiteFlashPanel.GetComponent<Image>().color = Color.white; 
                });
            }
            seq.Join(supremeSplashImage.rectTransform.DOScale(1.0f, 0.6f).SetEase(Ease.OutBack));
            
            if (backgroundPanel)
            {
                var bgImg = backgroundPanel.GetComponent<Image>();
                if (bgImg) seq.Join(bgImg.DOColor(Color.white, 0.6f)); 
            }

            seq.AppendCallback(() => {
                supremeNameText.gameObject.SetActive(true);
                supremeNameText.text = GetRewardName(supremeReward);
                supremeNameText.color = GetRarityColor(GachaRarity.Supreme);
                
                var txtCg = supremeNameText.GetComponent<CanvasGroup>();
                if (!txtCg) txtCg = supremeNameText.gameObject.AddComponent<CanvasGroup>();
                txtCg.alpha = 0f;
                
                supremeNameText.rectTransform.anchoredPosition = new Vector2(100f, supremeNameText.rectTransform.anchoredPosition.y);
                supremeNameText.rectTransform.localRotation = Quaternion.Euler(0, 0, -2f);
                
                txtCg.DOFade(1f, 0.4f);
                supremeNameText.rectTransform.DOAnchorPosX(0f, 0.4f).SetEase(Ease.OutCubic);
            });
            seq.AppendInterval(0.4f);

            seq.AppendInterval(0.8f);
            seq.Append(cg.DOFade(0f, 0.3f));
            seq.AppendCallback(() => {
                supremeRevealContainer.gameObject.SetActive(false);
                
                if (stickerSpawnArea) stickerSpawnArea.gameObject.SetActive(false);
                
                foreach(var st in activeStickers) if(st) st.SetActive(false);
                foreach(var ln in activeLines) if(ln) ln.SetActive(false);
                
                if (stampsSpawnArea)
                {
                    stampsSpawnArea.gameObject.SetActive(true);
                    var stampCg = stampsSpawnArea.GetComponent<CanvasGroup>();
                    if (stampCg) stampCg.alpha = 1f; 
                }
                
                if (objectToActivateAfterFlash) objectToActivateAfterFlash.SetActive(true);
            });
        }

        private void BuildPhase5_StampSlap(Sequence seq)
        {
            var sortedResults = currentResults.OrderBy(r => (int)r.Entry.Rarity).ToList();
            for (int i = 0; i < sortedResults.Count; i++)
            {
                var reward = sortedResults[i];
                
                if (reward.Entry.Rarity >= GachaRarity.Epic)
                {
                    seq.AppendInterval(0.4f); 
                    if (stampsSpawnArea) seq.Append(((RectTransform)stampsSpawnArea).DOShakeAnchorPos(0.15f, 3f, 5));
                    seq.AppendInterval(0.2f);
                }
                else
                {
                    seq.AppendInterval(0.1f);
                }
                
                seq.AppendCallback(() => {
                    var stampSeq = SpawnStampSeq(reward);
                    stampSeq.Play();
                });
                
                seq.AppendInterval(0.2f); 
            }
        }

        private Sequence PulseTween(float maxScale, float duration)
        {
            Sequence s = DOTween.Sequence();
            float half = duration / 2f;
            
            foreach (var st in activeStickers) {
                if (st) {
                    s.Join(st.transform.DOScale(maxScale, half).SetEase(Ease.InOutSine));
                }
            }
            s.AppendInterval(0f); 
            foreach (var st in activeStickers) {
                if (st) {
                    s.Join(st.transform.DOScale(1f, half).SetEase(Ease.InOutSine));
                }
            }
            return s;
        }

        private Tween SpawnStickerSeq(Vector2 pos, Color initColor, float fadeDuration = 0.15f)
        {
            var targetArea = stickerSpawnArea;
            var sticker = Instantiate(starStickerPrefab, targetArea);
            var rect = sticker.GetComponent<RectTransform>();
            
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            
            sticker.transform.localScale = Vector3.one * 2f;
            sticker.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
            
            var img = sticker.GetComponent<Image>();
            if (img != null) { Color c = initColor; c.a = 0; img.color = c; }
            
            var icon = sticker.transform.Find("Icon")?.gameObject;
            var txt = sticker.transform.Find("NameText")?.gameObject;
            if(icon) icon.SetActive(false);
            if(txt) txt.SetActive(false);

            activeStickers.Add(sticker);

            Sequence s = DOTween.Sequence().SetRecyclable(true);
            if (fadeDuration > 0.2f) {
                s.Append(sticker.transform.DOScale(1f, fadeDuration).From(0f));
            } else {
                s.Append(sticker.transform.DOScale(1f, fadeDuration).From(2f).SetEase(Ease.OutBack));
            }
            if (img != null) s.Join(img.DOFade(1f, fadeDuration).From(0f));
            s.AppendCallback(() => PlaySound(sfxSlap));
            
            return s;
        }

        private Tween DrawLineAnimatedSeq(Vector2 start, Vector2 end, float duration)
        {
            if (uiLinePrefab == null) return DOTween.Sequence();

            if (lineDirection == LineDrawDirection.NewToOld)
            {
                Vector2 temp = start;
                start = end;
                end = temp;
            }

            var targetArea = stickerSpawnArea;
            GameObject line = Instantiate(uiLinePrefab, targetArea);
            line.transform.SetAsFirstSibling();
            
            RectTransform rect = line.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = start;
            Vector2 dir = end - start;
            float finalMagnitude = dir.magnitude;
            
            rect.sizeDelta = new Vector2(0f, 4f);
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rect.localRotation = Quaternion.Euler(0, 0, angle);
            
            activeLines.Add(line);

            if (duration <= 0) duration = 0.1f;
            
            return rect.DOSizeDelta(new Vector2(finalMagnitude, 4f), duration)
                .From(new Vector2(0f, 4f))
                .SetEase(Ease.OutQuad)
                .SetRecyclable(true);
        }

        private Sequence SpawnStampSeq(RuntimeGachaResult reward)
        {
            var stamp = Instantiate(prizeStampPrefab, stampsSpawnArea);
            var rect = stamp.GetComponent<RectTransform>();
            
            stamp.transform.localScale = Vector3.one * 2.5f;
            stamp.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-5f, 5f));
            
            activeStamps.Add(stamp);

            var iconImg = stamp.transform.Find("Icon")?.GetComponent<Image>();
            var txtName = stamp.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var bgImg = stamp.GetComponent<Image>();

            if (bgImg != null) bgImg.color = GetRarityColor(reward.Entry.Rarity);
            if (txtName != null) txtName.text = GetRewardName(reward.Entry);

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

            float intensity = reward.Entry.Rarity >= GachaRarity.Epic ? epicStampShakeIntensity : stampShakeIntensity;

            Sequence s = DOTween.Sequence().SetRecyclable(true).SetAutoKill(true);
            s.Append(stamp.transform.DOScale(1f, 0.15f).From(2.5f).SetEase(Ease.OutBack));
            s.Append(stamp.transform.DOPunchScale(new Vector3(0.1f, -0.15f, 0), 0.12f, 1, 0));
            if (stampsSpawnArea) s.Join(((RectTransform)stampsSpawnArea).DOShakeAnchorPos(0.1f, intensity, 10, 90, false, true, ShakeRandomnessMode.Harmonic));
            s.InsertCallback(0.15f, () => {
                PlaySound(sfxCarimbo);
                if (reward.Entry.Rarity >= GachaRarity.Epic) PlaySound(sfxSinoLight);
            });
            
            var btn = stamp.GetComponent<Button>();
            if (!btn) btn = stamp.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => {
                if (sequenceFinished) OpenRewardModal(reward, stamp);
            });

            return s;
        }

        public void OpenRewardModal(RuntimeGachaResult result, GameObject stamp)
        {
            if (currentStampTween != null)
            {
                currentStampTween.Kill();
                if (currentSelectedStamp) currentSelectedStamp.transform.localScale = Vector3.one;
            }

            currentSelectedStamp = stamp;
            currentStampTween = stamp.transform.DOScale(1.1f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

            if (result.Entry.RewardType == GachaRewardType.Artifact && result.GeneratedInstance is CelestialCross.Artifacts.ArtifactInstanceData artifactData)
            {
                if (artifactActionModal != null)
                {
                    artifactActionModal.ShowFromGacha(artifactData, result.Entry.ArtifactSet, (wasSold) => {
                        // Ao fechar (vendido ou não)
                        if (wasSold && currentSelectedStamp != null)
                        {
                            var btn = currentSelectedStamp.GetComponent<Button>();
                            if (btn) btn.interactable = false;

                            var bgImg = currentSelectedStamp.GetComponent<Image>();
                            if (bgImg) bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                            
                            var iconImg = currentSelectedStamp.transform.Find("Icon")?.GetComponent<Image>();
                            if (iconImg) iconImg.color = new Color(1f, 1f, 1f, 0.3f);
                        }
                        CloseRewardModal();
                    });
                    return; // Retorna para não abrir o fallback
                }
            }
            else if (result.Entry.RewardType == GachaRewardType.Unit && result.GeneratedInstance is CelestialCross.Data.RuntimeUnitData unitData)
            {
                if (gachaUnitRewardModal != null)
                {
                    gachaUnitRewardModal.ShowUnit(unitData, result.Entry);
                    return;
                }
            }
            else if (result.Entry.RewardType == GachaRewardType.Pet && result.GeneratedInstance is CelestialCross.Data.Pets.RuntimePetData petData)
            {
                if (gachaUnitRewardModal != null)
                {
                    gachaUnitRewardModal.ShowPet(petData, result.Entry);
                    return;
                }
            }

            // Fallback (caso os complexos sejam nulos)
            if (rewardDetailModal == null) return;

            if (modalIcon) 
            {
                modalIcon.sprite = GetRewardIcon(result);
                modalIcon.color = modalIcon.sprite != null ? Color.white : new Color(0,0,0,0);
            }
            if (modalName) modalName.text = GetRewardName(result.Entry);
            if (modalRarityText) 
            {
                modalRarityText.text = result.Entry.Rarity.ToString();
                modalRarityText.color = GetRarityColor(result.Entry.Rarity);
            }
            if (modalDesc) modalDesc.text = "Tipo: " + result.Entry.RewardType.ToString();

            rewardDetailModal.SetActive(true);
            
            if (modalCloseButton)
            {
                modalCloseButton.onClick.RemoveAllListeners();
                modalCloseButton.onClick.AddListener(CloseRewardModal);
            }
        }

        public void CloseRewardModal()
        {
            if (currentStampTween != null)
            {
                currentStampTween.Kill();
                if (currentSelectedStamp) currentSelectedStamp.transform.DOScale(1f, 0.2f);
                currentStampTween = null;
                currentSelectedStamp = null;
            }
            
            if (rewardDetailModal) rewardDetailModal.SetActive(false);
            if (artifactActionModal != null) artifactActionModal.Close();
            if (artifactUpgradeModal != null) artifactUpgradeModal.Close();
            if (gachaUnitRewardModal != null) gachaUnitRewardModal.Close();
        }

        private Sprite GetSupremeRevealSprite(GachaRewardEntry supremeReward)
        {
            if (supremeReward.RewardType == GachaRewardType.Unit && supremeReward.UnitData != null)
            {
                if (supremeReward.UnitData.sprite != null) return supremeReward.UnitData.sprite;
                if (supremeReward.UnitData.icon != null) return supremeReward.UnitData.icon;
            }
            if (supremeReward.RewardType == GachaRewardType.Pet && supremeReward.PetSpeciesData != null)
            {
                return supremeReward.PetSpeciesData.Icon;
            }
            if (activeBanner != null) return activeBanner.BannerSplashArt;
            return null;
        }

        private Sprite GetSilhouetteSprite(GachaBannerSO banner, GachaRewardEntry supremeReward, Sprite revealSprite)
        {
            if (banner != null && banner.Silhouette != null) return banner.Silhouette;
            return revealSprite;
        }

        public void SkipAnimation()
        {
            if (isFinished) return;
            
            _masterSequence?.Kill();
            
            ClearBoard();

            if (whiteFlashPanel) { whiteFlashPanel.alpha = 0f; whiteFlashPanel.gameObject.SetActive(false); }
            if (stickerSpawnArea) stickerSpawnArea.gameObject.SetActive(false);
            if (stampsSpawnArea) 
            {
                stampsSpawnArea.gameObject.SetActive(true);
                var cg = stampsSpawnArea.GetComponent<CanvasGroup>();
                if (cg) cg.alpha = 1f;
            }
            
            if (backgroundPanel)
            {
                var bgImg = backgroundPanel.GetComponent<Image>();
                if (bgImg) bgImg.color = Color.white;
            }
            
            foreach(var st in activeStickers) if(st) st.SetActive(false);
            foreach(var ln in activeLines) if(ln) ln.SetActive(false);
            
            if (objectToActivateAfterFlash) objectToActivateAfterFlash.SetActive(true);
            
            if (supremeRevealContainer) supremeRevealContainer.gameObject.SetActive(false);

            var sortedResults = currentResults.OrderBy(r => (int)r.Entry.Rarity).ToList();
            for (int i = 0; i < sortedResults.Count; i++)
            {
                var reward = sortedResults[i];
                
                var stamp = Instantiate(prizeStampPrefab, stampsSpawnArea);
                var rect = stamp.GetComponent<RectTransform>();
                
                stamp.transform.localScale = Vector3.one;
                
                var img = stamp.GetComponent<Image>();
                if(img) img.color = GetRarityColor(reward.Entry.Rarity);

                var iconImg = stamp.transform.Find("Icon")?.GetComponent<Image>();
                var txtName = stamp.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
                
                if (txtName != null) txtName.text = GetRewardName(reward.Entry);

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
                
                var btn = stamp.GetComponent<Button>();
                if (!btn) btn = stamp.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.onClick.RemoveAllListeners();
                var capReward = reward;
                var capStamp = stamp;
                btn.onClick.AddListener(() => {
                    if (sequenceFinished) OpenRewardModal(capReward, capStamp);
                });
            }

            sequenceFinished = true;
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
            _masterSequence?.Kill();
            CloseRewardModal();
            ClearBoard();
            gameObject.SetActive(false);
            onSequenceFinished?.Invoke();
        }

        private void PlaySound(SoundKey key)
        {
            if (CelestialCross.Audio.AudioManager.Instance != null)
                CelestialCross.Audio.AudioManager.Instance.PlaySFX(key);
        }

        private Sprite GetRewardIcon(RuntimeGachaResult result)
        {
            var reward = result.Entry;
            if (reward.RewardType == GachaRewardType.Unit && reward.UnitData != null) return reward.UnitData.icon;
            if (reward.RewardType == GachaRewardType.Pet && reward.PetSpeciesData != null) return reward.PetSpeciesData.Icon;
            if (reward.RewardType == GachaRewardType.Artifact && reward.ArtifactSet != null) {
                if (result.GeneratedInstance is CelestialCross.Artifacts.ArtifactInstanceData inst)
                {
                    return reward.ArtifactSet.GetIconForSlot(inst.slot);
                }
                if (reward.ArtifactSet.slotIcons != null && reward.ArtifactSet.slotIcons.Count > 0)
                    return reward.ArtifactSet.slotIcons[0].icon;
            }
            return null;
        }

        private string GetRewardName(GachaRewardEntry reward)
        {
            if (reward.RewardType == GachaRewardType.Unit && reward.UnitData != null) return reward.UnitData.displayName;
            if (reward.RewardType == GachaRewardType.Pet && reward.PetSpeciesData != null) return reward.PetSpeciesData.SpeciesName;
            if (reward.RewardType == GachaRewardType.Artifact && reward.ArtifactSet != null) return reward.ArtifactSet.setName;
            return "???";
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