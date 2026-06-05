using UnityEngine;

[RequireComponent(typeof(FpsController))]
[RequireComponent(typeof(PlayerCasting))]
[RequireComponent(typeof(DragRigidbody))]
[RequireComponent(typeof(DragHingeInteractable))]
[RequireComponent(typeof(DragDrawer))]
public class Player : MonoBehaviour
{
    public FpsController FpsController {  get; private set; }
    public PlayerCasting Casting { get; private set; }
    public DragRigidbody DragRigidbody { get; private set; }
    public DragHingeInteractable DragHingeInteractable { get; private set; }
    public DragDrawer DragDrawer { get; private set; }
    public CameraFollowPlayer CameraFollowPlayer;

    public static Player Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        FpsController = GetComponent<FpsController>();
        Casting = GetComponent<PlayerCasting>();
        DragRigidbody = GetComponent<DragRigidbody>();
        DragHingeInteractable = GetComponent<DragHingeInteractable>();
        DragDrawer = GetComponent<DragDrawer>();
    }
}
