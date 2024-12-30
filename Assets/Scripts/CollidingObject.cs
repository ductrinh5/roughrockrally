using UnityEngine;

public class CollidingObject : MonoBehaviour {

    [SerializeField]
    private int price;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.CompareTag("Vehicle")) {
            //Khi lấy nhiên liệu
            if (gameObject.name.Contains("Fuel")) {  
                GameManager.Instance.FuelCharge();
                gameObject.SetActive(false);
            }

            //Đạt đến đích mục tiêu và thành công trong trò chơi
            else if (gameObject.name.Contains("Goal")) {  
                GameManager.Instance.ReachGoal = true;
                GameManager.Instance.StartGameOver(true);
            }

            //Lấy lại tiền xu
            else if (gameObject.name.Contains("Coin")) {  
                GameManager.Instance.GetCoin(price);
                gameObject.SetActive(false);
            }
        }
    }
}