using UnityEngine;

public class Inventory : MonoBehaviour {
    [SerializeField] public Pipe inventory = null;


    [SerializeField] private GameObject ui;
    [SerializeField] private GameObject uiParent;

    public static Inventory instance;

    private void Awake() {
        instance = this;
        inventory = null;
    }

    void Update() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 10)) {
            if (!hit.collider.CompareTag("Item")) return;

            if (Input.GetKeyDown(KeyCode.E)) {
                if (inventory == null) {
                    GrabObject(hit.collider);
                }
                else {
                    Debug.Log("Inventory Full");
                }
            }
        }
    }

    private void GrabObject(Collider _hit) {
        inventory = _hit.GetComponent<Item>().GetItem();
        ui.SetActive(true);
    }

    public void RemoveItem() {
        Destroy(inventory.gameObject);
        inventory = null;
        ui.SetActive(false);
    }
}