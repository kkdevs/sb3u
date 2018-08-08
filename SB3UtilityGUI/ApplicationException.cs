using System;
using System.Windows.Forms;

namespace SB3Utility
{
	public partial class ApplicationException : Form
	{
		public ApplicationException(Exception ex)
		{
			InitializeComponent();
			textBoxErrorText.Text = ex.ToString();
		}
	}
}
