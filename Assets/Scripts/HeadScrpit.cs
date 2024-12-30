using UnityEngine;

public class HeadScrpit : MonoBehaviour {

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Khi đầu nhân vật chạm đất, trò chơi kết thúc.
        if (collision.gameObject.CompareTag("Platform") && !GameManager.Instance.isDie)
        {
            GameManager.Instance.PlaySound("crack");
            GameManager.Instance.StartGameOver(true);
        }
    }
}