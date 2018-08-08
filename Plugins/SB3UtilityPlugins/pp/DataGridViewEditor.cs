using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SlimDX;

namespace SB3Utility
{
	public class DataGridViewEditor : DataGridView
	{
		public bool ShowRowIndex { get; set; }

		public delegate bool ValidateCellDelegate(string data, int rowIdx, int columnIdx);
		private ValidateCellDelegate ValidateCell = null;
		private bool isEditingCell = false;
		private int maxRowCount = 0;

		public DataGridViewEditor()
			: base()
		{
			this.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;

			this.DataError += new DataGridViewDataErrorEventHandler(DataGridViewEditor_DataError);
			this.Scroll += new ScrollEventHandler(DataGridViewEditor_Scroll);
			this.SelectionChanged += new EventHandler(DataGridViewEditor_SelectionChanged);
			this.CellValidating += new DataGridViewCellValidatingEventHandler(DataGridViewEditor_CellValidating);
			this.CellValidated += new DataGridViewCellEventHandler(DataGridViewEditor_CellValidated);
			this.CellPainting += new DataGridViewCellPaintingEventHandler(DataGridViewEditor_CellPainting);
			this.CellBeginEdit += new DataGridViewCellCancelEventHandler(DataGridViewEditor_CellBeginEdit);
			this.CellEndEdit += new DataGridViewCellEventHandler(DataGridViewEditor_CellEndEdit);
			this.KeyDown += new KeyEventHandler(DataGridViewEditor_KeyDown);
		}

		public void Initialize(DataTable table, ValidateCellDelegate validateCell, int maxRowCount)
		{
			this.ValidateCell = validateCell;
			this.maxRowCount = maxRowCount;
			this.DataSource = table;

			while (SelectedCells.Count > 0)
			{
				SelectedCells[0].Selected = false;
			}
		}

		private void DataGridViewEditor_DataError(object sender, DataGridViewDataErrorEventArgs e)
		{
			Report.ReportLog("Error editing cell (" + e.RowIndex + ", " + this.Columns[e.ColumnIndex].Name + "): " + e.Exception.Message);
			e.Cancel = true;
			e.ThrowException = false;
		}

		private void DataGridViewEditor_Scroll(object sender, ScrollEventArgs e)
		{
			if (this.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.DisplayedCells)
			{
				this.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
			}
		}

		private void DataGridViewEditor_SelectionChanged(object sender, EventArgs e)
		{
			if (isEditingCell)
			{
				this.EndEdit();
			}
		}

		private void DataGridViewEditor_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if (!ValidateCell((string)e.FormattedValue, e.RowIndex, e.ColumnIndex))
			{
				e.Cancel = true;
			}
		}

		private void DataGridViewEditor_CellValidated(object sender, DataGridViewCellEventArgs e)
		{
			DataTable table = (DataTable)this.DataSource;
			if ((e.RowIndex < table.Rows.Count) && (e.ColumnIndex < table.Columns.Count))
			{
				if (!table.Columns[e.ColumnIndex].ReadOnly && (table.Rows[e.RowIndex][e.ColumnIndex] == DBNull.Value))
				{
					table.Rows[e.RowIndex][e.ColumnIndex] = table.Columns[e.ColumnIndex].DefaultValue;
				}
			}
		}

		private void DataGridViewEditor_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			if (!ShowRowIndex || !RowHeadersVisible)
			{
				return;
			}

