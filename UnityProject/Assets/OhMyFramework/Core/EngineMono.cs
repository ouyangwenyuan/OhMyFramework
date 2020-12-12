using System;
using UnityEngine;

namespace OhMyFramework.Core
{
    public class EngineMono : MonoBehaviour
    {
        private ModuleManager _moduleManager;

        public ModuleManager ModuleManager
        {
            get => _moduleManager;
            set => _moduleManager = value;
        }

        private void Awake()
        {
            _moduleManager = new ModuleManager();
            _moduleManager.Awake();
            LogModule.Log($"Awake {GetType().Name}");
        }

        private void FixedUpdate()
        {
            _moduleManager.FixedUpdate();
        }

        private void LateUpdate()
        {
            _moduleManager.LateUpdate();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            _moduleManager.OnApplicationPause(pauseStatus);
        }

        private void OnApplicationQuit()
        {
            _moduleManager.OnApplicationQuit();
        }

        private void OnDestroy()
        {
            _moduleManager.OnDestroy();
        }

        private void Start()
        {
            _moduleManager.Start();
            LogModule.Log($"Start {GetType().Name}");
        }

        private void Update()
        {
            _moduleManager.Update();
        }
    }
}