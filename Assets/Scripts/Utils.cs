using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public static class Utilities
{
    public static IEnumerator InvokeDelayedScaled(Action action, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        action?.Invoke();
    }

    public static IEnumerator InvokeDelayedUnscaled(Action action, float delaySeconds)
    {
        yield return new WaitForSecondsRealtime(delaySeconds);
        action?.Invoke();
    }

    /// <summary>
    /// Interpolates a selected field on an object between a start and target value by Time.deltaTime.
    /// </summary>
    /// <typeparam name="T">The object type (e.g., Transform).</typeparam>
    /// <typeparam name="V">The value type (e.g., float, Vector3).</typeparam>
    /// <param name="thing">The object whose field is being interpolated.</param>
    /// <param name="lerpValue">The setter that applies the interpolated value to the object.</param>
    /// <param name="start">The starting value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="lerpFunction">The lerping function (e.g., Vector3.Lerp).</param>
    /// <param name="duration">The duration of the interpolation.</param>
    /// <param name="endAction">An optional action to call after interpolation completes.</param>
    /// <returns>Coroutine IEnumerator.</returns>
    public static IEnumerator InterpolateByScaled<T, V>(this T thing, Action<T, V> lerpValue, V start, V target, Func<V, V, float, V> lerpFunction, float duration, Action endAction = null)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime <= duration)
        {
            lerpValue(thing, lerpFunction(start, target, elapsedTime / duration));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        lerpValue(thing, target);
        endAction?.Invoke();
    }

    /// <summary>
    /// Interpolates a selected field on an object between a start and target value by Time.deltaTime.
    /// </summary>
    /// <typeparam name="T">The object type (e.g., Transform).</typeparam>
    /// <typeparam name="V">The value type (e.g., float, Vector3).</typeparam>
    /// <param name="thing">The object whose field is being interpolated.</param>
    /// <param name="lerpValue">The setter that applies the interpolated value to the object.</param>
    /// <param name="start">The starting value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="lerpFunction">The lerping function (e.g., Vector3.Lerp).</param>
    /// <param name="easingFunction">A function that modifies the original function.</param>
    /// <param name="duration">The duration of the interpolation.</param>
    /// <param name="endAction">An optional action to call after interpolation completes.</param>
    /// <returns>Coroutine IEnumerator.</returns>
    public static IEnumerator InterpolateByScaled<T, V>(this T thing, Action<T, V> lerpValue, V start, V target, Func<V, V, float, V> lerpFunction, Func<float, float> easingFunction, float duration, Action endAction = null)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime <= duration)
        {
            lerpValue(thing, lerpFunction(start, target, easingFunction(elapsedTime / duration)));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        lerpValue(thing, target);
        endAction?.Invoke();
    }

    /// <summary>
    /// Interpolates a selected field on an object between a start and target value by Time.unscaledDeltaTime.
    /// </summary>
    /// <typeparam name="T">The object type (e.g., Transform).</typeparam>
    /// <typeparam name="V">The value type (e.g., float, Vector3).</typeparam>
    /// <param name="thing">The object whose field is being interpolated.</param>
    /// <param name="lerpValue">The setter that applies the interpolated value to the object.</param>
    /// <param name="start">The starting value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="lerpFunction">The lerping function (e.g., Vector3.Lerp).</param>
    /// <param name="duration">The duration of the interpolation.</param>
    /// <param name="endAction">An optional action to call after interpolation completes.</param>
    /// <returns>Coroutine IEnumerator.</returns>
    public static IEnumerator InterpolateByUnscaled<T, V>(this T thing, Action<T, V> lerpValue, V start, V target, Func<V, V, float, V> lerpFunction, float duration, Action endAction = null)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime <= duration)
        {
            lerpValue(thing, lerpFunction(start, target, elapsedTime / duration));

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        lerpValue(thing, target);
        endAction?.Invoke();
    }

    /// <summary>
    /// Interpolates a selected field on an object between a start and target value by Time.unscaledDeltaTime.
    /// </summary>
    /// <typeparam name="T">The object type (e.g., Transform).</typeparam>
    /// <typeparam name="V">The value type (e.g., float, Vector3).</typeparam>
    /// <param name="thing">The object whose field is being interpolated.</param>
    /// <param name="lerpValue">The setter that applies the interpolated value to the object.</param>
    /// <param name="start">The starting value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="lerpFunction">The lerping function (e.g., Vector3.Lerp).</param>
    /// <param name="easingFunction">A function that modifies the original function.</param>
    /// <param name="duration">The duration of the interpolation.</param>
    /// <param name="endAction">An optional action to call after interpolation completes.</param>
    /// <returns>Coroutine IEnumerator.</returns>
    public static IEnumerator InterpolateByUnscaled<T, V>(this T thing, Action<T, V> lerpValue, V start, V target, Func<V, V, float, V> lerpFunction, Func<float, float> easingFunction, float duration, Action endAction = null)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime <= duration)
        {
            lerpValue(thing, lerpFunction(start, target, easingFunction(elapsedTime / duration)));

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        lerpValue(thing, target);
        endAction?.Invoke();
    }

    /// <summary>
    /// Interpolates any value between a start and target by Time.deltaTime.
    /// Can be used for properties, fields, or standalone values (e.g., floats, Vector3, Quaternion, Color).
    /// </summary>
    /// <typeparam name="V">The value type (e.g., float, Vector3).</typeparam>
    /// <param name="lerpValue">The setter that applies the interpolated value to the object.</param>
    /// <param name="start">The starting value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="lerpFunction">The lerping function (e.g., Vector3.Lerp).</param>
    /// <param name="duration">The duration of the interpolation.</param>
    /// <param name="endAction">An optional action to call after interpolation completes.</param>
    /// <returns>Coroutine IEnumerator.</returns>
    public static IEnumerator InterpolateByValueScaled<V>(Action<V> lerpValue, V start, V target, float duration, Func<V, V, float, V> lerpFunction, Action endAction = null)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            lerpValue(lerpFunction(start, target, elapsedTime / duration));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        lerpValue(target);
        endAction?.Invoke();
    }

    /// <summary>
    /// Interpolates any value between a start and target by Time.unscaledDeltaTime.
    /// Can be used for properties, fields, or standalone values (e.g., floats, Vector3, Quaternion, Color).
    /// </summary>
    /// <typeparam name="V">The value type (e.g., float, Vector3).</typeparam>
    /// <param name="lerpValue">The setter that applies the interpolated value to the object.</param>
    /// <param name="start">The starting value.</param>
    /// <param name="target">The target value.</param>
    /// <param name="lerpFunction">The lerping function (e.g., Vector3.Lerp).</param>
    /// <param name="duration">The duration of the interpolation.</param>
    /// <param name="endAction">An optional action to call after interpolation completes.</param>
    /// <returns>Coroutine IEnumerator.</returns>
    public static IEnumerator InterpolateByValueUnscaled<V>(Action<V> lerpValue, V start, V target, float duration, Func<V, V, float, V> lerpFunction, Action endAction = null)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            lerpValue(lerpFunction(start, target, elapsedTime / duration));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        lerpValue(target);
        endAction?.Invoke();
    }

    public static T RandomElement<T>(this List<T> list)
    {
        if (list.Count == 0)
            return default;

        return list[Random.Range(0, list.Count)];
    }

    public static T RandomElement<T>(this T[] arr)
    {
        if (arr.Length == 0)
            return default;

        return arr[Random.Range(0, arr.Length)];
    }

    public static Vector3 RandomRange(Vector3 min, Vector3 max)
    {
        return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
    }
}

