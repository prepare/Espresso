namespace EasePatcherWinForm
{
    partial class Form1
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
            this.cmdPatchEspresso = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.cmdPatch = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmdPatchEspresso
            // 
            this.cmdPatchEspresso.Location = new System.Drawing.Point(12, 12);
            this.cmdPatchEspresso.Name = "cmdPatchEspresso";
            this.cmdPatchEspresso.Size = new System.Drawing.Size(153, 38);
            this.cmdPatchEspresso.TabIndex = 0;
            this.cmdPatchEspresso.Text = "Init Build";
            this.cmdPatchEspresso.UseVisualStyleBackColor = true;
            this.cmdPatchEspresso.Click += new System.EventHandler(this.cmdPatchEspresso_Click);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(172, 13);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(410, 227);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // cmdPatch
            // 
            this.cmdPatch.Location = new System.Drawing.Point(13, 56);
            this.cmdPatch.Name = "cmdPatch";
            this.cmdPatch.Size = new System.Drawing.Size(153, 38);
            this.cmdPatch.TabIndex = 2;
            this.cmdPatch.Text = "Patch with Custom Code";
            this.cmdPatch.UseVisualStyleBackColor = true;
            this.cmdPatch.Click += new System.EventHandler(this.cmdPatch_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(594, 272);
            this.Controls.Add(this.cmdPatch);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.cmdPatchEspresso);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cmdPatchEspresso;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button cmdPatch;
    }
}

