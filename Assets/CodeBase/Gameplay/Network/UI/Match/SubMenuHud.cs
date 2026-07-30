using System;
using System.ComponentModel;
using CodeBase.Infrastructure.Services.SceneLoader;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.CodeBase.Gameplay.Network.UI.Match
{
    public class SubMenuHud: MonoBehaviour
    {
        private const string SceneName = "MainMenu";
        
        [SerializeField] private Button menuButton;
        [SerializeField] private GameObject menuContainer;

        [SerializeField] private Button continueButton;
        [SerializeField] private Button lobbyButton;
        [SerializeField] private Button exitButton;
        
        private ISceneLoader _sceneLoader;

        [Inject]
        private void Construct(ISceneLoader SceneLoader)
        {
            _sceneLoader = SceneLoader;
        }
        
        private void Start()
        {
            menuButton.onClick.AddListener(Show);
            
            continueButton.onClick.AddListener(HandleContinue);
            lobbyButton.onClick.AddListener(HandleLobby);
            exitButton.onClick.AddListener(HandleExit);
            
            Hide();
        }

        private void OnDestroy()
        {
            menuButton.onClick.RemoveListener(Show);
            
            continueButton.onClick.RemoveListener(HandleContinue);
            lobbyButton.onClick.RemoveListener(HandleLobby);
            exitButton.onClick.RemoveListener(HandleExit);
        }

        private void HandleContinue()
        {
            Hide();
        }

        private void HandleLobby()
        {
            _sceneLoader.LoadSceneAsync(SceneName);
        }

        private void HandleExit()
        {
            Application.Quit();
        }

        private void Show()
        {
            menuContainer.SetActive(true);
        }

        private void Hide()
        {
            menuContainer.SetActive(false);
        }
    }
}