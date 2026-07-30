using CodeBase.CodeBase.Infrastructure.Services.StaticData;
using CodeBase.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace CodeBase.Gameplay.Network.UI
{
    public sealed class LevelSelectorUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Dropdown levelDropdown;
        [SerializeField] private Image levelPreviewImage;
        [SerializeField] private TMP_Text levelNameText;

        private LobbySessionService _lobbyService;
        private IStaticDataService _staticDataService;

        [Inject]
        private void Construct(
            LobbySessionService lobbyService,
            IStaticDataService staticDataService)
        {
            _lobbyService = lobbyService;
            _staticDataService = staticDataService;
        }

        public void Init()
        {
            var levels = _staticDataService.LevelsRegistry.Levels;

            levelDropdown.ClearOptions();

            foreach (var level in levels)
            {
                levelDropdown.options.Add(new TMP_Dropdown.OptionData(level.DisplayName));
            }

            int defaultIndex = levels.IndexOf(_staticDataService.LevelsRegistry.Levels[0]);
            if (defaultIndex < 0) defaultIndex = 0;
            
            levelDropdown.value = defaultIndex;
            levelDropdown.RefreshShownValue();
            levelDropdown.onValueChanged.AddListener(OnLevelSelected);
            OnLevelSelected(defaultIndex);

            UpdatePreview(_staticDataService.LevelsRegistry.Levels[0]);
        }

        private void OnDestroy()
        {
            if (levelDropdown != null)
                levelDropdown.onValueChanged.RemoveListener(OnLevelSelected);
        }

        private void OnLevelSelected(int index)
        {
            // if (_staticDataService.LevelsRegistry == null || index < 0 || index >= _staticDataService.LevelsRegistry.Levels.Count)
            //     return;

            var level = _staticDataService.LevelsRegistry.Levels[index];
            _lobbyService.SetSelectedLevel(level);
            UpdatePreview(level);
        }

        private void UpdatePreview(LevelConfig level)
        {
            if (level == null) return;

            if (levelPreviewImage != null)
            {
                levelPreviewImage.sprite = level.PreviewImage;
                levelPreviewImage.enabled = level.PreviewImage != null;
            }

            if (levelNameText != null)
                levelNameText.text = level.DisplayName;
        }
    }
}
