using UnityEngine;

public class Inventory : MonoBehaviour {
    [SerializeField]
    public PipeData Slot {
        get {
            isEmpty = instance.slot == null;
            return instance.slot;
        }
        set {
            isEmpty = value == null;
            instance.slot = value;
        }
    }

    public bool isEmpty = true;


    [SerializeField] private PipeData slot;


    [Header("UI")] [SerializeField] private GameObject ui;
    [SerializeField] private GameObject uiParent;

    public static Inventory instance;

    private void Awake() {
        instance = this;
        Slot = null;
    }

    void Update() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 10)) {
            if (!hit.collider.CompareTag("Item")) return;

            if (Input.GetKeyDown(Keys.InteractionSecondary)) {
                if (Slot == null) {
                    GrabObject(hit.collider);
                }
                else {
                    Debug.Log("Inventory Full");
                }
            }
        }
    }

    private void GrabObject(Collider _hit) {
        Slot = _hit.GetComponent<Item>().GetItem().data;
        ui.SetActive(true);
    }

    public void RemoveItem() {
        // Destroy(inventory.gameObject);
        Slot = null;
        ui.SetActive(false);
    }
}