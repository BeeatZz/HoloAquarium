


#if DOTWEEN_UITOOLKIT && UNITY_2021_3_OR_NEWER 

using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening.Core;
using DG.Tweening.Core.Enums;
using DG.Tweening.Plugins.Options;

#pragma warning disable 1591
namespace DG.Tweening
{
	public static class DOTweenModuleUIToolkit
    {
        #region Shortcuts

        #region VisualElement

        
        
        
        
        public static TweenerCore<Vector3, Vector3, VectorOptions> DOMove(this VisualElement target, Vector3 endValue, float duration, bool snapping = false)
        {
            TweenerCore<Vector3, Vector3, VectorOptions> t
                = DOTween.To(() => target.resolvedStyle.translate, x => target.style.translate = new Translate(x.x, x.y, x.z), endValue, duration);
            t.SetOptions(snapping).SetTarget(target);
            return t;
        }
        
        
        
        
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOMove(this VisualElement target, Vector2 endValue, float duration, bool snapping = false)
        {
            TweenerCore<Vector2, Vector2, VectorOptions> t
                = DOTween.To(() => (Vector2)target.resolvedStyle.translate, x => target.style.translate = new Translate(x.x, x.y, 0), endValue, duration);
            t.SetOptions(snapping).SetTarget(target);
            return t;
        }
        
        
        
        
        public static TweenerCore<Vector3, Vector3, VectorOptions> DOMoveX(this VisualElement target, float endValue, float duration, bool snapping = false)
        {
            TweenerCore<Vector3, Vector3, VectorOptions> t
                = DOTween.To(() => target.resolvedStyle.translate, x => target.style.translate = new Translate(x.x, x.y, x.z), new Vector3(endValue, 0, 0), duration);
            t.SetOptions(AxisConstraint.X, snapping).SetTarget(target);
            return t;
        }
        
        
        
        
        public static TweenerCore<Vector3, Vector3, VectorOptions> DOMoveY(this VisualElement target, float endValue, float duration, bool snapping = false)
        {
            TweenerCore<Vector3, Vector3, VectorOptions> t
                = DOTween.To(() => target.resolvedStyle.translate, x => target.style.translate = new Translate(x.x, x.y, x.z), new Vector3(0, endValue, 0), duration);
            t.SetOptions(AxisConstraint.Y, snapping).SetTarget(target);
            return t;
        }
        
        
        
        
        public static TweenerCore<Vector3, Vector3, VectorOptions> DOMoveZ(this VisualElement target, float endValue, float duration, bool snapping = false)
        {
            TweenerCore<Vector3, Vector3, VectorOptions> t
                = DOTween.To(() => target.resolvedStyle.translate, x => target.style.translate = new Translate(x.x, x.y, x.z), new Vector3(0, 0, endValue), duration);
            t.SetOptions(AxisConstraint.Z, snapping).SetTarget(target);
            return t;
        }
        
        
        
        
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOScale(this VisualElement target, Vector2 endValue, float duration)
        {
            TweenerCore<Vector2, Vector2, VectorOptions> t
                = DOTween.To(() => (Vector2)target.resolvedStyle.scale.value, x => target.style.scale = new Scale(x), endValue, duration);
            t.SetTarget(target);
            return t;
        }
        
        
        
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOScale(this VisualElement target, float endValue, float duration)
        {
            TweenerCore<Vector2, Vector2, VectorOptions> t
                = DOTween.To(() => (Vector2)target.resolvedStyle.scale.value, x => target.style.scale = new Scale(x), new Vector2(endValue, endValue), duration);
            t.SetTarget(target);
            return t;
        }
        
        
        
        
        public static TweenerCore<float, float, FloatOptions> DORotate(this VisualElement target, float endValue, float duration)
        {
            TweenerCore<float, float, FloatOptions> t
                = DOTween.To(() => target.resolvedStyle.rotate.angle.value, x => target.style.rotate = new Rotate(x), endValue, duration);
            t.SetTarget(target);
            return t;
        }
        
        
        
        
        
        
        
        
        
        
        
        public static Tweener DOPunch(this VisualElement target, Vector3 punch, float duration, int vibrato = 10, float elasticity = 1, bool snapping = false)
        {
            return DOTween.Punch(() => target.resolvedStyle.translate, x => target.style.translate = new Translate(x.x, x.y, x.z), punch, duration, vibrato, elasticity)
                .SetTarget(target).SetOptions(snapping);
        }
        
        
        
        
        
        
        
        
        
        
        
        public static Tweener DOShake(this VisualElement target, float duration, float strength = 100, int vibrato = 10, float randomness = 90, bool snapping = false, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full)
        {
            return DOTween.Shake(() => target.resolvedStyle.translate, x => target.style.translate = new Translate(x.x, x.y, x.z), duration, strength, vibrato, randomness, true, fadeOut, randomnessMode)
                .SetTarget(target).SetSpecialStartupMode(SpecialStartupMode.SetShake).SetOptions(snapping);
        }
        
        
        
        
        
        
        
        
        
        
        public static Tweener DOShake(this VisualElement target, float duration, Vector2 strength, int vibrato = 10, float randomness = 90, bool snapping = false, bool fadeOut = true, ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full)
        {
            return DOTween.Shake(() => target.resolvedStyle.translate, x => target.style.translate = new Translate(x.x, x.y, x.z), duration, strength, vibrato, randomness, fadeOut, randomnessMode)
                .SetTarget(target).SetSpecialStartupMode(SpecialStartupMode.SetShake).SetOptions(snapping);
        }

        #endregion

        #endregion
	}
}
#endif
