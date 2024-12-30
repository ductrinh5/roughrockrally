using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager> {

    [SerializeField]
    private Image fuelGauge, captureImg;
    private Texture2D textureImg;
    private Sprite spriteImg;

    [SerializeField]
    private GameObject fuelWarning, fadeIn, pauseUI, gameOverUI;

    [SerializeField]
    private Text moneyText, moneyEarnedText, distanceText, totaldistanceText, gameStateText;

    [SerializeField]
    private AudioSource[] audio;

    private int totalMoney, moneyEarned = 0;

    public ObjectManager objectManager;
    public CameraController cameraController;
    private CarController carController;

    public bool GasBtnPressed { get; set; }
    public bool BrakeBtnPressed { get; set; }
    public bool isDie { get; set; }
    public bool ReachGoal { get; set; }

    private void Start() {
        Time.timeScale = 1f;
        isDie = false;
        ReachGoal = false;
        fadeIn.GetComponent<Animator>().SetTrigger("FadeIn");  //Thực hiện hoạt ảnh mờ dần
        Initialize();
    }

    private void Update() {
        //Nhấn trở lại để tạm dừng trò chơi
        if (Input.GetKeyDown(KeyCode.Escape))  
            GamePause();

        //Liên tục cập nhật văn bản bằng cách tính toán khoảng cách di chuyển
        if (!gameOverUI.activeSelf)
            distanceText.text = (int)(carController.transform.position.x - carController.StartPos.x) + "m / <color=yellow>1427m</color>";

        //Sau khi trò chơi kết thúc/thành công, chạm một lần nữa để khởi động lại trò chơi
        if (isDie && Input.GetMouseButtonDown(0) && gameOverUI.activeSelf) 
            LoadScene(0);

        //Phát âm thanh khi nhấn nút động cơ/phanh
        if (GasBtnPressed || BrakeBtnPressed)
            PlaySound("engine");
    }

    //Chức năng cài đặt ban đầu trò chơi
    private void Initialize() {
        string objName = "";
        int stageIndex = PlayerPrefs.GetInt("Stage"), vehicleIndex = PlayerPrefs.GetInt("Vehicle");

        //Tải bản đồ đã chọn
        if (stageIndex.Equals(0)) {
            objName = "Country";
            Camera.main.backgroundColor = new Color(0.5803922f, 0.8470589f, 0.937255f, 0);
        }
        else if(stageIndex.Equals(1)) {
            objName = "Mars";
            Camera.main.backgroundColor = new Color(0.8627452f, 0.6666667f, 0.6666667f, 0);
        }
        else if(stageIndex.Equals(2))
            objName = "Cave";
        objectManager.GetObject(objName);

        //Tải xe đã chọn/tạo đối tượng
        if (vehicleIndex.Equals(0)) objName = "HillClimber";
        else if(vehicleIndex.Equals(1)) objName = "Motorcycle";
        CarController vehicle = objectManager.GetObject(objName).GetComponent<CarController>();
        carController = vehicle;

        //điều chỉnh máy ảnh
        cameraController.vehiclePos = vehicle.gameObject.transform;
        cameraController.SetUp();

        //Truy xuất dữ liệu về số tiền bạn sở hữu và cập nhật văn bản
        totalMoney = PlayerPrefs.GetInt("Money");
        moneyText.text = totalMoney.ToString();
    }

    //chức năng tiêu thụ nhiên liệu
    public void FuelConsume() {
        fuelGauge.fillAmount = carController.Fuel;  //Bạn càng di chuyển, đồng hồ đo nhiên liệu càng nhỏ.
        if (fuelGauge.fillAmount <= 0.6f)
        {  //Điều chỉnh màu đồng hồ đo nhiên liệu
            fuelGauge.color = new Color(1, fuelGauge.fillAmount * 0.8f * 2f, 0, 1);  //Hiệu ứng chuyển màu khi thước đo giảm

            if (fuelGauge.fillAmount <= 0.3f)
            {  //Hoạt hình cảnh báo nhiên liệu thấp
                if (!isDie) fuelWarning.SetActive(true);
                if(fuelGauge.fillAmount == 0f)  //Trò chơi kết thúc vì hết nhiên liệu
                    StartGameOver();
            }
        }
        else {
            fuelGauge.color = new Color((1f - fuelGauge.fillAmount) * 2f, 1, 0, 1);  
            fuelWarning.SetActive(false);
        }
    }

    //Lấy nhiên liệu làm đầy đồng hồ đo nhiên liệu.
    public void FuelCharge() {
        carController.Fuel = 1;
        fuelGauge.fillAmount = 1;  //Đổ đầy thanh đo.
        PlaySound("refuel"); //Phát lại âm thanh sạc nhiên liệu
    }

    //Chức năng khi nhận được xu
    public void GetCoin(int price) {
        totalMoney += price;
        moneyEarned += price;
        moneyText.text = totalMoney.ToString(); //Cập nhật số tiền bằng văn bản
        moneyText.GetComponent<Animator>().SetTrigger("EarnMoney");  //hoạt hình
        PlaySound("coin"); //Phát âm thanh đồng xu
    }

    //chức năng nút nhấn ga
    public void GasBtn(bool press) {
        GasBtnPressed = press;
    }

    //chức năng nút ngắt ga
    public void BrakeBtn(bool press) {
        BrakeBtnPressed = press;
    }

    //Chơi âm thanh
    public void PlaySound(string audioName) {
        switch(audioName) {
            case "cameraShutter" :
                audio[0].Play();
                break;
            case "coin":
                audio[1].Play();
                break;
            case "crack":
                audio[2].Play();
                break;
            case "refuel":
                audio[3].Play();
                break;
            case "engine":
                audio[4].Play();
                break;
        }
    }

    //Chức năng tạm dừng trò chơi
    public void GamePause() {
        pauseUI.SetActive(!pauseUI.activeSelf); //Bật/Tắt giao diện người dùng tạm dừng

        if (pauseUI.activeSelf) Time.timeScale = 0f;
        else Time.timeScale = 1f;
    }

    //Chức năng kết thúc trò chơi
    public void StartGameOver(bool forceGameOver = false) {
        if(!isDie) {
            StartCoroutine(GameOver(forceGameOver));
            isDie = true;
        }
    }

    private IEnumerator GameOver(bool forceGameOver = false) {
        float timer = 4f;
        while (timer > 0f)
        {
            if (!forceGameOver && fuelGauge.fillAmount > 0f)
            {
                Debug.Log("Fuel refueled! Game continues.");
                isDie = false;
                fuelWarning.SetActive(false);
                yield break;
            }
            timer -= Time.deltaTime;
            yield return null;
        }

        fuelWarning.SetActive(false);

        //Khi trò chơi kết thúc, ảnh chụp màn hình của chiếc xe sẽ được hiển thị dưới dạng hình ảnh giao diện người dùng.
        yield return new WaitForEndOfFrame();
        Texture2D text = new Texture2D(Screen.width / 5, Screen.height / 3, TextureFormat.RGB24, false);
        textureImg = new Texture2D(Screen.width / 5, Screen.height / 3);
        text.ReadPixels(new Rect(-Screen.width / 2, Screen.height / 3 + 15f, Screen.width, Screen.height), 0, 0);
        text.Apply();
        textureImg = text;
        spriteImg = Sprite.Create(textureImg, new Rect(0, 0, textureImg.width, textureImg.height), new Vector2(0, 0));
        captureImg.sprite = spriteImg;

        //Thay đổi và kích hoạt giá trị văn bản trong Game Over UI
        if (!ReachGoal) gameStateText.text = "<color=#FF4C4C>Game Over</color>";
        else gameStateText.text = "<color=#FFFF4C>Game Complete!!</color>";
        moneyEarnedText.text = "+" + moneyEarned.ToString() + " COINS";  //Hiển thị số xu kiếm được trong trò chơi
        totaldistanceText.text = " Distance : " + (int)(carController.transform.position.x - carController.StartPos.x) + "m";
        gameOverUI.SetActive(true);
        
        PlaySound("cameraShutter"); //Phát âm thanh màn trập máy ảnh
    }

    public void LoadScene(int sceneIndex) {
        PlayerPrefs.SetInt("Money", totalMoney);  //Lưu dữ liệu tiền xu có được
        SceneManager.LoadScene(sceneIndex); 
    }
}