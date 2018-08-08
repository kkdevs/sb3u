using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Text;
using System.Runtime.InteropServices;

namespace UrielGuy.SyntaxHighlightingTextBox
{
	/// <summary>
	/// A textbox the does syntax highlighting.
	/// </summary>
	public class SyntaxHighlightingTextBox :	System.Windows.Forms.RichTextBox 
	{
		#region Members

		//Members exposed via properties
		private SeperaratorCollection mSeperators = new SeperaratorCollection();  
		private HighLightDescriptorCollection mHighlightDescriptors = new HighLightDescriptorCollection();
		private bool mCaseSesitive = false;
		private bool mFilterAutoComplete = false;

		//Internal use members
		private bool mAutoCompleteShown = false;
		private bool mParsing = false;
		private bool mIgnoreLostFocus = false;

		private AutoCompleteForm mAutoCompleteForm = new AutoCompleteForm();

		//Undo/Redo members
		private ArrayList mUndoList = new ArrayList();
		private Stack mRedoStack = new Stack();
		private bool mIsUndo = false;
		private UndoRedoInfo mLastInfo;
		private int mMaxUndoRedoSteps = 50;

		static Encoding SHIFT_JIS = Encoding.GetEncoding(932);
		static Font VisibleTabFont = new Font("Courier New", 12.0f, FontStyle.Regular, GraphicsUnit.Point, 0);
		static Color VisibleTabForeColour = Color.Blue;
//		static Color VisibleTabBackColour = Color.Bisque;
		bool tab_pressed = false;
		bool tab_backspaced = false;
		bool tab_deleted = false;

		#endregion

		#region Properties
		/// <summary>
		/// Determines if token recognition is case sensitive.
		/// </summary>
		[Category("Behavior")]
		public bool CaseSensitive 
		{ 
			get 
			{ 
				return mCaseSesitive; 
			}
			set 
			{ 
				mCaseSesitive = value;
			}
		}


		/// <summary>
		/// Sets whether or not to remove items from the Autocomplete window as the user types...
		/// </summary>
		[Category("Behavior")]
		public bool FilterAutoComplete 
		{
			get 
			{
				return mFilterAutoComplete;
			}
			set 
			{
				mFilterAutoComplete = value;
			}
		}

		/// <summary>
		/// Set the maximum amount of Undo/Redo steps.
		/// </summary>
		[Category("Behavior")]
		public int MaxUndoRedoSteps 
		{
			get 
			{
				return mMaxUndoRedoSteps;
			}
			set
			{
				mMaxUndoRedoSteps = value;
			}
		}

			/// <summary>
			/// A collection of charecters. a token is every string between two seperators.
			/// </summary>
			/// 
			public SeperaratorCollection Seperators 
		{
			get 
			{
				return mSeperators;
			}
		}
		
		/// <summary>
		/// The collection of highlight descriptors.
		/// </summary>
		/// 
		public HighLightDescriptorCollection HighlightDescriptors 
		{
			get 
			{
				return mHighlightDescriptors;
			}
		}

		public void InitText(string newContents)
		{
			AppendText(newContents);
			SelectionStart = 0;

			mUndoList.RemoveAt(0);
			mLastInfo = new UndoRedoInfo(Text, GetScrollPos(), SelectionStart);
		}

		#endregion

		#region Overriden methods

