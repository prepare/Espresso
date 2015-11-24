namespace VRoomJsBrezzaTest
{
    partial class BrezzaTestForm
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
            this.js_input = new System.Windows.Forms.TextBox();
            this.js_output = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.run_js = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // js_input
            // 
            this.js_input.Location = new System.Drawing.Point(13, 35);
            this.js_input.Multiline = true;
            this.js_input.Name = "js_input";
            this.js_input.Size = new System.Drawing.Size(340, 510);
            this.js_input.TabIndex = 0;
            // 
            // js_output
            // 
            this.js_output.Location = new System.Drawing.Point(486, 35);
            this.js_output.Multiline = true;
            this.js_output.Name = "js_output";
            this.js_output.Size = new System.Drawing.Size(340, 510);
            this.js_output.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(488, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Result";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Javascript Input";
            // 
            // run_js
            // 
            this.run_js.Location = new System.Drawing.Point(359, 35);
            this.run_js.Name = "run_js";
            this.run_js.Size = new System.Drawing.Size(121, 23);
            this.run_js.TabIndex = 4;
            this.run_js.Text = "Run JavaScript";
            this.run_js.UseVisualStyleBackColor = true;
            this.run_js.Click += new System.EventHandler(this.run_js_Click);
            // 
            // BrezzaTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(838, 561);
            this.Controls.Add(this.run_js);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.js_output);
            this.Controls.Add(this.js_input);
            this.Name = "BrezzaTestForm";
            this.Text = "Brezza Test Form";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox js_input;
        private System.Windows.Forms.TextBox js_output;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button run_js;
    }
}

