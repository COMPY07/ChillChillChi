using System;
using DG.Tweening;
using Manager;
using UnityEngine;
using UnityEngine.UI;

public class Room_Obj : MonoBehaviour
{
    [SerializeField] public AudioClip clip;

    [SerializeField] private GameObject[] ColorObj;
    public bool[] Room_Color;
    void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            Room_Color[i] = RoomManager._instance.OnColor[i];
        }
        if (Room_Color.Length > 0 && Room_Color[0]) // OnColor 배열이 비어있지 않은지 확인
        {
            SpriteRenderer[] allChildren = ColorObj[0].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.HSVToRGB(0.4f, 0.4f, 0.4f), 0);
            }
        }
        else
        {
            SpriteRenderer[] allChildren = ColorObj[0].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.white, 0.4f);
                

            }
        }
        if (Room_Color.Length > 0 && Room_Color[1]) // OnColor 배열이 비어있지 않은지 확인
        {
            SpriteRenderer[] allChildren = ColorObj[1].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.HSVToRGB(0.4f, 0.4f, 0.4f), 0);
            }
        }
        else
        {
            SpriteRenderer[] allChildren = ColorObj[1].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.white, 0.4f);
                

            }
        }
        if (Room_Color.Length > 0 && Room_Color[2]) // OnColor 배열이 비어있지 않은지 확인
        {
            SpriteRenderer[] allChildren = ColorObj[2].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.HSVToRGB(0.4f, 0.4f, 0.4f), 0);
            }
        }
        else
        {
            SpriteRenderer[] allChildren = ColorObj[2].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.white, 0.4f);
                

            }
        }
        if (Room_Color.Length > 0 && Room_Color[3]) // OnColor 배열이 비어있지 않은지 확인
        {
            SpriteRenderer[] allChildren = ColorObj[3].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.HSVToRGB(0.4f, 0.4f, 0.4f), 0);
            }
        }
        else
        {
            SpriteRenderer[] allChildren = ColorObj[3].GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer child in allChildren)
            {
                child.DOColor(Color.white, 0.4f);
                
            }
        }
    }


    public void OnEnable() {

        Manager.SoundManager.Instance.NextSoundAdd(0, new SoundClip(clip));
        Manager.SoundManager.Instance.NextSoundForce(0);
    }
}