public class Tween<TTarget, TValue> where TTarget : class
{
    public delegate TValue LerpDelegate(TValue v1, TValue v2, float t);
    public delegate float EaseDelegate(float x);
    public delegate void SetterDelegate(TTarget obj, TValue value);
    public delegate TValue ProviderDelegate();

    public struct Data
    {
        public ProviderDelegate StartProvider;
        public ProviderDelegate EndProvider;
        public float Duration;
        public LerpDelegate LerpFunction;
        public EaseDelegate EasingFunction;
        public SetterDelegate Setter;
        public bool UseUnscaledTime;
    }

    private readonly Data m_Data;

    private Tween(Data data)
    {
        m_Data = data;
    }

    public static Builder Configure()
    {
        return new Builder();
    }

    public IEnumerator AsRoutine(TTarget target, Action endAction = null)
    {
        return CoreRoutine(target, reversed: false, endAction);
    }

    public IEnumerator AsRoutineReversed(TTarget target, Action endAction = null)
    {
        return CoreRoutine(target, reversed: true, endAction);
    }

    private IEnumerator CoreRoutine(TTarget target, bool reversed, Action endAction)
    {
        float elapsedTime = 0.0f;
        TValue startValue = m_Data.StartProvider();
        TValue endValue = m_Data.EndProvider();

        while (elapsedTime < m_Data.Duration)
        {
            float normalTime = elapsedTime / m_Data.Duration;
            float t = reversed ? (1.0f - normalTime) : normalTime;

            TValue current = m_Data.LerpFunction(startValue, endValue, m_Data.EasingFunction(t));
            m_Data.Setter(target, current);

            elapsedTime += m_Data.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }

        m_Data.Setter(target, reversed ? startValue : endValue);
        endAction?.Invoke();
    }

