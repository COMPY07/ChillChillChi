using UnityEngine;
using DG.Tweening;

public class RoomManager : MonoBehaviour
{
    private static RoomManager _instance = null;
    public bool[] OnColor;
    [SerializeField] private GameObject[] ColorObj;
    [SerializeField] private string[] Description;
    Color Background = new Color(40, 40, 40, 255);

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (OnColor.Length > 0 && OnColor[0]) // OnColor 배열이 비어있지 않은지 확인
        {
            SpriteRenderer[] allChildren = ColorObj[0].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.white, 0.4f);
            }
        }
        else
        {
            SpriteRenderer[] allChildren = ColorObj[0].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.HSVToRGB(0.2f, 0.2f, 0.2f), 0);

            }
        }
        if (OnColor.Length > 0 && OnColor[1]) // OnColor 배열이 비어있지 않은지 확인
        {
            SpriteRenderer[] allChildren = ColorObj[1].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.white, 0.4f);
            }
        }
        else
        {
            SpriteRenderer[] allChildren = ColorObj[1].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.HSVToRGB(0.2f, 0.2f, 0.2f), 0);

            }
        }
        if (OnColor.Length > 0 && OnColor[2]) // OnColor 배열이 비어있지 않은지 확인
        {
            SpriteRenderer[] allChildren = ColorObj[2].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.white, 0.4f);
            }
        }
        else
        {
            SpriteRenderer[] allChildren = ColorObj[2].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.HSVToRGB(0.2f, 0.2f, 0.2f), 0);

            }
        }
        if (OnColor.Length > 0 && OnColor[3]) // OnColor 배열이 비어있지 않은지 확인
        {
            SpriteRenderer[] allChildren = ColorObj[3].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.white, 0.4f);
            }
        }
        else
        {
            SpriteRenderer[] allChildren = ColorObj[3].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.HSVToRGB(0.2f, 0.2f, 0.2f), 0);
            }
        }
    }
}

