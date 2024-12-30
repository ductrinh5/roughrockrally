using UnityEngine;

public class CameraController : MonoBehaviour {
    public Transform vehiclePos;
    private Vector3 offset;

    //Vị trí xe và cài đặt camera khi bắt đầu trò chơi
    public void SetUp() {
        vehiclePos.position = new Vector3(Camera.main.ViewportToWorldPoint(new Vector3(0, 0)).x + 3f, vehiclePos.position.y, 0);
        CarController carController = vehiclePos.gameObject.GetComponent<CarController>();
        carController.StartPos = vehiclePos.position;
        offset = transform.position - vehiclePos.position;
    }

    private void Update() {
        //Camera theo dõi xe
        transform.position = vehiclePos.position + offset;
    }
}