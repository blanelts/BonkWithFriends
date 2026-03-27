using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes;
using MelonLoader;
using UnityEngine;

namespace Megabonk.BonkWithFriends.MonoBehaviours.Enemies;

[RegisterTypeInIl2Cpp]
public class EnemyHitFlash : MonoBehaviour
{
    public EnemyHitFlash(System.IntPtr intPtr) : base(intPtr) { }
    public EnemyHitFlash()
        : base(ClassInjector.DerivedConstructorPointer<EnemyHitFlash>())
    {
        ClassInjector.DerivedConstructorBody((Il2CppObjectBase)(object)this);
    }

    private Renderer[] _renderers;
    private Color[] _originalColors;
    private float _flashTimer;
    private bool _isFlashing;
    private bool _initialized;
    private const float FLASH_DURATION = 0.15f;

    [HideFromIl2Cpp]
    private void CacheRenderers()
    {
        if (_initialized) return;

        var il2cppRenderers = ((Component)this).GetComponentsInChildren<Renderer>(true);
        if (il2cppRenderers == null || il2cppRenderers.Length == 0)
            return;

        _renderers = new Renderer[il2cppRenderers.Length];
        _originalColors = new Color[il2cppRenderers.Length];
        for (int i = 0; i < il2cppRenderers.Length; i++)
        {
            _renderers[i] = il2cppRenderers[i];
            if ((Object)(object)_renderers[i] != (Object)null &&
                (Object)(object)_renderers[i].material != (Object)null)
            {
                _originalColors[i] = _renderers[i].material.color;
            }
        }
        _initialized = true;
    }

    void Start()
    {
        CacheRenderers();
    }

    [HideFromIl2Cpp]
    public void TriggerFlash()
    {
        if (!_initialized) CacheRenderers();
        if (_renderers == null) return;

        _flashTimer = FLASH_DURATION;
        _isFlashing = true;

        for (int i = 0; i < _renderers.Length; i++)
        {
            if ((Object)(object)_renderers[i] != (Object)null &&
                (Object)(object)_renderers[i].material != (Object)null)
            {
                _renderers[i].material.color = Color.white;
            }
        }
    }

    void Update()
    {
        if (!_initialized || _renderers == null) return;

        // Detect native flash (white color set by game code, not by us)
        if (!_isFlashing)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if ((Object)(object)_renderers[i] != (Object)null &&
                    (Object)(object)_renderers[i].material != (Object)null)
                {
                    Color c = _renderers[i].material.color;
                    Color orig = _originalColors[i];
                    // Check if renderer turned white but original isn't white
                    if (c.r > 0.95f && c.g > 0.95f && c.b > 0.95f &&
                        !(orig.r > 0.95f && orig.g > 0.95f && orig.b > 0.95f))
                    {
                        _flashTimer = FLASH_DURATION;
                        _isFlashing = true;
                        break;
                    }
                }
            }
        }

        if (!_isFlashing) return;

        _flashTimer -= Time.deltaTime;
        if (_flashTimer <= 0f)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if ((Object)(object)_renderers[i] != (Object)null &&
                    (Object)(object)_renderers[i].material != (Object)null)
                {
                    _renderers[i].material.color = _originalColors[i];
                }
            }
            _isFlashing = false;
        }
    }
}