		/// <summary>
		/// The on text changed overrided. Here we parse the text into RTF for the highlighting.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnTextChanged(EventArgs e)
		{
			if (mParsing) return;
			mParsing = true;
			Win32.LockWindowUpdate(Handle);
			base.OnTextChanged(e);

			if (!mIsUndo)
			{
				mRedoStack.Clear();
				mUndoList.Insert(0,mLastInfo);
				this.LimitUndo();
				mLastInfo = new UndoRedoInfo(Text, GetScrollPos(), SelectionStart);
			}
			
			//Save scroll bar an cursor position, changeing the RTF moves the cursor and scrollbars to top positin
			Win32.POINT scrollPos = GetScrollPos();
			int cursorLoc = SelectionStart;

			//Created with an estimate of how big the stringbuilder has to be...
			StringBuilder sb = new
				StringBuilder((int)(Text.Length * 1.5 + 150));
	
			//Adding RTF header
			sb.Append(@"{\rtf1\fbidis\ansi\ansicpg932\deff0\deflang1041{\fonttbl ");
			
			//Font table creation
			int fontCounter = 0;
			Hashtable fonts = new Hashtable();
			AddFontToTable(sb, Font, ref fontCounter, fonts);
			AddFontToTable(sb, VisibleTabFont, ref fontCounter, fonts);
			foreach (HighlightDescriptor hd in mHighlightDescriptors)
			{
				if ((hd.Font !=  null) && !fonts.ContainsKey(hd.Font.Name))
				{
					AddFontToTable(sb, hd.Font,ref fontCounter, fonts);
				}
			}
			sb.Append("}\n");

			//ColorTable
			
			sb.Append(@"{\colortbl ;");
			Hashtable colors = new Hashtable();
			int colorCounter = 1;
			AddColorToTable(sb, ForeColor, ref colorCounter, colors);
			AddColorToTable(sb, BackColor, ref colorCounter, colors);
			AddColorToTable(sb, VisibleTabForeColour, ref colorCounter, colors);
//			AddColorToTable(sb, VisibleTabBackColour, ref colorCounter, colors);
			
			foreach (HighlightDescriptor hd in mHighlightDescriptors)
			{
				if (!colors.ContainsKey(hd.Color))
				{
					AddColorToTable(sb, hd.Color, ref colorCounter, colors);
				}
			}		

			//Parsing text
	
			sb.Append("}\n").Append(@"\viewkind4\uc1\pard\ltrpar");
			SetDefaultSettings(sb, colors, fonts);

			char[] sperators = mSeperators.GetAsCharArray();

			//Replacing "\" to "\\" for RTF...
			StringBuilder replacer = new StringBuilder(Text);
			if (tab_backspaced)
			{
				replacer.Remove(SelectionStart - 1, 1);
			}
			if (tab_deleted)
			{
				replacer.Remove(SelectionStart, 1);
			}
			string[] lines = replacer.Replace("»\t", "\t").Replace("\\", "\\\\").Replace("{", "\\{").Replace("}", "\\}").ToString().Split('\n');
			for (int lineCounter = 0 ; lineCounter < lines.Length; lineCounter++)
			{
				string line = lines[lineCounter];
				string[] tokens = mCaseSesitive ? line.Split(sperators) : line.ToUpper().Split(sperators);

				int tokenCounter = 0;
				for (int i = 0; i < line.Length ;)
				{
					char curChar = line[i];
					if (mSeperators.Contains(curChar))
					{
						SetColor(sb, VisibleTabForeColour, colors);
						SetFont(sb, VisibleTabFont, fonts);
						SetFontSize(sb, (int)VisibleTabFont.Size);
						EndTags(sb);
						sb.Append(@"\uc0\u187");
						sb.Append(curChar);
						SetDefaultSettings(sb, colors, fonts);
						i++;
					}
					else
					{
						string curToken = tokens[tokenCounter++];
						bool bAddToken = true;
						foreach	(HighlightDescriptor hd in mHighlightDescriptors)
						{
							string	compareStr = mCaseSesitive ? hd.Token : hd.Token.ToUpper();
							bool match = false;

							//Check if the highlight descriptor matches the current toker according to the DescriptoRecognision property.
							switch	(hd.DescriptorRecognition)
							{
								case DescriptorRecognition.WholeWord:
									if (curToken == compareStr)
									{
											match = true;
									}
									break;
								case DescriptorRecognition.StartsWith:
									if (curToken.StartsWith(compareStr))
									{
										match = true;
									}
									break;
								case DescriptorRecognition.Contains:
									if (curToken.IndexOf(compareStr) != -1)
									{
										match = true;
									}
									break;
							}
							if (!match)
							{
								//If this token doesn't match chech the next one.
								continue;
							}

							//printing this token will be handled by the inner code, don't apply default settings...
							bAddToken = false;
	
							//Set colors to current descriptor settings.
							SetDescriptorSettings(sb, hd, colors, fonts);

							//Print text affected by this descriptor.
							switch (hd.DescriptorType)
							{
								case DescriptorType.Word:
									sb.Append(line.Substring(i, curToken.Length));
									SetDefaultSettings(sb, colors, fonts);
									i += curToken.Length;
									break;
								case DescriptorType.ToEOL:
									JStringToRtf(sb, line.Remove(0, i), hd.Color, colors, fonts);
									i = line.Length;
									SetDefaultSettings(sb, colors, fonts);
									break;
								case DescriptorType.ToCloseToken:
									while((line.IndexOf(hd.CloseToken, i) == -1) && (lineCounter < lines.Length))
									{
										sb.Append(line.Remove(0, i));
										lineCounter++;
										if (lineCounter < lines.Length)
										{
											AddNewLine(sb);
											line = lines[lineCounter];
											i = 0;
										}
										else
										{
											i = line.Length;
										}
									}
									if (line.IndexOf(hd.CloseToken, i) != -1)
									{
										sb.Append(line.Substring(i, line.IndexOf(hd.CloseToken, i) + hd.CloseToken.Length - i) );
										line = line.Remove(0, line.IndexOf(hd.CloseToken, i) + hd.CloseToken.Length);
										tokenCounter = 0;
										tokens = mCaseSesitive ? line.Split(sperators) : line.ToUpper().Split(sperators);
										SetDefaultSettings(sb, colors, fonts);
										i = 0;
									}
									break;
							}
							break;
						}
						if (bAddToken)
						{
							//Print text with default settings...
							JStringToRtf(sb, line.Substring(i, curToken.Length), ForeColor, colors, fonts);
							i+=	curToken.Length;
						}
					}
				}
				AddNewLine(sb);
			}
			sb.Append("}");
	
//			System.Diagnostics.Debug.WriteLine(sb.ToString());
			Rtf = sb.ToString();

			//Restore cursor and scrollbars location.
			SelectionStart = cursorLoc + (tab_pressed ? 1 : 0) - (tab_backspaced ? 1 : 0);
			tab_pressed = false;
			tab_backspaced = false;
			tab_deleted = false;

			mParsing = false;
		
			SetScrollPos(scrollPos);
			Win32.LockWindowUpdate((IntPtr)0);
			Invalidate();
			
			if (mAutoCompleteShown)
			{
				if (mFilterAutoComplete)
				{
					SetAutoCompleteItems();
					SetAutoCompleteSize();
					SetAutoCompleteLocation(false);
				}
				SetBestSelectedAutoCompleteItem();
			}

		}

