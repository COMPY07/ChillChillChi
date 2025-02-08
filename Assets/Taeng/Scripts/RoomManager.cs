using UnityEngine;
using DG.Tweening;

public class RoomManager : MonoBehaviour
{
    public static RoomManager _instance = null;
    public bool[] OnColor;
    public int start;
    
    [SerializeField] private string[] Description;

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
    }

    public void OnColor_Plus(int _bool)
    {
        OnColor[_bool] = true;
    }
}

