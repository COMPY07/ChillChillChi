using System;
using Manager;
using Newtonsoft.Json.Linq;
using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;

namespace Menu
{
    public class GameStartMenu : MonoBehaviour
    {
        private GameStartAnimation animation;
        
        public void Awake()
        {
            animation = this.gameObject.GetComponent<GameStartAnimation>();
            
        }

        public void Start() {
            
            animation.Animation();
            Invoke("Send", 5f);
        }

        public void Send()
        {

        }
        
        
    }
}