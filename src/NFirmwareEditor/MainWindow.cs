﻿using System;
using System.Windows.Forms;
using NFirmwareEditor.Core;
using NFirmwareEditor.Firmware;

namespace NFirmwareEditor
{
	public partial class MainWindow : Form
	{
		private byte[] m_firmware;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void MainWindow_Load(object sender, EventArgs e)
		{
			ImagePixelGrid.Data = new bool[5, 5];
			var definitions = FirmwareDefinitionManager.Load();
			foreach (var definition in definitions)
			{
				DefinitionsComboBox.Items.Add(definition);
			}
			if (definitions.Count > 0)
			{
				DefinitionsComboBox.SelectedItem = definitions[0];
			}
		}

		private void OpenEncryptedMenuItem_Click(object sender, EventArgs e)
		{
			OpenDialogAndReadFirmwareOnOk(fileName => FirmwareEncoder.ReadFile(fileName));
		}

		private void OpenDecryptedMenuItem_Click(object sender, EventArgs e)
		{
			OpenDialogAndReadFirmwareOnOk(fileName => FirmwareEncoder.ReadFile(fileName, false));
		}

		private void OpenDialogAndReadFirmwareOnOk(Func<string, byte[]> readFirmwareDelegate)
		{
			using (var op = new OpenFileDialog { Filter = @"Firmware file|*.bin" })
			{
				if (op.ShowDialog() != DialogResult.OK) return;

				try
				{
					m_firmware = readFirmwareDelegate(op.FileName);
					StatusLabel.Text = @"Firmware loaded successfully. Now choose definition and scan...";
				}
				catch (Exception ex)
				{
					InfoBox.Show("Can't open file.\n" + ex.Message);
				}
			}
		}

		private void ExitMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void ImagesListBox_SelectedValueChanged(object sender, EventArgs e)
		{
			var metadata = ImagesListBox.SelectedItem as ImageMetadata;
			if (metadata == null) return;

			StatusLabel.Text = string.Format("Image: {0}x{1}", metadata.Width, metadata.Height);
			ImagePixelGrid.Data = FirmwareImageProcessor.ReadImage(m_firmware, metadata);
		}

		private void GridSizeUpDown_ValueChanged(object sender, EventArgs e)
		{
			ImagePixelGrid.BlockSize = (int)GridSizeUpDown.Value;
		}

		private void ShowGridCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			ImagePixelGrid.ShowGrid = ShowGridCheckBox.Checked;
		}

		private void ScanButton_Click(object sender, EventArgs e)
		{
			var definition = DefinitionsComboBox.SelectedItem as FirmwareDefinition;
			if (definition == null)
			{
				InfoBox.Show("Select definition first.");
				return;
			}

			var images = FirmwareImageProcessor.EnumerateImages(m_firmware, definition.ImageTable.OffsetFrom, definition.ImageTable.OffsetTo);
			ImagesListBox.BeginUpdate();
			ImagesListBox.Items.Clear();
			foreach (var imageData in images)
			{
				ImagesListBox.Items.Add(imageData);
			}
			ImagesListBox.EndUpdate();
		}
	}
}
