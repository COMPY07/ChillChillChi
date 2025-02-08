using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BaseStage : MonoBehaviour
{

        public string stageName;
        
        
        [Header("Setting")]
        [SerializeField] protected GeneratorSetting setting;
        [SerializeField] protected StageSpawnData spawnData;
        
        [Header("Generation Type")] [SerializeField]
        private GenerationType type = GenerationType.BSP;
                
        
        protected BSPGenerator bsp;

        protected List<GameObject> spawnObjects;
        
        
        public void Awake() {
                if (setting == null)
                        Debug.LogError("GeneratorSetting is null!");

                spawnObjects = new List<GameObject>();
                
                bsp = this.AddComponent<BSPGenerator>();
                bsp.GeneratorInit(setting);
        }

        protected void Generate() {
                bsp.GenerateMap();
        }

        public virtual void Begin() {
                SpawnPair[] data = this.spawnData.spawnPairs;
                foreach(SpawnPair sp in data) {
                        GameObject prefabs = sp.prefab;
                        SpawnSetting setting = sp.spawnSetting;

                        spawnObjects.Add(bsp.SpawnMonster(prefabs, setting));
                }
        }
        
        public virtual void End() {
                foreach (var obj in spawnObjects) {
                        if (obj == null) continue;
                        Destroy(obj);
                }
                spawnObjects.Clear();
                
                
                StageManager.Instance.EndStage();
        }

        public override string ToString()
        {
                return stageName;
        }

        public bool ValidPosition(Vector3 pos) {
                return bsp.GetRoomFromPosition(pos) != null;
        }
        
        
        
}