		private void JStringToRtf(StringBuilder sb, string jStr, Color foreground, Hashtable colors, Hashtable fonts)
		{
			byte[] bytes = SHIFT_JIS.GetBytes(jStr);
			for (int i = 0; i < bytes.Length; i++)
			{
				byte b = bytes[i];
				if (b >= 128)
				{
					sb.Append("\\\'");
					sb.Append(b.ToString("X2"));
					if (++i >= bytes.Length)
					{
						break;
					}
					b = bytes[i];
					sb.Append("\\\'");
					sb.Append(b.ToString("X2"));
				}
				else
				{
					char curChar = SHIFT_JIS.GetChars(bytes, i, 1)[0];
					if (mSeperators.Contains(curChar))
					{
						SetColor(sb, VisibleTabForeColour, colors);
						SetFont(sb, VisibleTabFont, fonts);
						SetFontSize(sb, (int)VisibleTabFont.Size);
						EndTags(sb);
						sb.Append(@"\uc0\u187");
						SetColor(sb, foreground, colors);
						SetFont(sb, Font, fonts);
						SetFontSize(sb, (int)Font.Size);
						EndTags(sb);
					}
					sb.Append(curChar);
				}
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
			case Keys.Tab:
				tab_pressed = true;
				break;
			case Keys.Back:
				if (SelectionStart > 0 && Text[SelectionStart - 1] == '\t')
				{
					tab_backspaced = true;
				}
				break;
			case Keys.Delete:
				if (SelectionStart + 1 < Text.Length && Text[SelectionStart + 1] == '\t')
				{
					tab_deleted = true;
				}
				break;
			}
			base.OnKeyDown(e);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
			{
				if (SelectionStart < Text.Length && Text[SelectionStart] == '\t')
				{
					SelectionStart--;
					if (SelectionLength > 0)
					{
						SelectionLength++;
					}
				}
			}
			else if (e.KeyCode == Keys.V)
			{
				if (e.Control)
				{
					IDataObject iData = Clipboard.GetDataObject();
					if (iData.GetDataPresent(DataFormats.Text))
					{
						string contents = ((string)iData.GetData(DataFormats.Text));
						int numTabs = 0;
						foreach (char c in contents)
						{
							if (c == '\t')
							{
								numTabs++;
							}
						}
						SelectionStart += numTabs;
					}
				}
			}
			mLastInfo.CursorLocation = SelectionStart;
			mLastInfo.ScrollPos = GetScrollPos();
			base.OnKeyUp(e);
		}

