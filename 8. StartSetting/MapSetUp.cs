using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapSetUp : MonoBehaviour
{
    [Header("UI 관련 변수들")]
    private ClassMaterial classMaterial;
    [SerializeField] private TMP_Text bookTitle;
    [SerializeField] private TMP_Text bookTitle2;
    [SerializeField] private TMP_Text bookTitleFirst;
    [SerializeField] private TMP_Text bookAuthor;
    [SerializeField] private TMP_Text role1;
    [SerializeField] private TMP_Text role2;
    [SerializeField] private TMP_Text role3;
    [SerializeField] private TMP_Text role4;
    [SerializeField] private TMP_Text paintTitle;

    private BookPageSetUp bookPageSetUp;
    private GameObject img_loading;
    private GameObject Btn_Left;
    private GameObject Btn_Right;

    private void Awake()
    {
        bookPageSetUp = GameObject.Find("BookCanvas").GetComponent<BookPageSetUp>();
    }

    void Start()
    {
        Btn_Left = GameObject.Find("Btn_Left");
        Btn_Right = GameObject.Find("Btn_Right");
        img_loading = GameObject.Find("Img_Loading");
        bookTitleFirst = GameObject.Find("Txt_Title").GetComponent<TMP_Text>();
        bookAuthor = GameObject.Find("Txt_Author").GetComponent<TMP_Text>();
    }

    public void ApplyClassMaterial()
    {
        classMaterial = HttpRoomSetUp.GetInstance().RealClassMaterial;

        bookPageSetUp.text = classMaterial.bookContents;
        bookTitle.text = classMaterial.bookTitle;
        bookTitle2.text = classMaterial.bookTitle;
        bookTitleFirst.text = classMaterial.bookTitle;
        bookAuthor.text = classMaterial.author;
        paintTitle.text = classMaterial.bookTitle;
        role1.text = classMaterial.lessonRoles[0];
        role2.text = classMaterial.lessonRoles[1];
        role3.text = classMaterial.lessonRoles[2];
        role4.text = classMaterial.lessonRoles[3];

        img_loading.SetActive(false);
        Btn_Left.GetComponent<Button>().interactable = true;
        Btn_Right.GetComponent<Button>().interactable = true;
    }
}
