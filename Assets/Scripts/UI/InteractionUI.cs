using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private Image m_Crosshair;
    [SerializeField] private Image m_IconImage;
    [SerializeField] private List<KVPair<string, Sprite>> m_Sprites;

    private Dictionary<string, Sprite> m_SpritesDict;

    public static InteractionUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        m_SpritesDict = m_Sprites.ToDictionary(a => a.key, a => a.value);
    }

    private void Start()
    {
        ToggleCrosshair(true);
    }

    private void Update()
    {
        if (!Player.Instance.Casting.HasTarget(out GameObject target) ||
            Player.Instance.DragRigidbody.IsDragging ||
            Player.Instance.DragHingeInteractable.IsDragging ||
            Player.Instance.DragDrawer.IsDragging)
        {
            m_IconImage.enabled = false;
            return;
        }

        if (m_SpritesDict.TryGetValue(target.tag, out Sprite sprite))
        {
            SetIcon(sprite);
        }
        else
        {
            m_IconImage.enabled = false;
        }
    }

    private void SetIcon(Sprite sprite)
    {
        m_IconImage.enabled = true;
        m_IconImage.sprite = sprite;
    }

    public void ToggleCrosshair(bool value)
    {
        m_Crosshair.enabled = value;
    }
}

[Serializable]
public class KVPair<K, V>
{
    public K key;
    public V value;
}