		protected override void OnVScroll(EventArgs e)
		{
			if (mParsing) return;
			base.OnVScroll (e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (SelectionStart < Text.Length && Text[SelectionStart] == '\t')
			{
				SelectionStart--;
				if (SelectionLength > 0)
				{
					SelectionLength++;
				}
			}
			if (SelectionStart + SelectionLength < Text.Length && Text[SelectionStart + SelectionLength] == '\t')
			{
				SelectionLength++;
			}
			HideAutoCompleteForm();
			base.OnMouseDown (e);
		}

		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			if (SelectionStart < Text.Length && Text[SelectionStart] == '\t')
			{
				SelectionStart--;
				if (SelectionLength > 0)
				{
					SelectionLength++;
				}
			}
			if (SelectionStart + SelectionLength < Text.Length && Text[SelectionStart + SelectionLength] == '\t')
			{
				SelectionLength++;
			}
			mLastInfo.CursorLocation = SelectionStart;
			mLastInfo.ScrollPos = GetScrollPos();
			base.OnMouseUp(mevent);
		}

		/// <summary>
		/// Taking care of Keyboard events
		/// </summary>
		/// <param name="m"></param>
		/// <remarks>
		/// Since even when overriding the OnKeyDown methoed and not calling the base function 
		/// you don't have full control of the input, I've decided to catch windows messages to handle them.
		/// </remarks>
		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case Win32.WM_PAINT:
				{
					//Don't draw the control while parsing to avoid flicker.
					if (mParsing)
					{
						return;
					}
					break;
				}
				case Win32.WM_KEYDOWN:
				{
					if (mAutoCompleteShown)
					{
						switch ((Keys)(int)m.WParam)
						{
							case Keys.Down:
							{
								if (mAutoCompleteForm.Items.Count != 0)
								{
									mAutoCompleteForm.SelectedIndex = (mAutoCompleteForm.SelectedIndex + 1) % mAutoCompleteForm.Items.Count;
								}
								return;
							}
							case Keys.Up:
							{
								if (mAutoCompleteForm.Items.Count != 0)
								{
									if (mAutoCompleteForm.SelectedIndex < 1)
									{
										mAutoCompleteForm.SelectedIndex = mAutoCompleteForm.Items.Count - 1;
									}
									else
									{
										mAutoCompleteForm.SelectedIndex--;
									}
								}
								return;
							}
							case Keys.Enter:
							case Keys.Space:
							{
								AcceptAutoCompleteItem();
								return;
							}
							case Keys.Escape:
							{
								HideAutoCompleteForm();
								return;
							}
								
						}
					}
					else
					{
						if (((Keys)(int)m.WParam == Keys.Space) && 
							((Win32.GetKeyState(Win32.VK_CONTROL) & Win32.KS_KEYDOWN) != 0))
						{
							// CompleteWord();
						} 
						else if (((Keys)(int)m.WParam == Keys.Z) && 
							((Win32.GetKeyState(Win32.VK_CONTROL) & Win32.KS_KEYDOWN) != 0))
						{
							Undo();
							return;
						}
						else if (((Keys)(int)m.WParam == Keys.Y) && 
							((Win32.GetKeyState(Win32.VK_CONTROL) & Win32.KS_KEYDOWN) != 0))
						{
							Redo();
							return;
						}
						else if ((Keys)(int)m.WParam == Keys.Left)
						{
							if (SelectionStart > 0 && Text[SelectionStart - 1] == '\t')
							{
								SelectionStart--;
							}
						}
						else if ((Keys)(int)m.WParam == Keys.Right)
						{
							if (SelectionStart < Text.Length - 1 && Text[SelectionStart + 1] == '\t')
							{
								SelectionStart++;
							}
						}
					}
					break;
				}
				case Win32.WM_KEYUP:
					switch ((Keys)(int)m.WParam)
					{
					case Keys.C:
						if ((Win32.GetKeyState(Win32.VK_CONTROL) & Win32.KS_KEYDOWN) != 0)
						{
							IDataObject iData = Clipboard.GetDataObject();
							if (iData.GetDataPresent(DataFormats.Text))
							{
								string contents = ((string)iData.GetData(DataFormats.UnicodeText)).Replace("»\t", "\t");
								Clipboard.SetDataObject(contents);
							}
						}
						break;
					}
					break;
				case Win32.WM_CHAR:
				{
					switch ((Keys)(int)m.WParam)
					{
						case Keys.Space:
							if ((Win32.GetKeyState(Win32.VK_CONTROL) & Win32.KS_KEYDOWN )!= 0)
							{
								return;
							}
							break;
						case Keys.Enter:
							if (mAutoCompleteShown) return;
							break;
					}
				}
				break;

			}
			base.WndProc (ref m);
		}


		/// <summary>
		/// Hides the AutoComplete form when losing focus on textbox.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnLostFocus(EventArgs e)
		{
			if (!mIgnoreLostFocus)
			{
				HideAutoCompleteForm();
			}
			base.OnLostFocus (e);
		}


		#endregion

		#region Undo/Redo Code
		public new bool CanUndo 
		{
			get 
			{
				return mUndoList.Count > 0;
			}
		}
		public new bool CanRedo
		{
			get 
			{
				return mRedoStack.Count > 0;
			}
		}

		private void LimitUndo()
		{
			while (mUndoList.Count > mMaxUndoRedoSteps)
			{
				mUndoList.RemoveAt(mMaxUndoRedoSteps);
			}
		}

		public new void Undo()
		{
			if (!CanUndo)
				return;
			mIsUndo = true;
			mRedoStack.Push(new UndoRedoInfo(Text, GetScrollPos(), SelectionStart));
			UndoRedoInfo info = (UndoRedoInfo)mUndoList[0];
			mUndoList.RemoveAt(0);
			Text = info.Text;
			SelectionStart = info.CursorLocation;
			SetScrollPos(info.ScrollPos);
			mLastInfo = info;
			mIsUndo = false;
		}
		public new void Redo()
		{
			if (!CanRedo)
				return;
			mIsUndo = true;
			mUndoList.Insert(0,new UndoRedoInfo(Text, GetScrollPos(), SelectionStart));
			LimitUndo();
			UndoRedoInfo info = (UndoRedoInfo)mRedoStack.Pop();
			Text = info.Text;
			SelectionStart = info.CursorLocation;
			SetScrollPos(info.ScrollPos);
			mLastInfo = info;
			mIsUndo = false;
		}

		private class UndoRedoInfo
		{
			public UndoRedoInfo(string text, Win32.POINT scrollPos, int cursorLoc)
			{
				Text = text;
				ScrollPos = scrollPos;
				CursorLocation = cursorLoc;
			}
			public Win32.POINT ScrollPos;
			public int CursorLocation;
			public readonly string Text;
		}
		#endregion

		#region AutoComplete functions

		/// <summary>
		/// Entry point to autocomplete mechanism.
		/// Tries to complete the current word. if it fails it shows the AutoComplete form.
		/// </summary>
		private void CompleteWord()
		{
			int curTokenStartIndex = Text.LastIndexOfAny(mSeperators.GetAsCharArray(), Math.Min(SelectionStart, Text.Length - 1))+1;
			int curTokenEndIndex= Text.IndexOfAny(mSeperators.GetAsCharArray(), SelectionStart);
			if (curTokenEndIndex == -1) 
			{
				curTokenEndIndex = Text.Length;
			}
			string curTokenString = Text.Substring(curTokenStartIndex, Math.Max(curTokenEndIndex - curTokenStartIndex,0)).ToUpper();
			
			string token = null;
			foreach (HighlightDescriptor hd in mHighlightDescriptors)
			{
				if (hd.UseForAutoComplete && hd.Token.ToUpper().StartsWith(curTokenString))
				{
					if (token == null)
					{
						token = hd.Token;
					}
					else
					{
						token = null;
						break;
					}
				}
			}
			if (token == null)
			{
				ShowAutoComplete();
			}
			else
			{
				SelectionStart = curTokenStartIndex;
				SelectionLength = curTokenEndIndex - curTokenStartIndex;
				SelectedText = token;
				SelectionStart = SelectionStart + SelectionLength;
				SelectionLength = 0;
			}
		}

		/// <summary>
		/// replace the current word of the cursor with the one from the AutoComplete form and closes it.
		/// </summary>
		/// <returns>If the operation was succesful</returns>
		private bool AcceptAutoCompleteItem()
		{
			
			if (mAutoCompleteForm.SelectedItem == null)
			{
				return false;
			}
			
			int curTokenStartIndex = Text.LastIndexOfAny(mSeperators.GetAsCharArray(), Math.Min(SelectionStart, Text.Length - 1)) + 1;
			int curTokenEndIndex= Text.IndexOfAny(mSeperators.GetAsCharArray(), SelectionStart);
			if (curTokenEndIndex == -1) 
			{
				curTokenEndIndex = Text.Length;
			}
			SelectionStart = Math.Max(curTokenStartIndex, 0);
			SelectionLength = Math.Max(0,curTokenEndIndex - curTokenStartIndex);
			SelectedText = mAutoCompleteForm.SelectedItem;
			SelectionStart = SelectionStart + SelectionLength;
			SelectionLength = 0;
			
			HideAutoCompleteForm();
			return true;
		}



		/// <summary>
		/// Finds the and sets the best matching token as the selected item in the AutoCompleteForm.
		/// </summary>
		private void SetBestSelectedAutoCompleteItem()
		{
			int curTokenStartIndex = Text.LastIndexOfAny(mSeperators.GetAsCharArray(), Math.Min(SelectionStart, Text.Length - 1))+1;
			int curTokenEndIndex= Text.IndexOfAny(mSeperators.GetAsCharArray(), SelectionStart);
			if (curTokenEndIndex == -1) 
			{
				curTokenEndIndex = Text.Length;
			}
			string curTokenString = Text.Substring(curTokenStartIndex, Math.Max(curTokenEndIndex - curTokenStartIndex,0)).ToUpper();
			
			if ((mAutoCompleteForm.SelectedItem != null) && 
				mAutoCompleteForm.SelectedItem.ToUpper().StartsWith(curTokenString))
			{
				return;
			}

			int matchingChars = -1;
			string bestMatchingToken = null;

			foreach (string item in mAutoCompleteForm.Items)
			{
				bool isWholeItemMatching = true;
				for (int i = 0 ; i < Math.Min(item.Length, curTokenString.Length); i++)
				{
					if (char.ToUpper(item[i]) != char.ToUpper(curTokenString[i]))
					{
						isWholeItemMatching = false;
						if (i-1 > matchingChars)
						{
							matchingChars = i;
							bestMatchingToken = item;
							break;
						}
					}
				}
				if (isWholeItemMatching &&
					(Math.Min(item.Length, curTokenString.Length) > matchingChars))
				{
					matchingChars = Math.Min(item.Length, curTokenString.Length);
					bestMatchingToken = item;
				}
			}
			
			if (bestMatchingToken != null)
			{
				mAutoCompleteForm.SelectedIndex = mAutoCompleteForm.Items.IndexOf(bestMatchingToken);
			}


		}

		/// <summary>
		/// Sets the items for the AutoComplete form.
		/// </summary>
		private void SetAutoCompleteItems()
		{
			mAutoCompleteForm.Items.Clear();
			string filterString = "";
			if (mFilterAutoComplete)
			{
			
				int filterTokenStartIndex = Text.LastIndexOfAny(mSeperators.GetAsCharArray(), Math.Min(SelectionStart, Text.Length - 1))+1;
				int filterTokenEndIndex= Text.IndexOfAny(mSeperators.GetAsCharArray(), SelectionStart);
				if (filterTokenEndIndex == -1) 
				{
					filterTokenEndIndex = Text.Length;
				}
				filterString = Text.Substring(filterTokenStartIndex, filterTokenEndIndex - filterTokenStartIndex).ToUpper();
			}
		
			foreach (HighlightDescriptor hd in mHighlightDescriptors)
			{
				if (hd.Token.ToUpper().StartsWith(filterString) && hd.UseForAutoComplete)
				{
					mAutoCompleteForm.Items.Add(hd.Token);
				}
			}
			mAutoCompleteForm.UpdateView();
		}
		
		/// <summary>
		/// Sets the size. the size is limited by the MaxSize property in the form itself.
		/// </summary>
		private void SetAutoCompleteSize()
		{
			mAutoCompleteForm.Height = Math.Min(
				Math.Max(mAutoCompleteForm.Items.Count, 1) * mAutoCompleteForm.ItemHeight + 4, 
				mAutoCompleteForm.MaximumSize.Height);
		}

		/// <summary>
		/// closes the AutoCompleteForm.
		/// </summary>
		private void HideAutoCompleteForm()
		{
			mAutoCompleteForm.Visible = false;
			mAutoCompleteShown = false;
		}
		

		/// <summary>
		/// Sets the location of the AutoComplete form, maiking sure it's on the screen where the cursor is.
		/// </summary>
		/// <param name="moveHorizontly">determines wheather or not to move the form horizontly.</param>
		private void SetAutoCompleteLocation(bool moveHorizontly)
		{
			Point cursorLocation = GetPositionFromCharIndex(SelectionStart);
			Screen screen = Screen.FromPoint(cursorLocation);
			Point optimalLocation = new Point(PointToScreen(cursorLocation).X-15, (int)(PointToScreen(cursorLocation).Y + Font.Size*2 + 2));
			Rectangle desiredPlace = new Rectangle(optimalLocation , mAutoCompleteForm.Size);
			desiredPlace.Width = 152;
			if (desiredPlace.Left < screen.Bounds.Left) 
			{
				desiredPlace.X = screen.Bounds.Left;
			}
			if (desiredPlace.Right > screen.Bounds.Right)
			{
				desiredPlace.X -= (desiredPlace.Right - screen.Bounds.Right);
			}
			if (desiredPlace.Bottom > screen.Bounds.Bottom)
			{
				desiredPlace.Y = cursorLocation.Y - 2 - desiredPlace.Height;
			}
			if (!moveHorizontly)
			{
				desiredPlace.X = mAutoCompleteForm.Left;
			}

			mAutoCompleteForm.Bounds = desiredPlace;
		}

		/// <summary>
		/// Shows the Autocomplete form.
		/// </summary>
		public void ShowAutoComplete()
		{
			SetAutoCompleteItems();
			SetAutoCompleteSize();
			SetAutoCompleteLocation(true);
			mIgnoreLostFocus = true;
			mAutoCompleteForm.Visible = true;
			SetBestSelectedAutoCompleteItem();
			mAutoCompleteShown = true;
			Focus();
			mIgnoreLostFocus = false;
		}

		#endregion 

		#region Rtf building helper functions

		/// <summary>
		/// Set color and font to default control settings.
		/// </summary>
		/// <param name="sb">the string builder building the RTF</param>
		/// <param name="colors">colors hashtable</param>
		/// <param name="fonts">fonts hashtable</param>
		private void SetDefaultSettings(StringBuilder sb, Hashtable colors, Hashtable fonts)
		{
			SetColor(sb, ForeColor, colors);
			SetFont(sb, Font, fonts);
			SetFontSize(sb, (int)Font.Size);
			EndTags(sb);
		}

		/// <summary>
		/// Set Color and font to a highlight descriptor settings.
		/// </summary>
		/// <param name="sb">the string builder building the RTF</param>
		/// <param name="hd">the HighlightDescriptor with the font and color settings to apply.</param>
		/// <param name="colors">colors hashtable</param>
		/// <param name="fonts">fonts hashtable</param>
		private void SetDescriptorSettings(StringBuilder sb, HighlightDescriptor hd, Hashtable colors, Hashtable fonts)
		{
			SetColor(sb, hd.Color, colors);
			if (hd.Font != null)
			{
				SetFont(sb, hd.Font, fonts);
				SetFontSize(sb, (int)hd.Font.Size);
			}
			EndTags(sb);

		}
		/// <summary>
		/// Sets the color to the specified color
		/// </summary>
		private void SetColor(StringBuilder sb, Color color, Hashtable colors)
		{
			sb.Append(@"\cf").Append(colors[color]);
		}
		/// <summary>
		/// Sets the backgroung color to the specified color.
		/// </summary>
		private void SetBackColor(StringBuilder sb, Color color, Hashtable colors)
		{
			sb.Append(@"\chshdng0\chcbpat").Append(colors[color]);
		}
		/// <summary>
		/// Sets the font to the specified font.
		/// </summary>
		private void SetFont(StringBuilder sb, Font font, Hashtable fonts)
		{
			if (font == null) return;
			sb.Append(@"\f").Append(fonts[font.Name]);
		}
		/// <summary>
		/// Sets the font size to the specified font size.
		/// </summary>
		private void SetFontSize(StringBuilder sb, int size)
		{
			sb.Append(@"\fs").Append(size*2);
		}
		/// <summary>
		/// Adds a newLine mark to the RTF.
		/// </summary>
		private void AddNewLine(StringBuilder sb)
		{
			sb.Append("\\par\n");
		}

		/// <summary>
		/// Ends a RTF tags section.
		/// </summary>
		private void EndTags(StringBuilder sb)
		{
			sb.Append(' ');
		}

		/// <summary>
		/// Adds a font to the RTF's font table and to the fonts hashtable.
		/// </summary>
		/// <param name="sb">The RTF's string builder</param>
		/// <param name="font">the Font to add</param>
		/// <param name="counter">a counter, containing the amount of fonts in the table</param>
		/// <param name="fonts">an hashtable. the key is the font's name. the value is it's index in the table</param>
		private void AddFontToTable(StringBuilder sb, Font font, ref int counter, Hashtable fonts)
		{
	
			sb.Append(@"\f").Append(counter).Append(@"\fnil\fcharset").Append(font.GdiCharSet).Append(font.Name).Append(";");
			fonts.Add(font.Name, counter++);
		}

		/// <summary>
		/// Adds a color to the RTF's color table and to the colors hashtable.
		/// </summary>
		/// <param name="sb">The RTF's string builder</param>
		/// <param name="color">the color to add</param>
		/// <param name="counter">a counter, containing the amount of colors in the table</param>
		/// <param name="colors">an hashtable. the key is the color. the value is it's index in the table</param>
		private void AddColorToTable(StringBuilder sb, Color color, ref int counter, Hashtable colors)
		{
	
			sb.Append(@"\red").Append(color.R).Append(@"\green").Append(color.G).Append(@"\blue")
				.Append(color.B).Append(";");
			colors.Add(color, counter++);
		}

		#endregion

		#region Scrollbar positions functions
		/// <summary>
		/// Sends a win32 message to get the scrollbars' position.
		/// </summary>
		/// <returns>a POINT structore containing horizontal and vertical scrollbar position.</returns>
		private unsafe Win32.POINT GetScrollPos()
		{
			Win32.POINT res = new Win32.POINT();
			IntPtr ptr = new IntPtr(&res);
			Win32.SendMessage(Handle, Win32.EM_GETSCROLLPOS, 0, ptr);
			return res;

		}

		/// <summary>
		/// Sends a win32 message to set scrollbars position.
		/// </summary>
		/// <param name="point">a POINT conatining H/Vscrollbar scrollpos.</param>
		private unsafe void SetScrollPos(Win32.POINT point)
		{
			IntPtr ptr = new IntPtr(&point);
			Win32.SendMessage(Handle, Win32.EM_SETSCROLLPOS, 0, ptr);

		}
		#endregion
	}

}