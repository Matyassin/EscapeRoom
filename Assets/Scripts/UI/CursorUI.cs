using UnityEngine;
using UnityEngine.InputSystem;

public class CursorUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private Texture2D m_CursorImage;

    public static CursorUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        Cursor.SetCursor(m_CursorImage, Vector2.zero, CursorMode.Auto);
    }

    public void ToggleCursor(bool value)
    {
        if (!value && InputHandler.Instance.IsCurrentControlKeyboard())
            Mouse.current.WarpCursorPosition(new Vector2(Screen.width / 2f, Screen.height / 2f));

        Cursor.visible = value;
        Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;

        if (value && InputHandler.Instance.IsCurrentControlKeyboard())
            Mouse.current.WarpCursorPosition(new Vector2(Screen.width / 2f, Screen.height / 2f));
    }
}
