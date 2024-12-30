using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {

    [SerializeField]
    private GameObject scrollView, scrollbar, purchaseUI, fadeOut;

    [SerializeField]
    private GameObject[] Contents, Stages, Vehicles;

    private GameObject[] content;

    [SerializeField]
    private Text moneyText, cantBuyText;

    [SerializeField]
    private AudioSource audio;

    private float scroll_pos = 0, distance;
    private float[] pos;

    private int selectedMenuIndex, selectedIndex;
    private bool changeIndex = true, start = true;

    private void Start() {
        //Khởi tạo nếu không có dữ liệu được lưu
        //PlayerPrefs.DeleteAll();
        if (!PlayerPrefs.HasKey("Stage")) {
            PlayerPrefs.SetInt("Stage", 0);
            PlayerPrefs.SetInt("Vehicle", 0);
            PlayerPrefs.SetInt("Stage_Mars", 0);
            PlayerPrefs.SetInt("Stage_Cave", 0);
            PlayerPrefs.SetInt("Vehicle_Motorcycle", 0);
            PlayerPrefs.SetInt("Money", 71323);
        }
        LoadData();
        MenuChange(1);  //Ban đầu, chế độ xem cuộn được sử dụng làm nội dung về xe.
        start = false;
    }

    //Tải dữ liệu giai đoạn/phương tiện đã chọn và dữ liệu tiền
    private void LoadData() {
        Stages[1].transform.GetChild(1).gameObject.SetActive(PlayerPrefs.GetInt("Stage_Mars").Equals(0));
        Stages[1].GetComponent<Button>().enabled = PlayerPrefs.GetInt("Stage_Mars").Equals(0);
        Stages[2].transform.GetChild(1).gameObject.SetActive(true);
        Stages[3].transform.GetChild(1).gameObject.SetActive(true);
        Vehicles[1].transform.GetChild(1).gameObject.SetActive(PlayerPrefs.GetInt("Vehicle_Motorcycle").Equals(0));
        Vehicles[1].GetComponent<Button>().enabled = PlayerPrefs.GetInt("Vehicle_Motorcycle").Equals(0);
        Vehicles[2].transform.GetChild(1).gameObject.SetActive(true);
        Vehicles[3].transform.GetChild(1).gameObject.SetActive(true);

        moneyText.text = PlayerPrefs.GetInt("Money").ToString();
    }

    private void Update() {
        //Sử dụng chế độ xem cuộn
        if (Input.GetMouseButton(0)) {
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
        }
        else {
            for(int i = 0; i < pos.Length; i++) {
                if(scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2)) {
                    scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
                    selectedIndex = i;
                }
            }
            changeIndex = true;
        }

        //Tăng kích thước của nội dung đã chọn và giảm kích thước của nội dung còn lại.
        for (int i = 0; i < pos.Length; i++) {
            if(scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2)) {
                content[i].transform.localScale = Vector2.Lerp(content[i].transform.localScale, new Vector2(1.2f, 1.2f), 0.1f);
                for(int j = 0; j < pos.Length; j++)
                    if(j != i)
                        content[j].transform.localScale = Vector2.Lerp(content[j].transform.localScale, new Vector2(0.8f, 0.8f), 0.1f);

                if(changeIndex)
                {  //Lưu nội dung đã chọn vào dữ liệu
                    SaveSelectedData(i);
                    changeIndex = false;
                }
            }
        }
    }

    //Nhấn nút màn/phương tiện sẽ thay đổi loại nội dung trong chế độ xem cuộn..
    public void MenuChange(int index) {
        //Nếu bạn cố gắng thay đổi loại nội dung trong khi nội dung đã mở khóa được chọn, bạn sẽ được hỏi có nên mua nội dung đó hay không.
        if (!CheckPurchased() && !start) {
            if(!(selectedMenuIndex == 0 && selectedIndex > 1 || selectedMenuIndex == 1 && selectedIndex > 1)) {
                purchaseUI.SetActive(true);
                return;
            }
        }    
        selectedMenuIndex = index;  //Lưu loại nội dung đã chọn vào một biến

        pos = new float[Contents[index].transform.childCount];
        distance = 1f / (pos.Length - 1f);
        for(int i = 0; i < pos.Length; i++)
            pos[i] = distance * i;

        if(index.Equals(0)) { //Thay đổi loại nội dung thành giai đoạn
            content = Stages;
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value = pos[PlayerPrefs.GetInt("Stage")];
        }
        else if(index.Equals(1)) {  //Thay đổi loại nội dung thành xe
            content = Vehicles;
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value = pos[PlayerPrefs.GetInt("Vehicle")];
        }

        foreach(var obj in Contents)
            obj.SetActive(false);
        Contents[index].SetActive(true);
        scrollView.GetComponent<ScrollRect>().content = Contents[index].GetComponent<RectTransform>();
    }

    //Mua thứ gì đó chưa được mở khóa và thay đổi dữ liệu của bạn.
    public void Purchase() {
        int price, moneyOwned = PlayerPrefs.GetInt("Money");
        if(selectedMenuIndex.Equals(0))
        {  //màn
            price = int.Parse(Stages[selectedIndex].transform.GetChild(1).gameObject.transform.GetChild(1).GetComponent<Text>().text);
            if(moneyOwned - price < 0) { cantBuyText.GetComponent<Animator>().SetTrigger("warning"); return; }
            if(selectedIndex.Equals(1)) PlayerPrefs.SetInt("Stage_Mars", 1);
            
        }
        else
        {  //phương tiện giao thông
            price = int.Parse(Vehicles[selectedIndex].transform.GetChild(1).gameObject.transform.GetChild(1).GetComponent<Text>().text);
            if(moneyOwned - price < 0) { cantBuyText.GetComponent<Animator>().SetTrigger("warning"); return; }
            PlayerPrefs.SetInt("Vehicle_Motorcycle", 1);
        }
        PlayerPrefs.SetInt("Money", moneyOwned - price);
        audio.Play();
        LoadData();
    }

    //Kiểm tra xem nội dung đã chọn đã được mở khóa (đã mua) chưa
    private bool CheckPurchased() {
        if(selectedMenuIndex.Equals(0)) {
            if(selectedIndex != 0)
                return !Stages[selectedIndex].transform.GetChild(1).gameObject.activeSelf;
        }
        else { 
            if(selectedIndex != 0) 
                return !Vehicles[selectedIndex].transform.GetChild(1).gameObject.activeSelf;
        }
        return true;
    }

    //Lưu chỉ mục nội dung đã chọn vào dữ liệu
    private void SaveSelectedData(int index) {
        if(selectedMenuIndex.Equals(0)) {
            if(!CheckPurchased()) return; 
            PlayerPrefs.SetInt("Stage", index);
        }
        else {
            if(!CheckPurchased()) return; 
            PlayerPrefs.SetInt("Vehicle", index);
        }
    }

    //Nhấn nút bắt đầu trò chơi để bắt đầu trò chơi
    public void GameStart() {
        if(!CheckPurchased()) {
            if(!(selectedMenuIndex == 0 && selectedIndex > 1 || selectedMenuIndex == 1 && selectedIndex > 1)) {
                purchaseUI.SetActive(true);
                return;
            }
        }
        fadeOut.GetComponent<Animator>().SetTrigger("FadeOut");
    }
}