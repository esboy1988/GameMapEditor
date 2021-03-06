﻿using GameMapEditor.Frames;
using GameMapEditor.Objects;
using GameMapEditor.Objects.Controls;
using GameMapEditor.Objects.Enumerations;
using GameMapEditor.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace GameMapEditor
{
    public partial class MainFrame : Form, IDisposable
    {
        private static List<MapPanel> MapPanels = new List<MapPanel>();

        private ConsoleStreamWriter consoleStreamWriter;

        public MainFrame()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Redirection du flux Console vers ConsolePanel
            this.consoleStreamWriter = new ConsoleStreamWriter(ConsolePanel.Instance);
            Console.SetOut(this.consoleStreamWriter);

            // Chargement des panels
            this.LoadTilesetPanel();
            this.LoadConsolePanel();
            this.LoadBrowserPanel();
            this.LoadLayerPanel();

            // Création des liens
            MapPanelFrame.Instance.MapValidated += MapFrame_CreationValidated;

            TilesetPanel.Instance.TilesetSelectionChanged += TilesetPanel_TilesetSelectionChanged;
            TilesetPanel.Instance.TilesetChanged += TilesetPanel_TilesetChanged;

            LayerPanel.Instance.LayerPane.ItemRemoved += LayerPane_ItemRemoved;
            LayerPanel.Instance.LayerPane.ItemAdded += LayerPane_ItemAdded;
            LayerPanel.Instance.LayerPane.ItemsSwapped += LayerPane_ItemsSwapped;
            LayerPanel.Instance.LayerPane.ItemSelectionChanged += LayerPane_ItemSelectionChanged;
            LayerPanel.Instance.LayerPane.ItemTypeChanged += LayerPane_ItemTypeChanged;
            LayerPanel.Instance.LayerPane.ItemVisibleStateChanged += LayerPane_ItemVisibleStateChanged;
        }

        private void LayerPane_ItemVisibleStateChanged(object sender, int index, bool value)
        {
            if (this.ActiveMapPanel != null)
            {
                GameMapLayer layer = this.ActiveMapPanel.Map.GetLayerAt(index);

                if (layer != null)
                {
                    layer.Visible = value;
                }
                else throw new NullReferenceException("Tentative de modification du type d'un objet inexistant");
            }
            else throw new NullReferenceException("Tentative de modification d'un control enfant inexistant");
        }

        private void LayerPane_ItemTypeChanged(object sender, int index, LayerType type)
        {
            if (this.ActiveMapPanel != null)
            {
                GameMapLayer layer = this.ActiveMapPanel.Map.GetLayerAt(index);

                if (layer != null)
                {
                    layer.Type = type;
                }
                else throw new NullReferenceException("Tentative de modification du type d'un objet inexistant");
            }
            else throw new NullReferenceException("Tentative de modification d'un control enfant inexistant");
        }

        private void LayerPane_ItemSelectionChanged(object sender, int index)
        {
            if (this.ActiveMapPanel != null)
            {
                this.ActiveMapPanel.SelectedLayerIndex = index;
            }
            else throw new NullReferenceException("Tentative de selection d'un objet inexistant");
        }

        private void LayerPane_ItemsSwapped(object sender, int index1, int index2)
        {
            if (this.ActiveMapPanel != null)
            {
                this.ActiveMapPanel.Map.SwapLayers(index1, index2);
            }
            else throw new NullReferenceException("Tentative d'échange entre une ou plusieurs valeurs inexistantes");
        }

        private void LayerPane_ItemAdded(object sender, int index, GameMapLayer layer)
        {
            if (this.ActiveMapPanel != null)
            {
                this.ActiveMapPanel.Map.InsertLayerAt(index, layer);
            }
            else throw new NullReferenceException("Tentative d'ajout d'une ou plusieurs valeurs dans un control parent inexistant");
        }

        private void LayerPane_ItemRemoved(object sender, int index)
        {
            if (this.ActiveMapPanel != null)
            {
                this.ActiveMapPanel.Map.RemoveLayerAt(index);
            }
            else throw new NullReferenceException("Tentative de suppression d'une ou plusieurs valeurs dans un control parent inexistant");
        }

        /// <summary>
        /// Réinitialise la position de la fenêtre Tileset et l'affiche
        /// </summary>
        private void LoadTilesetPanel()
        {
            TilesetPanel.Instance.DockAreas = DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop | DockAreas.DockBottom;
            TilesetPanel.Instance.Show(this.DockPanel);
            TilesetPanel.Instance.DockTo(this.DockPanel, DockStyle.Left);
        }

        /// <summary>
        /// Réinitialise la position de la fenêtre Console et l'affiche
        /// </summary>
        private void LoadConsolePanel()
        {
            ConsolePanel.Instance.DockAreas = DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop | DockAreas.DockBottom;
            ConsolePanel.Instance.Show(this.DockPanel);
            ConsolePanel.Instance.DockTo(this.DockPanel, DockStyle.Bottom);
        }

        /// <summary>
        /// Réinitialise la position de la fenêtre Explorateur et l'affiche
        /// </summary>
        private void LoadBrowserPanel()
        {
            BrowserPanel.Instance.DockAreas = DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop | DockAreas.DockBottom;
            BrowserPanel.Instance.Show(DockPanel);
            BrowserPanel.Instance.DockTo(this.DockPanel, DockStyle.Right);
        }

        /// <summary>
        /// Réinitialise la position de la fenêtre de layers et l'affiche
        /// </summary>
        private void LoadLayerPanel()
        {
            LayerPanel.Instance.DockAreas = DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop | DockAreas.DockBottom;
            LayerPanel.Instance.Show(this.DockPanel);
            LayerPanel.Instance.DockTo(this.DockPanel, DockStyle.Right);
        }

        /// <summary>
        /// Libère les ressources associées
        /// </summary>
        private new void Dispose()
        {
            MapPanelFrame.Instance.MapValidated -= MapFrame_CreationValidated;

            TilesetPanel.Instance.TilesetSelectionChanged -= TilesetPanel_TilesetSelectionChanged;
            TilesetPanel.Instance.TilesetChanged -= TilesetPanel_TilesetChanged;

            LayerPanel.Instance.LayerPane.ItemRemoved -= LayerPane_ItemRemoved;
            LayerPanel.Instance.LayerPane.ItemsSwapped -= LayerPane_ItemsSwapped;
            LayerPanel.Instance.LayerPane.ItemSelectionChanged -= LayerPane_ItemSelectionChanged;
            LayerPanel.Instance.LayerPane.ItemTypeChanged -= LayerPane_ItemTypeChanged;

            MapPanels.ForEach(mapPanel => mapPanel.UndoRedoUpdated -= MapPanel_UndoRedoUpdated);
            this.Dispose(true);
        }

        private void MainFrame_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ExistsUnsavedDocuments)
            {
                DialogResult result = MessageBox.Show(this, "Tous les documents n'ont pas été sauvegardé !\nQuitter sans sauvegarder ?",
                    "Sauvegarde", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                if (result == DialogResult.OK)
                {
                    this.Dispose();
                    Environment.Exit(0);
                }
                else e.Cancel = true;
            }
            else
            {
                this.Dispose();
            }
        }

        private void TilesetPanel_TilesetChanged(object sender, TextureInfo texture)
        {
            MapPanels.ForEach(x => x.TextureInfo = texture);
        }

        private void TilesetPanel_TilesetSelectionChanged(object sender, Rectangle selection)
        {
            MapPanels.ForEach(x => x.TextureInfo.Selection = selection);
        }

        private void toolStripBtnFill_Click(object sender, EventArgs e)
        {
            this.ActiveMapPanel?.Fill();
        }

        private void MapFrame_CreationValidated(object sender, string mapName)
        {
            MapPanel mapPanel = new MapPanel(TilesetPanel.Instance.TilesetInfo, mapName);
            this.DockPanel.SuspendLayout();
            mapPanel.Show(this.DockPanel);
            mapPanel.DockState = DockState.Document;
            mapPanel.Dock = DockStyle.Fill;
            mapPanel.State = this.toolStripButtonErase.Checked ? GameEditorState.Erase : GameEditorState.Default;
            mapPanel.UndoRedoUpdated += MapPanel_UndoRedoUpdated;
            this.DockPanel.ResumeLayout(true, true);

            MapPanels.Add(mapPanel);
        }

        private void nouveauToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapPanelFrame.Instance.ShowDialog();
        }

        private void tilesetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LoadTilesetPanel();
        }

        private void explorateurToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LoadBrowserPanel();
        }

        private void couchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LoadLayerPanel();
        }

        private void consoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.LoadConsolePanel();
        }

        private void PanelToolsSaveCurrent_Click(object sender, EventArgs e)
        {
            // Sauvegarder map courante
            this.ActiveMapPanel?.Save();
            ConsolePanel.Instance.WriteLine("Carte de jeu enregistrée avec succés.", RowType.Information);
        }

        private void PanelToolsSaveAll_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.DockPanel.DocumentsCount > 0)
                {
                    // Sauvegarder tout
                    foreach (MapPanel document in this.DockPanel.Documents)
                        document.Save();
                    ConsolePanel.Instance.WriteLine("Cartes de jeu enregistrées avec succés.", RowType.Information);
                }
            }
            catch (Exception ex)
            {
                ConsolePanel.Instance.WriteLine(ex.Message);

            }
        }

        private async void ouvrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "FRoG Creator map (*.frog)|*.frog";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                IndefinedWaitingFrame waintingFrame = new IndefinedWaitingFrame();
                try
                {
                    waintingFrame.Location = new Point(this.Location.X + (this.Width - waintingFrame.Width) / 2,
                        this.Location.Y + (this.Height - waintingFrame.Height) / 2);
                    waintingFrame.Show(this);

                    GameMap map = await GameMap.Load(openFileDialog.FileName);

                    MapPanel mapPanel = new MapPanel(TilesetPanel.Instance.TilesetInfo, map);
                    this.DockPanel.SuspendLayout();
                    mapPanel.Show(this.DockPanel);
                    mapPanel.DockState = DockState.Document;
                    mapPanel.Dock = DockStyle.Fill;
                    mapPanel.State = this.toolStripButtonErase.Checked ? GameEditorState.Erase : GameEditorState.Default;
                    mapPanel.UndoRedoUpdated += MapPanel_UndoRedoUpdated;
                    this.DockPanel.ResumeLayout(true, true);

                    MapPanels.Add(mapPanel);

                    LayerPanel.Instance.LayerPane.Load(map.Layers);
                }
                catch (Exception ex)
                {
                    ConsolePanel.Instance.WriteLine(ex);
                }
                finally
                {
                    waintingFrame.Close();
                }
            }
        }

        private void toolStripButtonDestinationFolder_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", Path.GetFullPath("."));
        }

        private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Met à jour les états des boutons UI liés à la gestion d'historique
        /// </summary>
        /// <param name="manager">Le gestionnaire d'historique courant</param>
        private void RefreshUndoRedoButtonState(UndoRedoManager<MemoryStream> manager)
        {
            this.toolStripButtonUndo.Enabled = manager != null ? manager.CanUndo : false;
            this.toolStripButtonRedo.Enabled = manager != null ? manager.CanRedo : false;
            this.toolStripMenuItemUndo.Enabled = manager != null ? manager.CanUndo : false;
            this.toolStripMenuItemRedo.Enabled = manager != null ? manager.CanRedo : false;
        }

        private void DockPanel_ActiveDocumentChanged(object sender, EventArgs e)
        {
            if (this.ActiveMapPanel != null)
            {
                LayerPanel.Instance.Enabled = true;
                LayerPanel.Instance.LayerPane.Load(this.ActiveMapPanel.Map.Layers);
                this.RefreshUndoRedoButtonState(this.ActiveMapPanel.Manager);
                this.toolStripBtnFill.Enabled = true;
                this.toolStripButtonErase.Enabled = true;
                this.toolStripButtonSaveAll.Enabled = true;
                this.toolStripButtonSaveCurrent.Enabled = true;
            }
            else
            {
                LayerPanel.Instance.Reset();
                LayerPanel.Instance.Enabled = false;
                this.RefreshUndoRedoButtonState(null);
                this.toolStripBtnFill.Enabled = false;
                this.toolStripButtonErase.Enabled = false;
                this.toolStripButtonSaveAll.Enabled = false;
                this.toolStripButtonSaveCurrent.Enabled = false;
            }
        }

        private void toolStripButtonErase_Click(object sender, EventArgs e)
        {
            this.toolStripButtonErase.Checked = !this.toolStripButtonErase.Checked;
            foreach (MapPanel mapPanel in this.DockPanel.Documents)
            {
                mapPanel.State = this.toolStripButtonErase.Checked ? GameEditorState.Erase : GameEditorState.Default;
            }
        }

        private void MapPanel_UndoRedoUpdated(object sender, UndoRedoManager<MemoryStream> manager)
        {
            this.RefreshUndoRedoButtonState(manager);
        }

        private void toolStripButtonUndo_Click(object sender, EventArgs e)
        {
            MapPanel mapPanel = this.ActiveMapPanel;
            if (mapPanel != null && mapPanel.Manager.CanUndo)
            {
                mapPanel.Manager.Undo(mapPanel.Map.CopyToMemoryStream());
                LayerPanel.Instance.Refresh();
                this.RefreshUndoRedoButtonState(mapPanel.Manager);
            }
        }

        private void toolStripButtonRedo_Click(object sender, EventArgs e)
        {
            MapPanel mapPanel = this.ActiveMapPanel;
            if (mapPanel != null && mapPanel.Manager.CanRedo)
            {
                mapPanel.Manager.Redo(mapPanel.Map.CopyToMemoryStream());
                this.RefreshUndoRedoButtonState(mapPanel.Manager);
            }
        }

        /// <summary>
        /// Obtient la valeur informant s'il existe des documents ouverts non sauvegardés
        /// </summary>
        private bool ExistsUnsavedDocuments
        {
            get
            {
                foreach (MapPanel mapPanel in DockPanel.Documents)
                {
                    if (!mapPanel.IsSaved)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Obtient le panel d'édition de carte actif
        /// </summary>
        private MapPanel ActiveMapPanel
        {
            get { return this.DockPanel.ActiveDocument as MapPanel; }
        }
    }
}
