using System.Collections.Generic;
using BlockMerge.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    /// <summary>Builds the entire screen procedurally (flat colored squares, no art assets,
    /// no hand-authored scene) and wires it to a Core GameSession. Attach this to a single
    /// empty GameObject in an otherwise empty scene and press Play.
    ///
    /// Input: drag a tray piece onto the board to place it (a floating ghost follows the
    /// finger, lifted above it so it stays visible, with a live valid/invalid preview on the
    /// grid). Tap-to-target is still used for armed power-ups, since that's a single-cell
    /// pick rather than a placement.</summary>
    public sealed class GameController : MonoBehaviour
    {
        private static readonly Color BackgroundColor = new Color32(0x14, 0x16, 0x1c, 0xff);
        private static readonly Color ChargedBackgroundColor = new Color32(0x2a, 0x1c, 0x10, 0xff);
        private const float DragLiftOffset = 220f;
        private const float GhostCellSize = 96f;
        private const float GhostSpacing = 4f;

        private GameSession _session;
        private GridView _gridView;
        private PieceTrayView _trayView;
        private HudView _hudView;
        private PowerUpBar _powerUpBar;
        private GameOverPanel _gameOverPanel;
        private SfxPlayer _sfx;
        private Image _background;

        private Transform _canvasTransform;
        private RectTransform _dragGhost;
        private int _dragGhostBoundsRows;
        private int _dragGhostBoundsCols;
        private int _draggingIndex = -1;
        private Piece _draggingPiece;
        private PowerUpType? _armedPowerUp;
        private int _lastSavedBest;

        private void Awake()
        {
            UiFactory.EnsureEventSystem();
            var canvas = UiFactory.CreateCanvas("Canvas");
            _canvasTransform = canvas.transform;

            _background = canvas.gameObject.AddComponent<Image>();
            _background.sprite = UiFactory.SolidSprite;
            _background.color = BackgroundColor;

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

            _lastSavedBest = BestScoreStore.Load();
            _session = new GameSession(startingBest: _lastSavedBest);
            _sfx = SfxPlayer.Create(transform);

            _hudView = HudView.Create(root);
            _gridView = GridView.Create(root, _session.Board.SizeValue, 968f);
            _trayView = PieceTrayView.Create(root, GameSession.TraySize);
            _powerUpBar = PowerUpBar.Create(root);

            AmbientEmbers.Create(canvas.transform);

            _gameOverPanel = GameOverPanel.Create(canvas.transform);

            _gridView.CellClicked += OnCellClicked;
            _trayView.DragStarted += OnTrayDragStarted;
            _trayView.DragMoved += OnTrayDragMoved;
            _trayView.DragEnded += OnTrayDragEnded;
            _powerUpBar.PowerUpChosen += OnPowerUpChosen;
            _gameOverPanel.RestartClicked += OnRestartClicked;

            RenderAllImmediate();
        }

        private void Update()
        {
            _session.Tick(Time.deltaTime);
        }

        private void OnTrayDragStarted(int index, PointerEventData eventData)
        {
            var piece = _session.Tray[index];
            if (piece == null) return;

            _armedPowerUp = null;
            _draggingIndex = index;
            _draggingPiece = piece;
            _trayView.Render(_session.Tray, _draggingIndex);
            CreateDragGhost(piece);
            UpdateGhostPosition(eventData);
            UpdatePreview();
        }

        private void OnTrayDragMoved(PointerEventData eventData)
        {
            if (_draggingPiece == null) return;
            UpdateGhostPosition(eventData);
            UpdatePreview();
        }

        private void OnTrayDragEnded(int index, PointerEventData eventData)
        {
            UpdateGhostPosition(eventData);
            _gridView.ClearPlacementPreview();

            bool placed = false;
            if (_draggingPiece != null && TryGetHoveredAnchor(out var anchor) && _session.CanPlace(index, anchor))
            {
                CompletePlacement(index, anchor);
                placed = true;
            }

            DestroyDragGhost();
            _draggingIndex = -1;
            _draggingPiece = null;

            if (!placed)
            {
                _trayView.Render(_session.Tray, null);
                _trayView.PlayInvalidDropFeedback(index);
            }
        }

        private void CompletePlacement(int index, GridCoord anchor)
        {
            var pieceColor = BlockColorPalette.ToColor(_session.Tray[index].Color);
            var outcome = _session.PlacePiece(index, anchor);
            if (!outcome.Placed) return;

            _sfx.Play(SfxPlayer.SfxKind.Place);

            foreach (var coord in outcome.PlacedCells)
            {
                var cell = _gridView.GetCell(coord.Row, coord.Col);
                cell.SetColor(pieceColor);
                cell.PlayPlacementPop();
            }

            if (outcome.LineClear.AnyLinesCleared)
            {
                var mergedSet = new HashSet<GridCoord>(outcome.LineClear.MergedCells);
                foreach (var coord in outcome.LineClear.ClearedCells)
                {
                    var cell = _gridView.GetCell(coord.Row, coord.Col);
                    if (mergedSet.Contains(coord))
                    {
                        PlayMergeBurst(cell);
                        cell.PlayMergeAndEmpty();
                    }
                    else
                    {
                        cell.PlayClearAndEmpty();
                    }
                }

                // Pitch and flash intensity both scale with combo heat, so a long chain reads
                // as faster and bigger even out of the corner of an eye, not just a number.
                float comboHeat = Mathf.Clamp01((outcome.ComboCount - 1) / 6f);
                float pitch = Mathf.Lerp(1f, 1.6f, comboHeat);

                _sfx.Play(SfxPlayer.SfxKind.Clear, pitch);
                if (outcome.LineClear.MergedGroups.Count > 0) _sfx.Play(SfxPlayer.SfxKind.Merge, pitch);

                if (outcome.ComboCount >= 2)
                    ScreenFlashEffect.Play(this, _canvasTransform, new Color32(0xff, 0x8a, 0x3d, 0xff), Mathf.Lerp(0.12f, 0.35f, comboHeat));

                PlayScorePopups(outcome.LineClear.ClearedCells, outcome.ScoreAwarded, outcome.ComboCount);
            }

            RefreshNonGridUi(outcome.GameOver);
        }

        private void PlayScorePopups(IReadOnlyList<GridCoord> cells, int amount, int comboCount)
        {
            if (cells.Count == 0) return;

            float sumRow = 0f, sumCol = 0f;
            foreach (var c in cells)
            {
                sumRow += c.Row;
                sumCol += c.Col;
            }

            int size = _session.Board.SizeValue;
            int avgRow = Mathf.Clamp(Mathf.RoundToInt(sumRow / cells.Count), 0, size - 1);
            int avgCol = Mathf.Clamp(Mathf.RoundToInt(sumCol / cells.Count), 0, size - 1);

            var cell = _gridView.GetCell(avgRow, avgCol);
            Vector2 screenPoint = cell.RectTransform.position;
            var canvasRect = (RectTransform)_canvasTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out var localPoint)) return;

            ScorePopupEffect.Play(this, _canvasTransform, localPoint, amount);
            ComboPopupEffect.Play(this, _canvasTransform, localPoint, comboCount);
        }

        private void PlayMergeBurst(CellView cell)
        {
            Vector2 screenPoint = cell.RectTransform.position;
            var canvasRect = (RectTransform)_canvasTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out var localPoint))
                BurstEffect.Play(this, _canvasTransform, localPoint, cell.CurrentColor);
        }

        private void OnCellClicked(int row, int col)
        {
            if (!_armedPowerUp.HasValue) return;

            var affected = _session.UsePowerUp(_armedPowerUp.Value, new GridCoord(row, col));
            foreach (var coord in affected)
                _gridView.GetCell(coord.Row, coord.Col).PlayClearAndEmpty();
            if (affected.Count > 0) _sfx.Play(SfxPlayer.SfxKind.PowerUp);

            _armedPowerUp = null;
            RefreshNonGridUi(_session.IsGameOver);
        }

        private void OnPowerUpChosen(PowerUpType type)
        {
            CancelDrag();
            _armedPowerUp = type;
        }

        private void OnRestartClicked()
        {
            CancelDrag();
            _armedPowerUp = null;
            _session.Restart();
            _gameOverPanel.Hide();
            RenderAllImmediate();
        }

        private void CancelDrag()
        {
            if (_draggingIndex < 0) return;
            _gridView.ClearPlacementPreview();
            DestroyDragGhost();
            _draggingIndex = -1;
            _draggingPiece = null;
            _trayView.Render(_session.Tray, null);
        }

        private void CreateDragGhost(Piece piece)
        {
            var ghost = UiFactory.CreateRect("DragGhost", _canvasTransform);
            ghost.anchorMin = ghost.anchorMax = new Vector2(0.5f, 0.5f);
            ghost.pivot = new Vector2(0.5f, 0.5f);

            int maxRow = 0, maxCol = 0;
            foreach (var cell in piece.Cells)
            {
                maxRow = Mathf.Max(maxRow, cell.Row);
                maxCol = Mathf.Max(maxCol, cell.Col);
            }

            float width = (maxCol + 1) * GhostCellSize + maxCol * GhostSpacing;
            float height = (maxRow + 1) * GhostCellSize + maxRow * GhostSpacing;
            var color = BlockColorPalette.ToColor(piece.Color);

            foreach (var offset in piece.Cells)
            {
                var cellRect = UiFactory.CreateRect("GhostCell", ghost);
                cellRect.sizeDelta = new Vector2(GhostCellSize, GhostCellSize);
                cellRect.anchorMin = cellRect.anchorMax = new Vector2(0.5f, 0.5f);
                float x = -width / 2f + GhostCellSize / 2f + offset.Col * (GhostCellSize + GhostSpacing);
                float y = height / 2f - GhostCellSize / 2f - offset.Row * (GhostCellSize + GhostSpacing);
                cellRect.anchoredPosition = new Vector2(x, y);

                var image = cellRect.gameObject.AddComponent<Image>();
                image.sprite = UiFactory.RoundedSprite;
                image.type = Image.Type.Sliced;
                image.color = color;
                image.raycastTarget = false;
            }

            _dragGhost = ghost;
            _dragGhostBoundsRows = maxRow + 1;
            _dragGhostBoundsCols = maxCol + 1;
        }

        private void DestroyDragGhost()
        {
            if (_dragGhost != null) Destroy(_dragGhost.gameObject);
            _dragGhost = null;
        }

        private void UpdateGhostPosition(PointerEventData eventData)
        {
            if (_dragGhost == null) return;
            var canvasRect = (RectTransform)_canvasTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, null, out var localPoint);
            _dragGhost.anchoredPosition = localPoint + new Vector2(0f, DragLiftOffset);
        }

        private bool TryGetHoveredAnchor(out GridCoord anchor)
        {
            anchor = default;
            if (_dragGhost == null) return false;

            Vector2 screenPoint = _dragGhost.position;
            if (!_gridView.TryGetCellAtScreenPoint(screenPoint, null, out int row, out int col)) return false;

            anchor = new GridCoord(row - _dragGhostBoundsRows / 2, col - _dragGhostBoundsCols / 2);
            return true;
        }

        private void UpdatePreview()
        {
            if (_draggingPiece == null) return;

            if (TryGetHoveredAnchor(out var anchor))
            {
                bool valid = _session.Board.CanPlace(_draggingPiece, anchor);
                _gridView.ShowPlacementPreview(_draggingPiece, anchor, valid);
            }
            else
            {
                _gridView.ClearPlacementPreview();
            }
        }

        private void RenderAllImmediate()
        {
            _gridView.Render(_session.Board);
            _trayView.Render(_session.Tray, null);
            _hudView.SetScore(_session.Score);
            _hudView.SetBest(_session.Best);
            _hudView.SetMeter(_session.Meter.Value, ChargeMeter.Max);
            _powerUpBar.SetAvailable(_session.PowerUpAvailable);
            UpdateBackgroundTint();
            SaveBestScoreIfImproved();
        }

        private void RefreshNonGridUi(bool gameOver)
        {
            _trayView.Render(_session.Tray, null);
            _hudView.SetScoreAnimated(_session.Score);
            _hudView.SetBest(_session.Best);
            _hudView.SetMeterAnimated(_session.Meter.Value, ChargeMeter.Max);
            _powerUpBar.SetAvailable(_session.PowerUpAvailable);
            UpdateBackgroundTint();
            SaveBestScoreIfImproved();

            if (gameOver)
            {
                _sfx.Play(SfxPlayer.SfxKind.GameOver);
                _gameOverPanel.Show(_session.Score, _session.Best);
            }
        }

        private void SaveBestScoreIfImproved()
        {
            if (_session.Best <= _lastSavedBest) return;
            _lastSavedBest = _session.Best;
            BestScoreStore.Save(_lastSavedBest);
        }

        /// <summary>Ties the whole screen's mood to how charged up the meter is — subtly
        /// warms from the base dark-blue toward an ember tone as charge builds.</summary>
        private void UpdateBackgroundTint()
        {
            float fraction = Mathf.Clamp01((float)_session.Meter.Value / ChargeMeter.Max);
            _background.color = Color.Lerp(BackgroundColor, ChargedBackgroundColor, fraction);
        }
    }
}
