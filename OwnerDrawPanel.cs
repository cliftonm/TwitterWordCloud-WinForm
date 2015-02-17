using System;
using System.Windows.Forms;

namespace twitterWordCloud
{
	public class OwnerDrawPanel : Panel
	{
		public OwnerDrawPanel()
		{
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
		}
	}
}
