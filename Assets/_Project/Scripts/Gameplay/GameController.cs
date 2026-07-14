using BlockMerge.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    /// <summary>Builds the entire screen procedurally (flat colored squares, no art assets,
    /// no hand-authored scene) and wires it to a Core GameSession. Attach this to a single
    /// empty GameObject in an otherwise empty scene and press Play.
    ///
    /// Input model mirrors the HTML prototype: tap a tray piece to select it, tap a cell to
    /// place it. No drag gestures yet — that's a UI-polish pass, not part of this port.</summary>
    public sealed class GameController : MonoBehaviour
    {
        private static readonly Color BackgroundColor = new Color32(0x14, 0x16, 0x1c, 0xff);

        private GameSession _session;
        private GridView _gridView;
        private PieceTrayView _trayView;
        private HudView _hudView;
        private PowerUpBar _powerUpBar;
        private GameOverPanel _gameOverPanel;

        private int? _selectedTrayIndex;
        private PowerUpType? _armedPowerUp;

        private void Awake()
        {
            UiFactory.EnsureEventSystem();
            var canvas = UiFactory.CreateCanvas("Canvas");

            var background = canvas.gameObject.AddComponent<Image>();
            background.sprite = UiFactory.SolidSprite;
            background.color = BackgroundColor;

            var root = UiFactory.CreateRect("Root", canvas.transform);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            var rootLayout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(24, 24, 40, 40);
            rootLayout.spacing = 20f;
            rootLayout.childAlignment = TextAnchor.UpperCenter;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;

            _session = new GameSession();

            _hudView = HudView.Create(root);
            _gridView = GridView.Create(root, _session.Board.SizeValue, 968f);
            _trayView = PieceTrayView.Create(root, GameSession.TraySize);
            _powerUpBar = PowerUpBar.Create(root);

            _gameOverPanel = GameOverPanel.Create(canvas.transform);

            _gridView.CellClicked += OnCellClicked;
            _trayView.SlotClicked += OnTraySlotClicked;
            _powerUpBar.PowerUpChosen += OnPowerUpChosen;
            _gameOverPanel.RestartClicked += OnRestartClicked;

            RenderAll();
        }

        private void OnTraySlotClicked(int index)
        {
            if (_session.Tray[index] == null) return;
            _armedPowerUp = null;
            _selectedTrayIndex = _selectedTrayIndex == index ? (int?)null : index;
            RenderAll();
        }

        private void OnCellClicked(int row, int col)
        {
            if (_armedPowerUp.HasValue)
            {
                _session.UsePowerUp(_armedPowerUp.Value, new GridCoord(row, col));
                _armedPowerUp = null;
                RenderAll();
                return;
            }

            if (!_selectedTrayIndex.HasValue) return;

            var outcome = _session.PlacePiece(_selectedTrayIndex.Value, new GridCoord(row, col));
            if (outcome.Placed) _selectedTrayIndex = null;
            RenderAll();
        }

        private void OnPowerUpChosen(PowerUpType type)
        {
            _selectedTrayIndex = null;
            _armedPowerUp = type;
        }

        private void OnRestartClicked()
        {
            _session.Restart();
            _selectedTrayIndex = null;
            _armedPowerUp = null;
            _gameOverPanel.Hide();
            RenderAll();
        }

        private void RenderAll()
        {
            _gridView.Render(_session.Board);
            _trayView.Render(_session.Tray, _selectedTrayIndex);
            _hudView.SetScore(_session.Score);
            _hudView.SetBest(_session.Best);
            _hudView.SetMeter(_session.Meter.Value, ChargeMeter.Max);
            _powerUpBar.SetAvailable(_session.PowerUpAvailable);

            if (_session.IsGameOver) _gameOverPanel.Show(_session.Score, _session.Best);
        }
    }
}