			DataTable table = (DataTable)this.DataSource;
			if ((e.ColumnIndex < 0) && (e.RowIndex >= 0) && (e.RowIndex < table.Rows.Count))
			{
				StringFormat sf = new StringFormat();
				sf.LineAlignment = StringAlignment.Center;
				sf.Alignment = StringAlignment.Center;
				SolidBrush brush;
				if (this.Rows[e.RowIndex].Selected)
				{
					brush = new SolidBrush(e.CellStyle.SelectionForeColor);
				}
				else
				{
					brush = new SolidBrush(e.CellStyle.ForeColor);
				}

				string rowIndex = e.RowIndex.ToString();
				e.PaintBackground(e.ClipBounds, true);
				e.Graphics.DrawString(rowIndex, this.Font, brush, e.CellBounds, sf);

				int rowHeaderWidth = TextRenderer.MeasureText(rowIndex, this.Font).Width + 20;
				if (rowHeaderWidth > this.RowHeadersWidth)
				{
					this.RowHeadersWidth = rowHeaderWidth;
				}

				e.Handled = true;
			}
		}

		private void DataGridViewEditor_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
		{
			isEditingCell = true;
			if (this.AllowUserToAddRows && (e.RowIndex >= maxRowCount))
			{
				e.Cancel = true;
				isEditingCell = false;
				Report.ReportLog("Max number of " + maxRowCount + " rows reached");
			}
		}

		private void DataGridViewEditor_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			isEditingCell = false;
		}

		private bool IsValidRow(DataRow row, int rowIdx)
		{
			DataTable table = (DataTable)this.DataSource;
			object[] itemArray = row.ItemArray;
			for (int i = 0; i < itemArray.Length; i++)
			{
				if (!itemArray[i].GetType().Equals(table.Columns[i].DataType) ||
					!ValidateCell(itemArray[i].ToString(), rowIdx, i))
				{
					return false;
				}
			}
			return true;
		}

		private void DataGridViewEditor_KeyDownInitNewRow(out bool newRowSelected, out List<DataGridViewCell> newRowSelectCells)
		{
			if (this.AllowUserToAddRows)
			{
				newRowSelected = this.Rows[this.NewRowIndex].Selected;
				newRowSelectCells = new List<DataGridViewCell>(this.Rows[this.NewRowIndex].Cells.Count);
				if (newRowSelected)
				{
					this.Rows[this.NewRowIndex].Selected = false;
				}
				else
				{
					for (int i = 0; i < this.Rows[this.NewRowIndex].Cells.Count; i++)
					{
						if (this.Rows[this.NewRowIndex].Cells[i].Selected)
						{
							newRowSelectCells.Add(this.Rows[this.NewRowIndex].Cells[i]);
							this.Rows[this.NewRowIndex].Cells[i].Selected = false;
						}
					}
				}
			}
			else
			{
				newRowSelected = false;
				newRowSelectCells = new List<DataGridViewCell>(0);
			}
		}

		public void DataGridViewEditor_KeyDown(object sender, KeyEventArgs e)
		{
			if (isEditingCell)
			{
				if (e.KeyData == (Keys.Control | Keys.C))
				{
					DataGridViewTextBoxEditingControl textBox = this.EditingControl as DataGridViewTextBoxEditingControl;
					if (textBox != null)
					{
						string s = textBox.SelectedText;
						if (!String.IsNullOrEmpty(s))
						{
							Clipboard.SetText(s);
						}
					}
				}
				else if (e.KeyData == (Keys.Control | Keys.V))
				{
					DataGridViewTextBoxEditingControl textBox = this.EditingControl as DataGridViewTextBoxEditingControl;
					if ((textBox != null) && Clipboard.ContainsText())
					{
						textBox.SelectedText = Clipboard.GetText();
					}
				}
			}
			else
			{
				if (e.KeyData == (Keys.Control | Keys.C))
				{
					e.Handled = true;
					bool newRowSelected;
					List<DataGridViewCell> newRowSelectCells;
					DataGridViewEditor_KeyDownInitNewRow(out newRowSelected, out newRowSelectCells);

					this.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
					Clipboard.SetDataObject(this.GetClipboardContent());
					this.ClipboardCopyMode = DataGridViewClipboardCopyMode.Disable;

					if (newRowSelected)
					{
						this.Rows[this.NewRowIndex].Selected = true;
					}
					else
					{
						for (int i = 0; i < newRowSelectCells.Count; i++)
						{
							newRowSelectCells[i].Selected = true;
						}
					}
				}
				else if (e.KeyData == (Keys.Control | Keys.V))
				{
					e.Handled = true;

					try
					{
						string s = Clipboard.GetText(TextDataFormat.UnicodeText);
						if (s != null)
						{
							DataTable table = (DataTable)this.DataSource;

							bool newRowSelected;
							List<DataGridViewCell> newRowSelectCells;
							DataGridViewEditor_KeyDownInitNewRow(out newRowSelected, out newRowSelectCells);

							string[] cellData = s.Split(new string[] { Environment.NewLine, "\t" }, StringSplitOptions.None);
							DataGridViewCell[] cells = new DataGridViewCell[this.SelectedCells.Count];
							this.SelectedCells.CopyTo(cells, 0);
							Array.Sort(cells, new DataGridViewCellComparer());
							newRowSelectCells.Sort(new DataGridViewCellComparer());

							int cellDataIdx = 0;
							for (; (cellDataIdx < cellData.Length) && (cellDataIdx < cells.Length); cellDataIdx++)
							{
								if (!cells[cellDataIdx].ReadOnly && ValidateCell(cellData[cellDataIdx], cells[cellDataIdx].RowIndex, cells[cellDataIdx].ColumnIndex))
								{
									cells[cellDataIdx].Value = cellData[cellDataIdx];
								}
							}
							if (newRowSelected)
							{
								while (cellDataIdx < cellData.Length)
								{
									DataRow newRow = table.NewRow();
									object[] items = newRow.ItemArray;
									for (int i = 0; (i < items.Length) && (cellDataIdx < cellData.Length); i++)
									{
										items[i] = cellData[cellDataIdx];
										cellDataIdx++;
									}
									newRow.ItemArray = items;

									if (table.Rows.Count >= maxRowCount)
									{
										Report.ReportLog("Max number of " + maxRowCount + " rows reached");
										break;
									}
									else if (IsValidRow(newRow, table.Rows.Count))
									{
										table.Rows.Add(newRow);
									}
								}
							}
							else if (newRowSelectCells.Count > 0)
							{
								DataRow newRow = table.NewRow();
								object[] items = newRow.ItemArray;
								for (int i = 0; (i < newRowSelectCells.Count) && (cellDataIdx < cellData.Length); i++)
								{
									items[newRowSelectCells[i].ColumnIndex] = cellData[cellDataIdx];
									cellDataIdx++;
								}
								newRow.ItemArray = items;

								if (table.Rows.Count >= maxRowCount)
								{
									Report.ReportLog("Max number of " + maxRowCount + " rows reached");
								}
								else if (IsValidRow(newRow, table.Rows.Count))
								{
									table.Rows.Add(newRow);
								}
							}

							if (newRowSelected)
							{
								this.Rows[this.NewRowIndex].Selected = true;
							}
							else
							{
								for (int i = 0; i < newRowSelectCells.Count; i++)
								{
									newRowSelectCells[i].Selected = true;
								}
							}
						}
					}
					catch (Exception ex)
					{
						Report.ReportLog("Error pasting: " + ex.Message);
					}
				}
			}
		}

		private class DataGridViewCellComparer : IComparer<DataGridViewCell>
		{
			public int Compare(DataGridViewCell x, DataGridViewCell y)
			{
				int rowDiff = x.RowIndex - y.RowIndex;
				if (rowDiff != 0)
				{
					return rowDiff;
				}
				else
				{
					return x.ColumnIndex - y.ColumnIndex;
				}
			}
		}

		public static void InitDataGridViewSRT(DataGridViewEditor viewSRT, DataGridViewEditor viewMatrix)
		{
			DataTable tableSRT = new DataTable();
			tableSRT.Columns.Add(" ", typeof(string));
			tableSRT.Columns[0].ReadOnly = true;
			tableSRT.Columns.Add("X", typeof(float));
			tableSRT.Columns.Add("Y", typeof(float));
			tableSRT.Columns.Add("Z", typeof(float));
			tableSRT.Rows.Add(new object[] { "Translate", 0f, 0f, 0f });
			tableSRT.Rows.Add(new object[] { "Rotate", 0f, 0f, 0f });
			tableSRT.Rows.Add(new object[] { "Scale", 1f, 1f, 1f });
			viewSRT.Initialize(tableSRT, new DataGridViewEditor.ValidateCellDelegate(ValidateCellSRT), 3);
			viewSRT.Scroll += new ScrollEventHandler(dataGridViewEditor_Scroll);

			viewSRT.Columns[0].DefaultCellStyle = viewSRT.ColumnHeadersDefaultCellStyle;
			for (int i = 0; i < viewSRT.Columns.Count; i++)
			{
				viewSRT.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
			}

			viewSRT.Tag = viewMatrix;
		}

		public static void dataGridViewSRT_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView viewSRT = (DataGridView)sender;
			Vector3[] srt = GetSRT(viewSRT);
			Matrix mat = FbxUtility.SRTToMatrix(srt[0], srt[1], srt[2]);
			LoadMatrix(mat, null, (DataGridView)viewSRT.Tag);
		}

		public static void dataGridViewMatrix_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			DataGridView viewMatrix = (DataGridView)sender;
			Matrix mat = GetMatrix(viewMatrix);
			LoadMatrix(mat, (DataGridView)viewMatrix.Tag, null);
		}

		public static void InitDataGridViewMatrix(DataGridViewEditor viewMatrix, DataGridViewEditor viewSRT)
		{
			DataTable tableMatrix = new DataTable();
			tableMatrix.Columns.Add("1", typeof(float));
			tableMatrix.Columns.Add("2", typeof(float));
			tableMatrix.Columns.Add("3", typeof(float));
			tableMatrix.Columns.Add("4", typeof(float));
			tableMatrix.Rows.Add(new object[] { 1f, 0f, 0f, 0f });
			tableMatrix.Rows.Add(new object[] { 0f, 1f, 0f, 0f });
			tableMatrix.Rows.Add(new object[] { 0f, 0f, 1f, 0f });
			tableMatrix.Rows.Add(new object[] { 0f, 0f, 0f, 1f });
			viewMatrix.Initialize(tableMatrix, new DataGridViewEditor.ValidateCellDelegate(ValidateCellSingle), 4);
			viewMatrix.Scroll += new ScrollEventHandler(dataGridViewEditor_Scroll);

			for (int i = 0; i < viewMatrix.Columns.Count; i++)
			{
				viewMatrix.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
			}

			viewMatrix.Tag = viewSRT;
		}

		public static void dataGridViewEditor_Scroll(object sender, ScrollEventArgs e)
		{
			try
			{
				e.NewValue = e.OldValue;
			}
			catch (Exception ex)
			{
				Utility.ReportException(ex);
			}
		}

		public static bool ValidateCellSRT(string s, int row, int col)
		{
			if (col == 0)
			{
				return true;
			}
			else
			{
				return ValidateCellSingle(s, row, col);
			}
		}

		public static bool ValidateCellSingle(string s, int row, int col)
		{
			float f;
			if (Single.TryParse(s, out f))
			{
				return true;
			}
			return false;
		}

		public static Matrix GetMatrix(DataGridView viewMatrix)
		{
			Matrix m = new Matrix();
			DataTable table = (DataTable)viewMatrix.DataSource;
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					m[i, j] = (float)table.Rows[i][j];
				}
			}
			return m;
		}

		public static Vector3[] GetSRT(DataGridView viewSRT)
		{
			DataTable table = (DataTable)viewSRT.DataSource;
			Vector3[] srt = new Vector3[3];
			for (int i = 0; i < 3; i++)
			{
				srt[0][i] = (float)table.Rows[2][i + 1];
				srt[1][i] = (float)table.Rows[1][i + 1];
				srt[2][i] = (float)table.Rows[0][i + 1];
			}
			return srt;
		}

		public static void GetSQT(DataGridView viewSRT, out Vector3 localScale, out Quaternion localRotation, out Vector3 localTranslation)
		{
			DataTable table = (DataTable)viewSRT.DataSource;
			Vector3[] srt = new Vector3[3];
			for (int i = 0; i < 3; i++)
			{
				srt[0][i] = (float)table.Rows[2][i + 1];
				srt[1][i] = (float)table.Rows[1][i + 1];
				srt[2][i] = (float)table.Rows[0][i + 1];
			}
			localTranslation = srt[2];
			localRotation = FbxUtility.EulerToQuaternion(srt[1]);
			localScale = srt[0];
		}

		public static void LoadMatrix(Matrix matrix, DataGridView viewSRT, DataGridView viewMatrix)
		{
			if (viewSRT != null)
			{
				Vector3[] srt = FbxUtility.MatrixToSRT(matrix);
				DataTable tableSRT = (DataTable)viewSRT.DataSource;
				for (int i = 0; i < 3; i++)
				{
					tableSRT.Rows[0][i + 1] = srt[2][i];
					tableSRT.Rows[1][i + 1] = srt[1][i];
					tableSRT.Rows[2][i + 1] = srt[0][i];
				}
			}

			if (viewMatrix != null)
			{
				DataTable tableMatrix = (DataTable)viewMatrix.DataSource;
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						tableMatrix.Rows[i][j] = matrix[i, j];
					}
				}
			}
		}

		public static void LoadMatrix(Vector3 localScale, Quaternion localRotation, Vector3 localTranslation, DataGridView viewSRT, DataGridView viewMatrix)
		{
			if (viewSRT != null)
			{
				Vector3 rot = FbxUtility.QuaternionToEuler(localRotation);
				DataTable tableSRT = (DataTable)viewSRT.DataSource;
				for (int i = 0; i < 3; i++)
				{
					tableSRT.Rows[0][i + 1] = localTranslation[i];
					tableSRT.Rows[1][i + 1] = rot[i];
					tableSRT.Rows[2][i + 1] = localScale[i];
				}
			}

			if (viewMatrix != null)
			{
				Matrix matrix = Matrix.Scaling(localScale) * Matrix.RotationQuaternion(localRotation) * Matrix.Translation(localTranslation);
				DataTable tableMatrix = (DataTable)viewMatrix.DataSource;
				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						tableMatrix.Rows[i][j] = matrix[i, j];
					}
				}
			}
		}
	}
}
