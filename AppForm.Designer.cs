namespace twitterWordCloud
{
	partial class AppForm
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
			this.pnlCloud = new twitterWordCloud.OwnerDrawPanel();
			this.label1 = new System.Windows.Forms.Label();
			this.tbKeyword = new System.Windows.Forms.TextBox();
			this.btnGo = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.nudMinCount = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.nudMinCount)).BeginInit();
			this.SuspendLayout();
			// 
			// pnlCloud
			// 
			this.pnlCloud.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pnlCloud.Location = new System.Drawing.Point(0, 32);
			this.pnlCloud.Name = "pnlCloud";
			this.pnlCloud.Size = new System.Drawing.Size(618, 435);
			this.pnlCloud.TabIndex = 0;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(51, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Keyword:";
			// 
			// tbKeyword
			// 
			this.tbKeyword.Location = new System.Drawing.Point(57, 6);
			this.tbKeyword.Name = "tbKeyword";
			this.tbKeyword.Size = new System.Drawing.Size(135, 20);
			this.tbKeyword.TabIndex = 2;
			// 
			// btnGo
			// 
			this.btnGo.Location = new System.Drawing.Point(198, 4);
			this.btnGo.Name = "btnGo";
			this.btnGo.Size = new System.Drawing.Size(75, 23);
			this.btnGo.TabIndex = 3;
			this.btnGo.Text = "Go!";
			this.btnGo.UseVisualStyleBackColor = true;
			this.btnGo.Click += new System.EventHandler(this.OnGo);
			// 
			// btnStop
			// 
			this.btnStop.Location = new System.Drawing.Point(279, 4);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(75, 23);
			this.btnStop.TabIndex = 4;
			this.btnStop.Text = "Stop";
			this.btnStop.UseVisualStyleBackColor = true;
			this.btnStop.Click += new System.EventHandler(this.OnStop);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(396, 9);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(44, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "Count >";
			// 
			// nudMinCount
			// 
			this.nudMinCount.Location = new System.Drawing.Point(446, 6);
			this.nudMinCount.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
			this.nudMinCount.Name = "nudMinCount";
			this.nudMinCount.Size = new System.Drawing.Size(42, 20);
			this.nudMinCount.TabIndex = 6;
			this.nudMinCount.ValueChanged += new System.EventHandler(this.OnMinCountChanged);
			// 
			// AppForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(618, 467);
			this.Controls.Add(this.nudMinCount);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnStop);
			this.Controls.Add(this.btnGo);
			this.Controls.Add(this.tbKeyword);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pnlCloud);
			this.Name = "AppForm";
			this.Text = "Twitter Word Cloud";
			((System.ComponentModel.ISupportInitialize)(this.nudMinCount)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OwnerDrawPanel pnlCloud;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbKeyword;
		private System.Windows.Forms.Button btnGo;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown nudMinCount;
	}
}

