namespace twitterWordCloud
{
	partial class TweetForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tbTweets = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// tbTweets
			// 
			this.tbTweets.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tbTweets.Location = new System.Drawing.Point(0, 0);
			this.tbTweets.Multiline = true;
			this.tbTweets.Name = "tbTweets";
			this.tbTweets.Size = new System.Drawing.Size(494, 262);
			this.tbTweets.TabIndex = 0;
			this.tbTweets.Text = "Test";
			// 
			// TweetForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(494, 262);
			this.Controls.Add(this.tbTweets);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "TweetForm";
			this.Text = "TweetForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public System.Windows.Forms.TextBox tbTweets;

	}
}