    public class Builder
    {
        private Data m_Data = new()
        {
            Duration = 1.0f,
            EasingFunction = x => x,
            UseUnscaledTime = false
        };

        public Tween<TTarget, TValue> Build()
        {
            if (m_Data.Setter == null) throw new ArgumentNullException(nameof(m_Data.Setter));
            if (m_Data.LerpFunction == null) throw new ArgumentNullException(nameof(m_Data.LerpFunction));
            if (m_Data.StartProvider == null) throw new InvalidOperationException("Start value or provider is missing.");
            if (m_Data.EndProvider == null) throw new InvalidOperationException("End value or provider is missing.");

            return new Tween<TTarget, TValue>(m_Data);
        }

        public Builder Setter(SetterDelegate setter)
        {
            m_Data.Setter = setter;
            return this;
        }

        public Builder Start(TValue start)
        {
            m_Data.StartProvider = () => start;
            return this;
        }

        public Builder Start(ProviderDelegate startProvider)
        {
            m_Data.StartProvider = startProvider;
            return this;
        }

        public Builder End(TValue end)
        {
            m_Data.EndProvider = () => end;
            return this;
        }

        public Builder End(ProviderDelegate endProvider)
        {
            m_Data.EndProvider = endProvider;
            return this;
        }

        public Builder Duration(float duration)
        {
            m_Data.Duration = duration;
            return this;
        }

        public Builder LerpFunction(LerpDelegate lerpFunc)
        {
            m_Data.LerpFunction = lerpFunc;
            return this;
        }

        public Builder EasingFunction(EaseDelegate easingFunc)
        {
            m_Data.EasingFunction = easingFunc;
            return this;
        }

        public Builder UnscaledTime()
        {
            m_Data.UseUnscaledTime = true;
            return this;
        }

        public Builder ScaledTime()
        {
            m_Data.UseUnscaledTime = false;
            return this;
        }
    }
}

public static class TweenBuilderExtensions
{
    public static Tween<Transform, Vector3>.Builder ForPosition(this Tween<Transform, Vector3>.Builder builder)
    {
        return builder
            .LerpFunction(Vector3.Lerp)
            .Setter((t, p) => t.position = p);
    }

    public static Tween<Transform, Vector3>.Builder ForLocalPosition(this Tween<Transform, Vector3>.Builder builder)
    {
        return builder
            .LerpFunction(Vector3.Lerp)
            .Setter((t, p) => t.localPosition = p);
    }

    public static Tween<Transform, Quaternion>.Builder ForRotation(this Tween<Transform, Quaternion>.Builder builder)
    {
        return builder
            .LerpFunction(Quaternion.Lerp)
            .Setter((t, r) => t.rotation = r);
    }

    public static Tween<Transform, Quaternion>.Builder ForLocalRotation(this Tween<Transform, Quaternion>.Builder builder)
    {
        return builder
            .LerpFunction(Quaternion.Lerp)
            .Setter((t, r) => t.localRotation = r);
    }

    public static Tween<Image, Color>.Builder ForColor(this Tween<Image, Color>.Builder builder)
    {
        return builder
            .LerpFunction(Color.Lerp)
            .Setter((i, c) => i.color = c);
    }

    public static Tween<CanvasGroup, float>.Builder ForAlpha(this Tween<CanvasGroup, float>.Builder builder)
    {
        return builder
            .LerpFunction(Mathf.Lerp)
            .Setter((cg, a) => cg.alpha = a);
    }

    public static Tween<Light, Color>.Builder ForColor(this Tween<Light, Color>.Builder builder)
    {
        return builder
            .LerpFunction(Color.Lerp)
            .Setter((l, c) => l.color = c);
    }

    public static Tween<Light, float>.Builder ForIntensity(this Tween<Light, float>.Builder builder)
    {
        return builder
            .LerpFunction(Mathf.Lerp)
            .Setter((l, i) => l.intensity = i);
    }

    // Cached
    private static readonly MaterialPropertyBlock s_PropertyBlock = new MaterialPropertyBlock();
    private static readonly int s_EmissionColorId = Shader.PropertyToID("_EmissionColor");

    public static Tween<Renderer, Color>.Builder ForMaterialEmissionColor(this Tween<Renderer, Color>.Builder builder)
    {
        return builder
            .LerpFunction(Color.Lerp)
            .Setter((r, c) =>
            {
                r.GetPropertyBlock(s_PropertyBlock);
                s_PropertyBlock.SetColor(s_EmissionColorId, c);
                r.SetPropertyBlock(s_PropertyBlock);
            });
    }
}
