using UnityEngine;
using UnityEngine.UI;

namespace CelestialCross.Tutorial
{
    [RequireComponent(typeof(Image))]
    public class TutorialInputBlocker : MonoBehaviour, ICanvasRaycastFilter
    {
        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            if (TutorialManager.Instance == null || !TutorialManager.Instance.IsActive)
                return false; // Não bloqueia nada se não estiver ativo

            var step = TutorialManager.Instance.CurrentStep;
            if (step == null || step.HighlightType == TutorialHighlightTarget.None)
                return true; // Bloqueia tudo se não houver highlight (exceto se a condição for ClickAnywhere, mas o Overlay tem um botão transparente por cima)

            // Se for ClickAnywhere, o InputBlocker deve permitir cliques para que o fullscreenButton do Overlay detecte
            if (step.AdvanceCondition == TutorialAdvanceCondition.ClickAnywhere)
                return true;

            // Calcula se o clique está dentro do spotlight (buraco)
            // Pegamos o centro e tamanho do SpotlightManager ou TutorialManager
            // Aqui vamos assumir que o spotlight está centralizado no alvo.
            
            // Nota: Para precisão total, poderíamos pegar os valores direto do material,
            // mas vamos calcular de forma similar ao Shader.
            
            // Precisamos encontrar a posição do alvo novamente ou passar pelo Manager
            // Por simplicidade, vamos usar o TutorialManager para validar.
            
            return !IsInsideSpotlight(sp);
        }

        private bool IsInsideSpotlight(Vector2 screenPos)
        {
            // Esta lógica deve bater com a do TutorialManager.UpdateSpotlightForStep
            var step = TutorialManager.Instance.CurrentStep;
            if (step == null) return false;

            Vector2 targetScreenPos = Vector2.zero;
            
            // Re-localiza o alvo (poderíamos otimizar guardando no Manager)
            switch (step.HighlightType)
            {
                case TutorialHighlightTarget.UIButton:
                    GameObject btn = GameObject.Find(step.TargetIdentifier);
                    if (btn != null) targetScreenPos = RectTransformUtility.WorldToScreenPoint(null, btn.transform.position);
                    else return false;
                    break;
                case TutorialHighlightTarget.GridTile:
                    string[] parts = step.TargetIdentifier.Split('_');
                    if (parts.Length == 3 && int.TryParse(parts[1], out int x) && int.TryParse(parts[2], out int y))
                    {
                        Vector3 worldPos = GridMap.Instance.GridToWorld(new Vector2Int(x, y));
                        targetScreenPos = Camera.main.WorldToScreenPoint(worldPos);
                    }
                    else return false;
                    break;
                case TutorialHighlightTarget.Unit:
                    GameObject unit = GameObject.Find(step.TargetIdentifier);
                    if (unit != null) targetScreenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
                    else return false;
                    break;
                default:
                    return false;
            }

            Vector2 diff = screenPos - targetScreenPos;
            Vector2 halfSize = step.SpotlightSize * 0.5f;

            // Checagem retangular simples
            return Mathf.Abs(diff.x) <= halfSize.x && Mathf.Abs(diff.y) <= halfSize.y;
        }
    }
}
