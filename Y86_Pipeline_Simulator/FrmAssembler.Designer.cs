namespace Y86_Pipeline_Simulator
{
    partial class FrmAssembler
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmAssembler));
            this.rtbxInput = new System.Windows.Forms.RichTextBox();
            this.btnAssemble = new System.Windows.Forms.Button();
            this.btnDisassembler = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnOutputFile = new System.Windows.Forms.Button();
            this.btnOutputOK = new System.Windows.Forms.Button();
            this.txtOutputFile = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnInputFile = new System.Windows.Forms.Button();
            this.btnInputOK = new System.Windows.Forms.Button();
            this.txtInputFile = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.rtbxOutput = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.odlgInput = new System.Windows.Forms.OpenFileDialog();
            this.sdlgOutput = new System.Windows.Forms.SaveFileDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // rtbxInput
            // 
            this.rtbxInput.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rtbxInput.Location = new System.Drawing.Point(13, 217);
            this.rtbxInput.Name = "rtbxInput";
            this.rtbxInput.Size = new System.Drawing.Size(185, 310);
            this.rtbxInput.TabIndex = 0;
            this.rtbxInput.Text = "";
            this.rtbxInput.WordWrap = false;
            // 
            // btnAssemble
            // 
            this.btnAssemble.Location = new System.Drawing.Point(29, 19);
            this.btnAssemble.Name = "btnAssemble";
            this.btnAssemble.Size = new System.Drawing.Size(110, 30);
            this.btnAssemble.TabIndex = 1;
            this.btnAssemble.Text = "汇编";
            this.btnAssemble.UseVisualStyleBackColor = true;
            this.btnAssemble.Click += new System.EventHandler(this.btnAssemble_Click);
            // 
            // btnDisassembler
            // 
            this.btnDisassembler.Location = new System.Drawing.Point(145, 19);
            this.btnDisassembler.Name = "btnDisassembler";
            this.btnDisassembler.Size = new System.Drawing.Size(110, 30);
            this.btnDisassembler.TabIndex = 2;
            this.btnDisassembler.Text = "反汇编";
            this.btnDisassembler.UseVisualStyleBackColor = true;
            this.btnDisassembler.Click += new System.EventHandler(this.btnDisassembler_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(261, 19);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(110, 30);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "清空";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(377, 19);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(110, 30);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "退出";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 5;
            this.label1.Text = "文件路径:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnAssemble);
            this.groupBox1.Controls.Add(this.btnDisassembler);
            this.groupBox1.Controls.Add(this.btnClear);
            this.groupBox1.Controls.Add(this.btnClose);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(505, 59);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "控制面板";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnOutputFile);
            this.groupBox2.Controls.Add(this.btnOutputOK);
            this.groupBox2.Controls.Add(this.txtOutputFile);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(12, 135);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(505, 52);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "输出结果到:";
            // 
            // btnOutputFile
            // 
            this.btnOutputFile.Location = new System.Drawing.Point(370, 17);
            this.btnOutputFile.Name = "btnOutputFile";
            this.btnOutputFile.Size = new System.Drawing.Size(33, 23);
            this.btnOutputFile.TabIndex = 7;
            this.btnOutputFile.Text = "…";
            this.btnOutputFile.UseVisualStyleBackColor = true;
            this.btnOutputFile.Click += new System.EventHandler(this.btnOutputFile_Click);
            // 
            // btnOutputOK
            // 
            this.btnOutputOK.Location = new System.Drawing.Point(409, 17);
            this.btnOutputOK.Name = "btnOutputOK";
            this.btnOutputOK.Size = new System.Drawing.Size(75, 23);
            this.btnOutputOK.TabIndex = 5;
            this.btnOutputOK.Text = "输出";
            this.btnOutputOK.UseVisualStyleBackColor = true;
            this.btnOutputOK.Click += new System.EventHandler(this.btnOutputOK_Click);
            // 
            // txtOutputFile
            // 
            this.txtOutputFile.Location = new System.Drawing.Point(78, 19);
            this.txtOutputFile.Name = "txtOutputFile";
            this.txtOutputFile.Size = new System.Drawing.Size(286, 21);
            this.txtOutputFile.TabIndex = 6;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnInputFile);
            this.groupBox3.Controls.Add(this.btnInputOK);
            this.groupBox3.Controls.Add(this.txtInputFile);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Location = new System.Drawing.Point(12, 77);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(505, 52);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "选择源文件:";
            // 
            // btnInputFile
            // 
            this.btnInputFile.Location = new System.Drawing.Point(370, 17);
            this.btnInputFile.Name = "btnInputFile";
            this.btnInputFile.Size = new System.Drawing.Size(33, 23);
            this.btnInputFile.TabIndex = 7;
            this.btnInputFile.Text = "…";
            this.btnInputFile.UseVisualStyleBackColor = true;
            this.btnInputFile.Click += new System.EventHandler(this.btnInputFile_Click);
            // 
            // btnInputOK
            // 
            this.btnInputOK.Location = new System.Drawing.Point(409, 17);
            this.btnInputOK.Name = "btnInputOK";
            this.btnInputOK.Size = new System.Drawing.Size(75, 23);
            this.btnInputOK.TabIndex = 5;
            this.btnInputOK.Text = "输入";
            this.btnInputOK.UseVisualStyleBackColor = true;
            this.btnInputOK.Click += new System.EventHandler(this.btnInputOK_Click);
            // 
            // txtInputFile
            // 
            this.txtInputFile.Location = new System.Drawing.Point(78, 19);
            this.txtInputFile.Name = "txtInputFile";
            this.txtInputFile.Size = new System.Drawing.Size(286, 21);
            this.txtInputFile.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "文件路径:";
            // 
            // rtbxOutput
            // 
            this.rtbxOutput.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rtbxOutput.Location = new System.Drawing.Point(204, 217);
            this.rtbxOutput.Name = "rtbxOutput";
            this.rtbxOutput.ReadOnly = true;
            this.rtbxOutput.Size = new System.Drawing.Size(313, 310);
            this.rtbxOutput.TabIndex = 9;
            this.rtbxOutput.Text = "";
            this.rtbxOutput.WordWrap = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(88, 199);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 10;
            this.label3.Text = "输入框";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(342, 199);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "输出框";
            // 
            // odlgInput
            // 
            this.odlgInput.FileName = "openFileDialog1";
            // 
            // FrmAssembler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(529, 539);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.rtbxOutput);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.rtbxInput);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmAssembler";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Y86汇编器";
            this.Load += new System.EventHandler(this.FrmAssembler_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbxInput;
        private System.Windows.Forms.Button btnAssemble;
        private System.Windows.Forms.Button btnDisassembler;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnOutputFile;
        private System.Windows.Forms.Button btnOutputOK;
        private System.Windows.Forms.TextBox txtOutputFile;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnInputFile;
        private System.Windows.Forms.Button btnInputOK;
        private System.Windows.Forms.TextBox txtInputFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox rtbxOutput;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.OpenFileDialog odlgInput;
        private System.Windows.Forms.SaveFileDialog sdlgOutput;
    }
}