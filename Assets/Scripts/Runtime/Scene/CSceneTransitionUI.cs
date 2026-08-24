using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CSceneTransitionUI : MonoBehaviour
{
    #region Inspector
    [Header("페이드 연출")]
    [SerializeField] private CanvasGroup _fadeGroup;
    [SerializeField] private float _defaultFadeDuration = 0.25f;
    [SerializeField] private bool _useUnscaledTime = true;

    [Header("로딩 텍스트")]
    [SerializeField] private Text _loadText;
    [SerializeField] private TMP_Text _loadingTMP;

    [Header("옵션")]
    [SerializeField] private bool _hideTextWhenEmpty = true;
    #endregion

    #region Field
    private Coroutine _fadeRoutine;
    #endregion

    public void Initialize()
    {
        if(_fadeGroup == null)
        {
            CPrint.Warn("SeneTransitionUI is Null Inspector Check");
            return;
        }

        _fadeGroup.alpha = 0.0f;
        _fadeGroup.blocksRaycasts = false;
        _fadeGroup.interactable = false;

        SetLoadingText("");
        CPrint.Log("초기화 완료");
    }

    public void SetLoadingText(string msg)
    {
        if(_loadingTMP != null)
        {
            _loadingTMP.text = msg;

            if(_hideTextWhenEmpty)
            {
                _loadingTMP.enabled = !string.IsNullOrEmpty(msg);
            }
        }
    }

    public IEnumerator Co_FadeTo(float targetAlpha, float duration = -1f, bool blockRaycastingWhileFading = true)
    {
        // 코루틴이 비어 있는 경우
        if(_fadeGroup == null)
        {
            yield break;
        }

        // 0보다 작을 경우엔 기본값으로 적용
        if(duration < 0f)
        {
            duration = _defaultFadeDuration;
        }

        // 페이드가 이미 들어와 있는 경우 페이드를 지우기
        if(_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }

        // 내부 처리
        _fadeRoutine = StartCoroutine(Co_Fade_Internal(targetAlpha, duration, blockRaycastingWhileFading));

        yield return _fadeRoutine;

        _fadeRoutine = null;
    }

    private IEnumerator Co_Fade_Internal(float targetAlpha, float duration, bool blockRaycastingWhileFading)
    {
        float startAlpha = _fadeGroup.alpha;
        
        // 페이드중에 입력을 막을건인가?
        _fadeGroup.blocksRaycasts = blockRaycastingWhileFading;

        // 페이드중일때 상호작용을 막기
        _fadeGroup.interactable = false;

        if(duration <= 0f)
        {
            _fadeGroup.alpha = targetAlpha;

            _fadeGroup.blocksRaycasts = (targetAlpha >= 0.99f);

            yield break;
        }

        float t = 0f;

        while (t < duration)
        {
            // 1. 타임 스케일에 영향을 받게 할 것인가?
            float dt = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            t += dt;

            // 0과 1 사이값을 받아서 퍼센트를 나타낼 때 유용한 Clamp01
            float lerp = Mathf.Clamp01(t / duration);

            _fadeGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, lerp);

            yield return null;
        }

        // 2. 알파값을 변경
        _fadeGroup.alpha = targetAlpha;

        // 3. 끝날때 쯤 입력을 다시 할 수 있도록
        _fadeGroup.blocksRaycasts = (targetAlpha >= 0.99f);
    }
}
