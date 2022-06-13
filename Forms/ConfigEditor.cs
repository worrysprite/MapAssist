﻿using MapAssist.Helpers;
using MapAssist.Settings;
using MapAssist.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapAssist
{
    public partial class ConfigEditor : Form
    {
        private bool formReady = false;
        private bool formShown = false;
        private CancellationTokenSource formShownCancelToken;

        private PropertyInfo SelectedProperty;
        private AddAreaForm areaForm;

        public ConfigEditor()
        {
            formReady = false;
            InitializeComponent();

            var propertyList = MapAssistConfiguration.Loaded.MapConfiguration.GetType().GetProperties();
            foreach (var property in propertyList)
            {
                cboRenderOption.Items.Add(property.Name.ToProperCase());
            }

            foreach (var element in Enum.GetNames(typeof(BuffPosition)))
            {
                cboBuffPosition.Items.Add(element.ToProperCase());
            }

            foreach (var element in Enum.GetNames(typeof(MapPosition)))
            {
                cboPosition.Items.Add(element.ToProperCase());
            }

            foreach (var element in Enum.GetNames(typeof(Shape)))
            {
                cboIconShape.Items.Add(element);
            }

            foreach (Locale element in Enum.GetValues(typeof(Locale)))
            {
                cboLanguage.Items.Add(LocaleExtensions.Name(element));
            }

            foreach (var element in Enum.GetNames(typeof(GameInfoPosition)))
            {
                cboGameInfoPosition.Items.Add(element.ToProperCase());
                cboItemLogPosition.Items.Add(element.ToProperCase());
            }

            cboLanguage.SelectedIndex = (int)MapAssistConfiguration.Loaded.LanguageCode;

            opacity.Value = (int)Math.Round(MapAssistConfiguration.Loaded.RenderingConfiguration.Opacity * 100d / 5);
            lblOpacityValue.Text = (opacity.Value * 5).ToString();

            allIconOpacity.Value = (int)Math.Round(MapAssistConfiguration.Loaded.RenderingConfiguration.IconOpacity * 100d / 5);
            lblAllIconOpacityValue.Text = (allIconOpacity.Value * 5).ToString();

            mapSize.Value = (int)Math.Round(MapAssistConfiguration.Loaded.RenderingConfiguration.Size / 100d);
            lblMapSizeValue.Text = (mapSize.Value * 100).ToString();

            mapZoom.Value = zoomToTick(MapAssistConfiguration.Loaded.RenderingConfiguration.ZoomLevel);
            lblMapZoomValue.Text = MapAssistConfiguration.Loaded.RenderingConfiguration.ZoomLevel.ToString();

            chkOverlayMode.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.OverlayMode;
            chkMonsterHealthBar.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.MonsterHealthBar;
            chkToggleViaMap.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.ToggleViaInGameMap;
            chkToggleViaPanels.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.ToggleViaInGamePanels;
            chkStickToLastGameWindow.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.StickToLastGameWindow;
            cboPosition.SelectedIndex = cboPosition.FindStringExact(MapAssistConfiguration.Loaded.RenderingConfiguration.Position.ToString().ToProperCase());

            buffSize.Value = (int)Math.Round(MapAssistConfiguration.Loaded.RenderingConfiguration.BuffSize * 10d);
            lblBuffSizeValue.Text = MapAssistConfiguration.Loaded.RenderingConfiguration.BuffSize.ToString();
            chkBuffs.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.ShowBuffBarBuffs;
            chkAuras.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.ShowBuffBarAuras;
            chkPassives.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.ShowBuffBarPassives;
            chkDebuffs.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.ShowBuffBarDebuffs;
            chkAlertLowerRes.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.BuffAlertLowRes;
            cboBuffPosition.SelectedIndex = cboBuffPosition.FindStringExact(MapAssistConfiguration.Loaded.RenderingConfiguration.BuffPosition.ToString().ToProperCase());

            chkLinesHostiles.Checked = MapAssistConfiguration.Loaded.MapConfiguration.HostilePlayer.CanDrawLine();
            chkLinesCorpse.Checked = MapAssistConfiguration.Loaded.MapConfiguration.Corpse.CanDrawLine();
            chkLinesNextArea.Checked = MapAssistConfiguration.Loaded.MapConfiguration.NextArea.CanDrawLine();
            chkLinesQuest.Checked = MapAssistConfiguration.Loaded.MapConfiguration.Quest.CanDrawLine();
            chkLinesWaypoint.Checked = MapAssistConfiguration.Loaded.MapConfiguration.Waypoint.CanDrawLine();
            chkLinesShrines.Checked = MapAssistConfiguration.Loaded.MapConfiguration.Shrine.CanDrawLine();

            chkLife.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.ShowLife;
            chkMana.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.ShowMana;
            chkLifePerc.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.ShowLifePerc;
            chkManaPerc.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.ShowManaPerc;
            chkCurrentLevel.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.ShowCurrentLevel;
            chkExpProgress.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.ShowExpProgress;
            chkPotionBelt.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.ShowPotionBelt;
            chkResistances.Checked = MapAssistConfiguration.Loaded.RenderingConfiguration.ShowResistances;

            chkShowGameName.Checked = MapAssistConfiguration.Loaded.GameInfo.ShowGameName;
            chkShowGameTimer.Checked = MapAssistConfiguration.Loaded.GameInfo.ShowGameTimer;
            chkShowAreaTimer.Checked = MapAssistConfiguration.Loaded.GameInfo.ShowAreaTimer;
            chkShowArea.Checked = MapAssistConfiguration.Loaded.GameInfo.ShowArea;
            chkShowAreaLevel.Checked = MapAssistConfiguration.Loaded.GameInfo.ShowAreaLevel;
            chkShowDifficulty.Checked = MapAssistConfiguration.Loaded.GameInfo.ShowDifficulty;
            chkShowArea.Checked = MapAssistConfiguration.Loaded.GameInfo.ShowAreaLevel;
            txtD2Path.Text = MapAssistConfiguration.Loaded.D2LoDPath;
            chkGameInfoTextShadow.Checked = MapAssistConfiguration.Loaded.GameInfo.LabelTextShadow;
            btnClearGameInfoFont.Visible = MapAssistConfiguration.Loaded.GameInfo.LabelFont != MapAssistConfiguration.Default.GameInfo.LabelFont ||
                MapAssistConfiguration.Loaded.GameInfo.LabelFontSize != MapAssistConfiguration.Default.GameInfo.LabelFontSize;
            chkShowOverlayFPS.Checked = MapAssistConfiguration.Loaded.GameInfo.ShowOverlayFPS;
            cboGameInfoPosition.SelectedIndex = cboGameInfoPosition.FindStringExact(MapAssistConfiguration.Loaded.GameInfo.Position.ToString().ToProperCase());

            new Hotkey(MapAssistConfiguration.Loaded.HotkeyConfiguration.ToggleKey.ToString()).Monitor(txtToggleMapKey);
            new Hotkey(MapAssistConfiguration.Loaded.HotkeyConfiguration.HideMapKey.ToString()).Monitor(txtHideMapKey);
            new Hotkey(MapAssistConfiguration.Loaded.HotkeyConfiguration.MapPositionsKey.ToString()).Monitor(txtMapPositionsKey);
            new Hotkey(MapAssistConfiguration.Loaded.HotkeyConfiguration.ZoomInKey.ToString()).Monitor(txtZoomInKey);
            new Hotkey(MapAssistConfiguration.Loaded.HotkeyConfiguration.ZoomOutKey.ToString()).Monitor(txtZoomOutKey);
            new Hotkey(MapAssistConfiguration.Loaded.HotkeyConfiguration.ExportItemsKey.ToString()).Monitor(txtExportItemsKey);
            new Hotkey(MapAssistConfiguration.Loaded.HotkeyConfiguration.ToggleConfigKey.ToString()).Monitor(txtToggleConfigKey);

            cboItemLogPosition.SelectedIndex = cboItemLogPosition.FindStringExact(MapAssistConfiguration.Loaded.ItemLog.Position.ToString().ToProperCase());
            chkItemLogEnabled.Checked = MapAssistConfiguration.Loaded.ItemLog.Enabled;
            chkItemLogItemsOnIdentify.Checked = MapAssistConfiguration.Loaded.ItemLog.CheckItemOnIdentify;
            chkItemLogVendorItems.Checked = MapAssistConfiguration.Loaded.ItemLog.CheckVendorItems;
            chkShowDistanceToItem.Checked = MapAssistConfiguration.Loaded.ItemLog.ShowDistanceToItem;
            chkShowDirectionToItem.Checked = MapAssistConfiguration.Loaded.ItemLog.ShowDirectionToItem;
            chkPlaySound.Checked = MapAssistConfiguration.Loaded.ItemLog.PlaySoundOnDrop;
            txtFilterFile.Text = MapAssistConfiguration.Loaded.ItemLog.FilterFileName;
            soundVolume.Value = (int)Math.Round(MapAssistConfiguration.Loaded.ItemLog.SoundVolume / 5d);
            lblSoundVolumeValue.Text = $"{soundVolume.Value * 5}";
            itemDisplayForSeconds.Value = (int)Math.Round(MapAssistConfiguration.Loaded.ItemLog.DisplayForSeconds / 5d);
            lblItemDisplayForSecondsValue.Text = $"{itemDisplayForSeconds.Value * 5} s";
            btnClearLogFont.Visible = MapAssistConfiguration.Loaded.ItemLog.LabelFont != MapAssistConfiguration.Default.ItemLog.LabelFont ||
                MapAssistConfiguration.Loaded.ItemLog.LabelFontSize != MapAssistConfiguration.Default.ItemLog.LabelFontSize;
            chkLogTextShadow.Checked = MapAssistConfiguration.Loaded.ItemLog.LabelTextShadow;

            btnSuperiorColor.BackColor = MapAssistConfiguration.Loaded.ItemLog.SuperiorColor;
            btnSuperiorColor.ForeColor = ContrastTextColor(btnSuperiorColor.BackColor);
            btnClearSuperiorColor.Visible = MapAssistConfiguration.Loaded.ItemLog.SuperiorColor != MapAssistConfiguration.Default.ItemLog.SuperiorColor;

            btnMagicColor.BackColor = MapAssistConfiguration.Loaded.ItemLog.MagicColor;
            btnMagicColor.ForeColor = ContrastTextColor(btnMagicColor.BackColor);
            btnClearMagicColor.Visible = MapAssistConfiguration.Loaded.ItemLog.MagicColor != MapAssistConfiguration.Default.ItemLog.MagicColor;

            btnRareColor.BackColor = MapAssistConfiguration.Loaded.ItemLog.RareColor;
            btnRareColor.ForeColor = ContrastTextColor(btnRareColor.BackColor);
            btnClearRareColor.Visible = MapAssistConfiguration.Loaded.ItemLog.RareColor != MapAssistConfiguration.Default.ItemLog.RareColor;

            btnSetColor.BackColor = MapAssistConfiguration.Loaded.ItemLog.SetColor;
            btnSetColor.ForeColor = ContrastTextColor(btnSetColor.BackColor);
            btnClearSetColor.Visible = MapAssistConfiguration.Loaded.ItemLog.SetColor != MapAssistConfiguration.Default.ItemLog.SetColor;

            btnUniqueColor.BackColor = MapAssistConfiguration.Loaded.ItemLog.UniqueColor;
            btnUniqueColor.ForeColor = ContrastTextColor(btnUniqueColor.BackColor);
            btnClearUniqueColor.Visible = MapAssistConfiguration.Loaded.ItemLog.UniqueColor != MapAssistConfiguration.Default.ItemLog.UniqueColor;

            btnCraftedColor.BackColor = MapAssistConfiguration.Loaded.ItemLog.CraftedColor;
            btnCraftedColor.ForeColor = ContrastTextColor(btnCraftedColor.BackColor);
            btnClearCraftedColor.Visible = MapAssistConfiguration.Loaded.ItemLog.CraftedColor != MapAssistConfiguration.Default.ItemLog.CraftedColor;

            if (MapAssistConfiguration.Loaded.MapColorConfiguration.Walkable != null)
            {
                var color = (Color)MapAssistConfiguration.Loaded.MapColorConfiguration.Walkable;
                btnWalkableColor.BackColor = color;
                btnWalkableColor.ForeColor = ContrastTextColor(color);
                btnClearWalkableColor.Visible = color.A > 0;
            }
            else
            {
                btnClearWalkableColor.Visible = false;
            }

            if (MapAssistConfiguration.Loaded.MapColorConfiguration.Border != null)
            {
                var color = (Color)MapAssistConfiguration.Loaded.MapColorConfiguration.Border;
                btnBorderColor.BackColor = color;
                btnBorderColor.ForeColor = ContrastTextColor(color);
                btnClearBorderColor.Visible = color.A > 0;
            }
            else
            {
                btnClearBorderColor.Visible = false;
            }

            if (MapAssistConfiguration.Loaded.MapColorConfiguration.ExpRange != null)
            {
                var color = (Color)MapAssistConfiguration.Loaded.MapColorConfiguration.ExpRange;
                btnExpRangeColor.BackColor = color;
                btnExpRangeColor.ForeColor = ContrastTextColor(color);
                btnClearExpRangeColor.Visible = color.A > 0;
            }
            else
            {
                btnClearExpRangeColor.Visible = false;
            }

            foreach (var area in MapAssistConfiguration.Loaded.HiddenAreas)
            {
                lstHidden.Items.Add(area.Name());
            }

            foreach (var authorizedWindowTitle in MapAssistConfiguration.Loaded.AuthorizedWindowTitles)
            {
                lstAuthorizedWindowTitle.Items.Add(authorizedWindowTitle);
            }

            chkDPIAware.Checked = MapAssistConfiguration.Loaded.DPIAware;

            var filePaths = Directory.GetFiles(System.IO.Path.Combine(Application.StartupPath, "Sounds"), @"*.wav");
            foreach (var filePath in filePaths)
            {
                var fileName = System.IO.Path.GetFileName(filePath);
                cboItemLogSound.Items.Add(fileName);
            }
            cboItemLogSound.Text = MapAssistConfiguration.Loaded.ItemLog.SoundFile;

            if (cboRenderOption.Items.Count > 0)
            {
                cboRenderOption.SelectedIndex = 0;
            }

            void RemoveTabStop(Control container)
            {
                foreach (Control control in container.Controls)
                {
                    control.TabStop = false;
                    RemoveTabStop(control);
                }
            }

            RemoveTabStop(this);

            formReady = true;
        }

        private void ConfigEditor_Shown(object sender, EventArgs e)
        {
            Activate();

            formShownCancelToken = new CancellationTokenSource();
            Task.Run(() =>
            {
                Task.Delay(500).Wait(); // Allow a timeout if holding down the hotkey for too long
                if (!formShownCancelToken.IsCancellationRequested) formShown = true;
            }, formShownCancelToken.Token);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var keys = new Hotkey(Keys.None, keyData);

            if (keyData == Keys.Escape)
            {
                formShownCancelToken.Cancel();
                Close();
                return true;
            }
            else if (keys == new Hotkey(MapAssistConfiguration.Loaded.HotkeyConfiguration.ToggleConfigKey))
            {
                if (!formShown) return false;

                formShownCancelToken.Cancel();
                Close();
                return true;
            }

            // Call the base class
            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            MapAssistConfiguration.Loaded.Save();
            formShown = false;
            base.OnFormClosing(e);
        }

        private void IgnoreMouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;
        }

        private void updateTime_Scroll(object sender, EventArgs e)
        {
        }

        private void opacity_Scroll(object sender, EventArgs e)
        {
            if (opacity.Value > 0)
            {
                MapAssistConfiguration.Loaded.RenderingConfiguration.Opacity = Math.Round(opacity.Value * 5 / 100d, 2);
            }
            else
            {
                MapAssistConfiguration.Loaded.RenderingConfiguration.Opacity = 0;
            }
            lblOpacityValue.Text = (opacity.Value * 5).ToString();
        }

        private void allIconOpacity_Scroll(object sender, EventArgs e)
        {
            if (allIconOpacity.Value > 0)
            {
                MapAssistConfiguration.Loaded.RenderingConfiguration.IconOpacity = Math.Round(allIconOpacity.Value * 5 / 100d, 2);
            }
            else
            {
                MapAssistConfiguration.Loaded.RenderingConfiguration.IconOpacity = 0;
            }
            lblAllIconOpacityValue.Text = (allIconOpacity.Value * 5).ToString();
        }

        private void mapSize_Scroll(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.Size = mapSize.Value * 100;
            lblMapSizeValue.Text = (mapSize.Value * 100).ToString();
        }

        private void mapZoom_Scroll(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.ZoomLevel = tickToZoom(mapZoom.Value);
            lblMapZoomValue.Text = MapAssistConfiguration.Loaded.RenderingConfiguration.ZoomLevel.ToString();
        }

        private int zoomToTick(double zoom)
        {
            if (zoom == 1) return 10;
            else if (zoom < 1) return (int)Math.Max(Math.Round((1 - zoom) * 10), 1); // Minimum zoom = 0.1 translates to tick = 1
            else return (int)Math.Min(Math.Round((zoom - 1) * 5) + 10, 25); // Maximum zoom = 4 translates to tick = 25
        }

        private double tickToZoom(int tick)
        {
            if (tick == 10) return 1;
            else if (tick < 10) return Math.Max(tick / 10d, 0.1d); // Minimum zoom = 0.1 translates to tick = 1
            else return Math.Min((tick - 10) / 5d + 1, 4); // Maximum zoom = 4 translates to tick = 25
        }

        private void buffSize_Scroll(object sender, EventArgs e)
        {
            if (buffSize.Value > 0)
            {
                MapAssistConfiguration.Loaded.RenderingConfiguration.BuffSize = Math.Round(buffSize.Value / 10d, 2);
            }
            else
            {
                MapAssistConfiguration.Loaded.RenderingConfiguration.BuffSize = 0;
            }
            lblBuffSizeValue.Text = MapAssistConfiguration.Loaded.RenderingConfiguration.BuffSize.ToString();
        }

        private void chkBuffs_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.ShowBuffBarBuffs = chkBuffs.Checked;
        }

        private void chkAuras_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.ShowBuffBarAuras = chkAuras.Checked;
        }

        private void chkPassives_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.ShowBuffBarPassives = chkPassives.Checked;
        }

        private void chkDebuffs_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.ShowBuffBarDebuffs = chkDebuffs.Checked;
        }

        private void chkAlertLowerRes_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.BuffAlertLowRes = chkAlertLowerRes.Checked;
        }

        private void chkLife_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.ShowLife = chkLife.Checked;
        }

        private void chkMana_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.ShowMana = chkMana.Checked;
        }

        private void chkLifePerc_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.ShowLifePerc = chkLifePerc.Checked;
        }

        private void chkManaPerc_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.ShowManaPerc = chkManaPerc.Checked;
        }

        private void chkCurrentLevel_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.ShowCurrentLevel = chkCurrentLevel.Checked;
        }

        private void chkExpProgress_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.ShowExpProgress = chkExpProgress.Checked;
        }

        private void chkPotionBelt_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.ShowPotionBelt = chkPotionBelt.Checked;
        }

        private void chkResistances_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.ShowResistances = chkResistances.Checked;
        }

        private void chkOverlayMode_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.OverlayMode = chkOverlayMode.Checked;
        }

        private void chkMonsterHealthBar_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.MonsterHealthBar = chkMonsterHealthBar.Checked;
        }

        private void cboPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.Position = (MapPosition)cboPosition.SelectedIndex;
        }

        private void chkToggleViaMap_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.ToggleViaInGameMap = chkToggleViaMap.Checked;
        }

        private void chkToggleViaPanels_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.ToggleViaInGamePanels = chkToggleViaPanels.Checked;
        }

        private void chkStickToLastGameWindow_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.StickToLastGameWindow = chkStickToLastGameWindow.Checked;
        }

        private void cboBuffPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.RenderingConfiguration.BuffPosition = (BuffPosition)cboBuffPosition.SelectedIndex;
        }

        private void chkShowGameName_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.GameInfo.ShowGameName = chkShowGameName.Checked;
        }

        private void chkShowGameTimer_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.GameInfo.ShowGameTimer = chkShowGameTimer.Checked;
        }

        private void chkShowAreaTimer_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.GameInfo.ShowAreaTimer = chkShowAreaTimer.Checked;
        }

        private void chkShowDifficulty_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.GameInfo.ShowDifficulty = chkShowDifficulty.Checked;
        }

        private void chkShowAreaLevel_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.GameInfo.ShowAreaLevel = chkShowAreaLevel.Checked;
        }

        private void chkShowArea_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.GameInfo.ShowArea = chkShowArea.Checked;
        }

        private void txtD2Path_TextChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.D2LoDPath = txtD2Path.Text;
        }

        private void chkShowOverlayFPS_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.GameInfo.ShowOverlayFPS = chkShowOverlayFPS.Checked;
        }

        private void btnGameInfoFont_Click(object sender, EventArgs e)
        {
            var labelFont = MapAssistConfiguration.Loaded.GameInfo.LabelFont;
            var labelSize = (float)MapAssistConfiguration.Loaded.GameInfo.LabelFontSize;
            if (labelFont == null)
            {
                labelFont = "Helvetica";
                labelSize = 16;
            }
            var fontDlg = new FontDialog();
            fontDlg.Font = new Font(labelFont, labelSize, FontStyle.Regular);
            fontDlg.ShowEffects = false;
            if (fontDlg.ShowDialog() == DialogResult.OK)
            {
                MapAssistConfiguration.Loaded.GameInfo.LabelFont = fontDlg.Font.Name;
                MapAssistConfiguration.Loaded.GameInfo.LabelFontSize = fontDlg.Font.Size;

                btnClearGameInfoFont.Visible = true;
                fontDlg.Dispose();
            }
            else
            {
                fontDlg.Dispose();
            }
        }

        private void btnClearGameInfoFont_Click(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.GameInfo.LabelFont = MapAssistConfiguration.Default.GameInfo.LabelFont;
            MapAssistConfiguration.Loaded.GameInfo.LabelFontSize = MapAssistConfiguration.Default.GameInfo.LabelFontSize;

            btnClearGameInfoFont.Visible = false;
        }

        private void chkGameInfoTextShadow_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.GameInfo.LabelTextShadow = chkGameInfoTextShadow.Checked;
        }

        private void cboGameInfoPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.GameInfo.Position = (GameInfoPosition)cboGameInfoPosition.SelectedIndex;
        }

        private void cboRenderOption_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedProperty = MapAssistConfiguration.Loaded.MapConfiguration.GetType().GetProperty(cboRenderOption.Text.ToPascalCase());
            if (SelectedProperty != null)
            {
                tabDrawing.Visible = true;
            }
            else
            {
                return;
            }
            dynamic iconProp = SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null);
            btnIconColor.BackColor = iconProp.IconColor;
            btnIconColor.ForeColor = ContrastTextColor(btnIconColor.BackColor);
            btnClearFillColor.Visible = btnIconColor.BackColor.A > 0;
            btnIconOutlineColor.BackColor = iconProp.IconOutlineColor;
            btnIconOutlineColor.ForeColor = ContrastTextColor(btnIconOutlineColor.BackColor);
            btnClearOutlineColor.Visible = btnIconOutlineColor.BackColor.A > 0;
            cboIconShape.SelectedIndex = cboIconShape.FindStringExact(Enum.GetName(typeof(Shape), iconProp.IconShape));
            iconSize.Value = (int)iconProp.IconSize;
            iconThickness.Value = (int)iconProp.IconThickness;
            iconOpacity.Value = (int)Math.Round(iconProp.IconOpacity * 100d / 5);
            lblIconSizeValue.Text = iconSize.Value.ToString();
            lblIconThicknessValue.Text = iconThickness.Value.ToString();
            lblIconOpacityValue.Text = (iconOpacity.Value * 5).ToString();
            if (SelectedProperty.PropertyType != typeof(PointOfInterestRendering) && SelectedProperty.PropertyType != typeof(PortalRendering))
            {
                tabDrawing.TabPages.Remove(tabLabel);
                tabDrawing.TabPages.Remove(tabLine);
            }
            else
            {
                tabDrawing.TabPages.Remove(tabLabel);
                tabDrawing.TabPages.Remove(tabLine);
                tabDrawing.TabPages.Insert(1, tabLabel);
                tabDrawing.TabPages.Insert(2, tabLine);

                btnLabelColor.BackColor = iconProp.LabelColor;
                btnLabelColor.ForeColor = ContrastTextColor(btnLabelColor.BackColor);
                btnClearLabelColor.Visible = btnLabelColor.BackColor.A > 0;

                dynamic defaultlabelProp = MapAssistConfiguration.Default.MapConfiguration.GetType().GetProperty(cboRenderOption.Text.ToPascalCase()).GetValue(MapAssistConfiguration.Default.MapConfiguration, null);
                btnClearLabelFont.Visible = iconProp.LabelFont != defaultlabelProp.LabelFont || iconProp.LabelFontSize != defaultlabelProp.LabelFontSize;
                chkTextShadow.Checked = iconProp.LabelTextShadow;

                btnLineColor.BackColor = iconProp.LineColor;
                btnLineColor.ForeColor = ContrastTextColor(btnLineColor.BackColor);
                btnClearLineColor.Visible = btnLineColor.BackColor.A > 0;

                lineArrowSize.Value = iconProp.ArrowHeadSize;
                lineThicknessSize.Value = (int)iconProp.LineThickness;
                lblLineArrowSizeValue.Text = lineArrowSize.Value.ToString();
                lblLineThicknessSizeValue.Text = lineThicknessSize.Value.ToString();
            }
        }

        private void btnIconColor_Click(object sender, EventArgs e)
        {
            var (colorDlg, colorResult) = SelectColor(btnIconColor.BackColor);
            if (colorResult == DialogResult.OK)
            {
                var iconProp = SelectedProperty.PropertyType.GetProperty("IconColor");
                iconProp.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), colorDlg.Color, null);
                btnIconColor.BackColor = colorDlg.Color;
                btnIconColor.ForeColor = ContrastTextColor(btnIconColor.BackColor);

                btnClearFillColor.Visible = true;
            }
        }

        private void btnClearFillColor_Click(object sender, EventArgs e)
        {
            var iconProp = SelectedProperty.PropertyType.GetProperty("IconColor");
            iconProp.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), Color.Empty, null);
            btnIconColor.BackColor = Color.Empty;
            btnIconColor.ForeColor = ContrastTextColor(btnIconColor.BackColor);

            btnClearFillColor.Visible = false;
        }

        private void btnIconOutlineColor_Click(object sender, EventArgs e)
        {
            var (colorDlg, colorResult) = SelectColor(btnIconOutlineColor.BackColor);
            if (colorResult == DialogResult.OK)
            {
                var iconProp = SelectedProperty.PropertyType.GetProperty("IconOutlineColor");
                iconProp.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), colorDlg.Color, null);
                btnIconOutlineColor.BackColor = colorDlg.Color;
                btnIconOutlineColor.ForeColor = ContrastTextColor(btnIconOutlineColor.BackColor);

                btnClearOutlineColor.Visible = true;
            }
        }

        private void btnClearOutlineColor_Click(object sender, EventArgs e)
        {
            var iconOutlineProp = SelectedProperty.PropertyType.GetProperty("IconOutlineColor");
            iconOutlineProp.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), Color.Empty, null);
            btnIconOutlineColor.BackColor = Color.Empty;
            btnIconOutlineColor.ForeColor = ContrastTextColor(btnIconOutlineColor.BackColor);

            btnClearOutlineColor.Visible = false;
        }

        private void cboIconShape_SelectedIndexChanged(object sender, EventArgs e)
        {
            var iconProp = SelectedProperty.PropertyType.GetProperty("IconShape");
            iconProp.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), (MapPosition)cboIconShape.SelectedIndex, null);
        }

        private void iconSize_Scroll(object sender, EventArgs e)
        {
            var iconProp = SelectedProperty.PropertyType.GetProperty("IconSize");
            iconProp.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), iconSize.Value, null);
            lblIconSizeValue.Text = iconSize.Value.ToString();
        }

        private void iconThickness_Scroll(object sender, EventArgs e)
        {
            var iconProp = SelectedProperty.PropertyType.GetProperty("IconThickness");
            iconProp.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), iconThickness.Value, null);
            lblIconThicknessValue.Text = iconThickness.Value.ToString();
        }

        private void iconOpacity_Scroll(object sender, EventArgs e)
        {
            var iconProp = SelectedProperty.PropertyType.GetProperty("IconOpacity");
            if (iconOpacity.Value > 0)
            {
                iconProp.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), (float)Math.Round(iconOpacity.Value * 5 / 100d, 2), null);
            }
            else
            {
                iconProp.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), 0, null);
            }
            lblIconOpacityValue.Text = (iconOpacity.Value * 5).ToString();
        }

        private void tabDrawing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabDrawing.SelectedIndex > 0 && (SelectedProperty.PropertyType != typeof(PointOfInterestRendering) && SelectedProperty.PropertyType != typeof(PortalRendering)))
            {
                tabDrawing.SelectTab(0);
                MessageBox.Show("This property type does not support Labels or Lines.");
            }
        }

        private void btnLabelColor_Click(object sender, EventArgs e)
        {
            var (colorDlg, colorResult) = SelectColor(btnLabelColor.BackColor);
            if (colorResult == DialogResult.OK)
            {
                var labelPropColor = SelectedProperty.PropertyType.GetProperty("LabelColor");
                labelPropColor.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), colorDlg.Color, null);
                btnLabelColor.BackColor = colorDlg.Color;
                btnLabelColor.ForeColor = ContrastTextColor(btnLabelColor.BackColor);

                btnClearLabelColor.Visible = true;
            }
        }

        private void btnClearLabelColor_Click(object sender, EventArgs e)
        {
            var labelPropColor = SelectedProperty.PropertyType.GetProperty("LabelColor");
            labelPropColor.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), Color.Empty, null);
            btnLabelColor.BackColor = Color.Empty;
            btnLabelColor.ForeColor = ContrastTextColor(btnLabelColor.BackColor);

            btnClearLabelColor.Visible = false;
        }

        private void btnFont_Click(object sender, EventArgs e)
        {
            dynamic labelProp = SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null);
            var labelFont = (string)labelProp.LabelFont;
            var labelSize = (float)labelProp.LabelFontSize;
            if (labelFont == null)
            {
                labelFont = "Helvetica";
                labelSize = 16;
            }
            var fontDlg = new FontDialog();
            fontDlg.Font = new Font(labelFont, labelSize, FontStyle.Regular);
            fontDlg.ShowEffects = false;
            if (fontDlg.ShowDialog() == DialogResult.OK)
            {
                var labelPropFont = SelectedProperty.PropertyType.GetProperty("LabelFont");
                var labelPropFontSize = SelectedProperty.PropertyType.GetProperty("LabelFontSize");
                labelPropFont.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), fontDlg.Font.Name, null);
                labelPropFontSize.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), fontDlg.Font.Size, null);

                btnClearLabelFont.Visible = true;
            }
        }

        private void btnClearLabelFont_Click(object sender, EventArgs e)
        {
            dynamic defaultlabelProp = MapAssistConfiguration.Default.MapConfiguration.GetType().GetProperty(cboRenderOption.Text.ToPascalCase()).GetValue(MapAssistConfiguration.Default.MapConfiguration, null);

            var labelPropFont = SelectedProperty.PropertyType.GetProperty("LabelFont");
            var labelPropFontSize = SelectedProperty.PropertyType.GetProperty("LabelFontSize");

            labelPropFont.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), defaultlabelProp.LabelFont, null);
            labelPropFontSize.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), defaultlabelProp.LabelFontSize, null);

            btnClearLabelFont.Visible = false;
        }

        private void btnLineColor_Click(object sender, EventArgs e)
        {
            var (colorDlg, colorResult) = SelectColor(btnLineColor.BackColor);
            if (colorResult == DialogResult.OK)
            {
                var linePropColor = SelectedProperty.PropertyType.GetProperty("LineColor");
                linePropColor.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), colorDlg.Color, null);
                btnLineColor.BackColor = colorDlg.Color;
                btnLineColor.ForeColor = ContrastTextColor(btnLineColor.BackColor);

                btnClearLineColor.Visible = true;
            }
        }

        private void btnClearLineColor_Click(object sender, EventArgs e)
        {
            var linePropColor = SelectedProperty.PropertyType.GetProperty("LineColor");
            linePropColor.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), Color.Empty, null);
            btnLineColor.BackColor = Color.Empty;
            btnLineColor.ForeColor = ContrastTextColor(btnLineColor.BackColor);

            btnClearLineColor.Visible = false;
        }

        private void lineArrowSize_Scroll(object sender, EventArgs e)
        {
            var iconProp = SelectedProperty.PropertyType.GetProperty("ArrowHeadSize");
            iconProp.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), lineArrowSize.Value, null);
            lblLineArrowSizeValue.Text = lineArrowSize.Value.ToString();
        }

        private void lineThicknessSize_Scroll(object sender, EventArgs e)
        {
            var iconProp = SelectedProperty.PropertyType.GetProperty("LineThickness");
            iconProp.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), lineThicknessSize.Value, null);
            lblLineThicknessSizeValue.Text = lineThicknessSize.Value.ToString();
        }

        private void chkTextShadow_CheckedChanged(object sender, EventArgs e)
        {
            var LabelTextShadow = SelectedProperty.PropertyType.GetProperty("LabelTextShadow");
            LabelTextShadow.SetValue(SelectedProperty.GetValue(MapAssistConfiguration.Loaded.MapConfiguration, null), chkTextShadow.Checked, null);
        }

        private void cboLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.LanguageCode = (Locale)cboLanguage.SelectedIndex;
            PointOfInterestHandler.UpdateLocalizationNames();
        }

        private void chkItemLogEnabled_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.Enabled = chkItemLogEnabled.Checked;
        }

        private void chkItemLogItemsOnIdentify_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.CheckItemOnIdentify = chkItemLogItemsOnIdentify.Checked;
        }

        private void chkShowDistanceToItem_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.ShowDistanceToItem = chkShowDistanceToItem.Checked;
        }

        private void chkShowDirectionToItem_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.ShowDirectionToItem = chkShowDirectionToItem.Checked;
        }

        private void chkItemLogVendorItems_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.CheckVendorItems = chkItemLogVendorItems.Checked;
        }

        private void cboItemLogPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.Position = (GameInfoPosition)cboItemLogPosition.SelectedIndex;
        }

        private void txtFilterFile_TextChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.FilterFileName = txtFilterFile.Text;
        }

        private void soundSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.SoundFile = cboItemLogSound.SelectedItem.ToString();
            if (formReady) AudioPlayer.PlayItemAlert(MapAssistConfiguration.Loaded.ItemLog.SoundFile, stopPreviousAlert: true);
        }

        private void chkPlaySound_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.PlaySoundOnDrop = chkPlaySound.Checked;
        }

        private void soundVolume_Scroll(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.SoundVolume = soundVolume.Value * 5;
            lblSoundVolumeValue.Text = $"{soundVolume.Value * 5}";
        }

        private void itemDisplayForSeconds_Scroll(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.DisplayForSeconds = (double)itemDisplayForSeconds.Value * 5;
            lblItemDisplayForSecondsValue.Text = $"{itemDisplayForSeconds.Value * 5} s";
        }

        private void btnLogFont_Click(object sender, EventArgs e)
        {
            var labelFont = MapAssistConfiguration.Loaded.ItemLog.LabelFont;
            var labelSize = (float)MapAssistConfiguration.Loaded.ItemLog.LabelFontSize;
            if (labelFont == null)
            {
                labelFont = "Helvetica";
                labelSize = 16;
            }
            var fontDlg = new FontDialog();
            fontDlg.Font = new Font(labelFont, labelSize, FontStyle.Regular);
            fontDlg.ShowEffects = false;
            if (fontDlg.ShowDialog() == DialogResult.OK)
            {
                MapAssistConfiguration.Loaded.ItemLog.LabelFont = fontDlg.Font.Name;
                MapAssistConfiguration.Loaded.ItemLog.LabelFontSize = fontDlg.Font.Size;

                btnClearLogFont.Visible = true;
            }
        }

        private void btnClearLogFont_Click(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.LabelFont = MapAssistConfiguration.Default.ItemLog.LabelFont;
            MapAssistConfiguration.Loaded.ItemLog.LabelFontSize = MapAssistConfiguration.Default.ItemLog.LabelFontSize;

            btnClearLogFont.Visible = false;
        }

        private void chkLogTextShadow_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.LabelTextShadow = chkLogTextShadow.Checked;
        }

        private void btnSuperiorColor_Click(object sender, EventArgs e)
        {
            var (colorDlg, colorResult) = SelectColor(btnSuperiorColor.BackColor);
            if (colorResult == DialogResult.OK)
            {
                MapAssistConfiguration.Loaded.ItemLog.SuperiorColor = colorDlg.Color;
                btnSuperiorColor.BackColor = colorDlg.Color;
                btnSuperiorColor.ForeColor = ContrastTextColor(btnSuperiorColor.BackColor);

                btnClearSuperiorColor.Visible = true;
            }
        }

        private void btnMagicColor_Click(object sender, EventArgs e)
        {
            var (colorDlg, colorResult) = SelectColor(btnMagicColor.BackColor);
            if (colorResult == DialogResult.OK)
            {
                MapAssistConfiguration.Loaded.ItemLog.MagicColor = colorDlg.Color;
                btnMagicColor.BackColor = colorDlg.Color;
                btnMagicColor.ForeColor = ContrastTextColor(btnMagicColor.BackColor);

                btnClearMagicColor.Visible = true;
            }
        }

        private void btnRareColor_Click(object sender, EventArgs e)
        {
            var (colorDlg, colorResult) = SelectColor(btnRareColor.BackColor);
            if (colorResult == DialogResult.OK)
            {
                MapAssistConfiguration.Loaded.ItemLog.RareColor = colorDlg.Color;
                btnRareColor.BackColor = colorDlg.Color;
                btnRareColor.ForeColor = ContrastTextColor(btnRareColor.BackColor);

                btnClearRareColor.Visible = true;
            }
        }

        private void btnSetColor_Click(object sender, EventArgs e)
        {
            var (colorDlg, colorResult) = SelectColor(btnSetColor.BackColor);
            if (colorResult == DialogResult.OK)
            {
                MapAssistConfiguration.Loaded.ItemLog.SetColor = colorDlg.Color;
                btnSetColor.BackColor = colorDlg.Color;
                btnSetColor.ForeColor = ContrastTextColor(btnSetColor.BackColor);

                btnClearSetColor.Visible = true;
            }
        }

        private void btnUniqueColor_Click(object sender, EventArgs e)
        {
            var (colorDlg, colorResult) = SelectColor(btnUniqueColor.BackColor);
            if (colorResult == DialogResult.OK)
            {
                MapAssistConfiguration.Loaded.ItemLog.UniqueColor = colorDlg.Color;
                btnUniqueColor.BackColor = colorDlg.Color;
                btnUniqueColor.ForeColor = ContrastTextColor(btnUniqueColor.BackColor);

                btnClearUniqueColor.Visible = true;
            }
        }

        private void btnCraftedColor_Click(object sender, EventArgs e)
        {
            var (colorDlg, colorResult) = SelectColor(btnCraftedColor.BackColor);
            if (colorResult == DialogResult.OK)
            {
                MapAssistConfiguration.Loaded.ItemLog.CraftedColor = colorDlg.Color;
                btnCraftedColor.BackColor = colorDlg.Color;
                btnCraftedColor.ForeColor = ContrastTextColor(btnCraftedColor.BackColor);

                btnClearCraftedColor.Visible = true;
            }
        }

        private void btnClearSuperiorColor_Click(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.SuperiorColor = MapAssistConfiguration.Default.ItemLog.SuperiorColor;

            btnSuperiorColor.BackColor = MapAssistConfiguration.Loaded.ItemLog.SuperiorColor;
            btnSuperiorColor.ForeColor = ContrastTextColor(btnSuperiorColor.BackColor);

            btnClearSuperiorColor.Visible = false;
        }

        private void btnClearMagicColor_Click(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.MagicColor = MapAssistConfiguration.Default.ItemLog.MagicColor;

            btnMagicColor.BackColor = MapAssistConfiguration.Loaded.ItemLog.MagicColor;
            btnMagicColor.ForeColor = ContrastTextColor(btnMagicColor.BackColor);

            btnClearMagicColor.Visible = false;
        }

        private void btnClearRareColor_Click(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.RareColor = MapAssistConfiguration.Default.ItemLog.RareColor;

            btnRareColor.BackColor = MapAssistConfiguration.Loaded.ItemLog.RareColor;
            btnRareColor.ForeColor = ContrastTextColor(btnRareColor.BackColor);

            btnClearRareColor.Visible = false;
        }

        private void btnClearSetColor_Click(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.SetColor = MapAssistConfiguration.Default.ItemLog.SetColor;

            btnSetColor.BackColor = MapAssistConfiguration.Loaded.ItemLog.SetColor;
            btnSetColor.ForeColor = ContrastTextColor(btnSetColor.BackColor);

            btnClearSetColor.Visible = false;
        }

        private void btnClearUniqueColor_Click(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.UniqueColor = MapAssistConfiguration.Default.ItemLog.UniqueColor;

            btnUniqueColor.BackColor = MapAssistConfiguration.Loaded.ItemLog.UniqueColor;
            btnUniqueColor.ForeColor = ContrastTextColor(btnUniqueColor.BackColor);

            btnClearUniqueColor.Visible = false;
        }

        private void btnClearCraftedColor_Click(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.ItemLog.CraftedColor = MapAssistConfiguration.Default.ItemLog.CraftedColor;

            btnCraftedColor.BackColor = MapAssistConfiguration.Loaded.ItemLog.CraftedColor;
            btnCraftedColor.ForeColor = ContrastTextColor(btnCraftedColor.BackColor);

            btnClearCraftedColor.Visible = false;
        }

        private void txtToggleMapKey_TextChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.HotkeyConfiguration.ToggleKey = txtToggleMapKey.Text;
        }

        private void txtHideMapKey_TextChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.HotkeyConfiguration.HideMapKey = txtHideMapKey.Text;
        }

        private void txtMapPositionsKey_TextChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.HotkeyConfiguration.MapPositionsKey = txtMapPositionsKey.Text;
        }

        private void txtZoomInKey_TextChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.HotkeyConfiguration.ZoomInKey = txtZoomInKey.Text;
        }

        private void txtZoomOutKey_TextChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.HotkeyConfiguration.ZoomOutKey = txtZoomOutKey.Text;
        }

        private void txtExportItemsKey_TextChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.HotkeyConfiguration.ExportItemsKey = txtExportItemsKey.Text;
        }

        private void txtToggleConfigKey_TextChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.HotkeyConfiguration.ToggleConfigKey = txtToggleConfigKey.Text;
        }

        private void btnWalkableColor_Click(object sender, EventArgs e)
        {
            var (colorDlg, colorResult) = SelectColor(btnWalkableColor.BackColor);
            if (colorResult == DialogResult.OK)
            {
                MapAssistConfiguration.Loaded.MapColorConfiguration.Walkable = colorDlg.Color;
                btnWalkableColor.BackColor = colorDlg.Color;
                btnWalkableColor.ForeColor = ContrastTextColor(btnWalkableColor.BackColor);

                btnClearWalkableColor.Visible = true;
            }
        }

        private void btnClearWalkableColor_Click(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.MapColorConfiguration.Walkable = null;
            btnWalkableColor.BackColor = Color.Empty;
            btnWalkableColor.ForeColor = ContrastTextColor(btnWalkableColor.BackColor);

            btnClearWalkableColor.Visible = false;
        }

        private void btnBorderColor_Click(object sender, EventArgs e)
        {
            var (colorDlg, colorResult) = SelectColor(btnBorderColor.BackColor);
            if (colorResult == DialogResult.OK)
            {
                MapAssistConfiguration.Loaded.MapColorConfiguration.Border = colorDlg.Color;
                btnBorderColor.BackColor = colorDlg.Color;
                btnBorderColor.ForeColor = ContrastTextColor(btnBorderColor.BackColor);

                btnClearBorderColor.Visible = true;
            }
        }

        private void btnClearBorderColor_Click(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.MapColorConfiguration.Border = null;
            btnBorderColor.BackColor = Color.Empty;
            btnBorderColor.ForeColor = ContrastTextColor(btnBorderColor.BackColor);

            btnClearBorderColor.Visible = false;
        }

        private void btnExpRangeColor_Click(object sender, EventArgs e)
        {
            var (colorDlg, colorResult) = SelectColor(btnExpRangeColor.BackColor);
            if (colorResult == DialogResult.OK)
            {
                MapAssistConfiguration.Loaded.MapColorConfiguration.ExpRange = colorDlg.Color;
                btnExpRangeColor.BackColor = colorDlg.Color;
                btnExpRangeColor.ForeColor = ContrastTextColor(btnExpRangeColor.BackColor);

                btnClearExpRangeColor.Visible = true;
            }
        }

        private void btnClearExpRangeColor_Click(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.MapColorConfiguration.ExpRange = null;
            btnExpRangeColor.BackColor = Color.Empty;
            btnExpRangeColor.ForeColor = ContrastTextColor(btnExpRangeColor.BackColor);

            btnClearExpRangeColor.Visible = false;
        }

        private void btnAddHidden_Click(object sender, EventArgs e)
        {
            if (areaForm == null)
            {
                areaForm = new AddAreaForm();
            }

            areaForm.listToAddTo = "lstHidden";

            if (areaForm.Visible)
            {
                areaForm.Activate();
            }
            else
            {
                areaForm.ShowDialog(this);
            }
        }

        private void btnRemoveHidden_Click(object sender, EventArgs e)
        {
            var indexToRemove = lstHidden.SelectedIndex;
            if (indexToRemove >= 0)
            {
                lstHidden.Items.RemoveAt(indexToRemove);
                var hiddenList = new List<Area>(MapAssistConfiguration.Loaded.HiddenAreas);
                hiddenList.RemoveAt(indexToRemove);
                MapAssistConfiguration.Loaded.HiddenAreas = hiddenList.ToArray();
            }
        }

        private void btnAddAuthorizedWindowTitle_Click(object sender, EventArgs e)
        {
            if (txtAuthorizedWindowTitle.Text.Length > 0)
            {
                lstAuthorizedWindowTitle.Items.Add(txtAuthorizedWindowTitle.Text);
                MapAssistConfiguration.Loaded.AuthorizedWindowTitles = MapAssistConfiguration.Loaded.AuthorizedWindowTitles.Append(txtAuthorizedWindowTitle.Text).ToArray();
                txtAuthorizedWindowTitle.Text = "";
            }
        }

        private void btnRemoveAuthorizedWindowTitle_Click(object sender, EventArgs e)
        {
            var indexToRemove = lstAuthorizedWindowTitle.SelectedIndex;
            if (indexToRemove >= 0)
            {
                lstAuthorizedWindowTitle.Items.RemoveAt(indexToRemove);
                var authorizedWindowTitleList = new List<string>(MapAssistConfiguration.Loaded.AuthorizedWindowTitles);
                authorizedWindowTitleList.RemoveAt(indexToRemove);
                MapAssistConfiguration.Loaded.AuthorizedWindowTitles = authorizedWindowTitleList.ToArray();
            }
        }

        private void btnBrowseD2Location_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    txtD2Path.Text = fbd.SelectedPath;
                }
                else
                {
                    txtD2Path.Text = "";
                }
            }
        }

        private Color ContrastTextColor(Color backgroundColor)
        {
            // https://en.wikipedia.org/wiki/Luma_%28video%29#Rec._601_luma_versus_Rec._709_luma_coefficients

            var brightness = (int)Math.Sqrt(
                backgroundColor.R * backgroundColor.R * .299 +
                backgroundColor.G * backgroundColor.G * .587 +
                backgroundColor.B * backgroundColor.B * .114);

            return brightness > 128 ? Color.Black : Color.White;
        }

        private void chkDPIAware_CheckedChanged(object sender, EventArgs e)
        {
            MapAssistConfiguration.Loaded.DPIAware = chkDPIAware.Checked;
        }

        private List<Color> customColors = new List<Color>();

        private (ColorDialog, DialogResult) SelectColor(Color presetColor)
        {
            var colorDlg = new ColorDialog();
            colorDlg.FullOpen = true;
            colorDlg.Color = presetColor;
            if (customColors.Count > 0)
            {
                colorDlg.CustomColors = customColors.Select(color => ColorTranslator.ToOle(color)).ToArray();
            }
            var result = colorDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                customColors = colorDlg.CustomColors.Select(color => ColorTranslator.FromOle(color)).Where(color => color != Color.White).ToList();
                customColors.Remove(colorDlg.Color);
                customColors.Insert(0, colorDlg.Color);
            }
            return (colorDlg, result);
        }

        private void linkWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkWebsite.Text);
        }

        private void HandleLineToggle(CheckBox input, PointOfInterestRendering rendering)
        {
            if (input.Checked == rendering.CanDrawLine())
            {
                return;
            }
            rendering.ToggleLine();
        }

        private void chkLinesHostiles_CheckedChanged(object sender, EventArgs e)
        {
            HandleLineToggle(chkLinesHostiles, MapAssistConfiguration.Loaded.MapConfiguration.HostilePlayer);
        }

        private void chkLinesCorpse_CheckedChanged(object sender, EventArgs e)
        {
            HandleLineToggle(chkLinesCorpse, MapAssistConfiguration.Loaded.MapConfiguration.Corpse);
        }

        private void chkLinesNextArea_CheckedChanged(object sender, EventArgs e)
        {
            HandleLineToggle(chkLinesNextArea, MapAssistConfiguration.Loaded.MapConfiguration.NextArea);
        }

        private void chkLinesQuest_CheckedChanged(object sender, EventArgs e)
        {
            HandleLineToggle(chkLinesQuest, MapAssistConfiguration.Loaded.MapConfiguration.Quest);
        }

        private void chkLinesWaypoint_CheckedChanged(object sender, EventArgs e)
        {
            HandleLineToggle(chkLinesWaypoint, MapAssistConfiguration.Loaded.MapConfiguration.Waypoint);
        }

        private void chkLinesShrines_CheckedChanged(object sender, EventArgs e)
        {
            HandleLineToggle(chkLinesShrines, MapAssistConfiguration.Loaded.MapConfiguration.Shrine);
        }
    }
}
