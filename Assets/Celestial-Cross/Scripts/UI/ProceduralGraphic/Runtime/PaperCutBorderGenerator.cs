using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace CelestialCross.UI.ProceduralGraphic
{
    [RequireComponent(typeof(ProceduralGraphic))]
    [ExecuteAlways]
    public class PaperCutBorderGenerator : MonoBehaviour
    {
        [Title("Target")]
        [Tooltip("A imagem que servirá de molde para o recorte")]
        public Image targetImage;

        [Tooltip("Atualiza a borda automaticamente se a imagem (Sprite) mudar. Útil para animações 2D quadro a quadro.")]
        public bool autoUpdateOnChange = true;

        [Title("Settings")]
        [Tooltip("Expansão da borda para fora (ex: 0.15 = 15%)")]
        [Range(0f, 1f)]
        public float borderExpansion = 0.15f;

        [Tooltip("Quantidade de irregularidade no corte do papel")]
        [Range(0f, 0.2f)]
        public float jitterAmount = 0.03f;

        [Tooltip("Quantos pontos a borda de papel deve ter")]
        [Range(4, 128)]
        public int borderPointCount = 24;

        [Tooltip("Seed fixa para a aleatoriedade do corte (mesmo valor = mesma forma)")]
        public int randomSeed = 42;
        
        [Tooltip("Limiar do canal alpha para considerar como contorno")]
        [Range(0.01f, 1f)]
        public float alphaThreshold = 0.5f;

        [Tooltip("Preenche espaços vazios e concavidades (como braços na cintura). Um valor maior une vãos maiores (em pixels). 0 = Desativado.")]
        [Range(0, 50)]
        public int closeGapsRadius = 10;

        private ProceduralGraphic _graphic;
        private Sprite _lastProcessedSprite;
        private ShapePreset _runtimePreset;

        private void Awake()
        {
            _graphic = GetComponent<ProceduralGraphic>();
        }

        private void Update()
        {
            if (autoUpdateOnChange && targetImage != null)
            {
                if (targetImage.sprite != _lastProcessedSprite)
                {
                    GenerateBorder();
                }
            }
        }

        [Button(ButtonSizes.Large), GUIColor(1f, 0.9f, 0.5f)]
        public void GenerateBorder()
        {
            if (_graphic == null) _graphic = GetComponent<ProceduralGraphic>();

            if (targetImage == null || targetImage.sprite == null || targetImage.sprite.texture == null)
            {
                if (_graphic.Preset != null && _runtimePreset != null)
                {
                    _runtimePreset.Points.Clear();
                    _graphic.SetVerticesDirty();
                }
                return;
            }

            Texture2D tex = targetImage.sprite.texture;
            if (!tex.isReadable)
            {
                Debug.LogWarning($"[PaperCutBorder] A textura '{tex.name}' não possui 'Read/Write' ativado nas import settings.");
                return;
            }

            // 1. Extrair
            var contour = ContourExtractor.ExtractContour(tex, alphaThreshold, closeGapsRadius);
            if (contour.Count == 0) return;

            // 2. Simplificar
            var simplified = ContourExtractor.SimplifyContour(contour, borderPointCount);
            
            // 3. Normalizar
            var normalized = ContourExtractor.NormalizeContour(simplified, tex.width, tex.height);

            // 4. Expandir
            var expanded = ContourExtractor.ExpandContour(normalized, borderExpansion);

            // 5. Jitter
            var finalPoints = ContourExtractor.ApplyJitter(expanded, jitterAmount, randomSeed);

            // 6. Aplicar no Graphic local
            if (_runtimePreset == null)
            {
                _runtimePreset = ScriptableObject.CreateInstance<ShapePreset>();
                _runtimePreset.name = "RuntimePaperCutPreset";
            }
            
            _runtimePreset.Points.Clear();
            foreach (var p in finalPoints)
            {
                _runtimePreset.Points.Add(new ShapePreset.ShapePoint 
                { 
                    position = p, 
                    isSharp = true 
                });
            }

            // Força a atualização do preset
            _graphic.SetPreset(_runtimePreset);
            _lastProcessedSprite = targetImage.sprite;
        }
    }
}
