using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    [SerializeField]
    private GameObject[] main_Ui;
    [SerializeField]
    private GameObject Cutscene_Ui;

    [SerializeField] private GameObject Last_Obj;

    private float minus = 0;

    [SerializeField] private float duration = 1;
    public void Start()
    {
        main_Ui[0].GetComponent<RectTransform>().DOAnchorPosX(-1500, duration);
        main_Ui[1].GetComponent<RectTransform>().DOAnchorPosX(1500, duration);
        StartCoroutine("Cutscene");
    }
    

    IEnumerator Cutscene() {

        for (int i = 0; i < 3; i++)
        {
            minus -= 1920f;
            Debug.Log(minus);
            yield return new WaitForSeconds(2f);
            Cutscene_Ui.GetComponent<RectTransform>().DOAnchorPosX(minus, 0.4f);
        }
        
        yield return new WaitForSeconds(2f);
        Last_Obj.GetComponent<Image>().DOColor(Color.black, 0.9f);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Room");
    }
}