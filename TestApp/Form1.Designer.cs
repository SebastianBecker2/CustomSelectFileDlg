namespace TestApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            SelectFolderDialogResult = new TextBox();
            SelectFileDialogResult = new TextBox();
            SaveFileDialogResult = new TextBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(12, 12);
            button1.Name = "button1";
            button1.Size = new Size(186, 63);
            button1.TabIndex = 0;
            button1.Text = "Select Folder Dialog...";
            button1.UseVisualStyleBackColor = true;
            button1.Click += SelectFolderDialog_Click;
            // 
            // button2
            // 
            button2.Location = new Point(12, 81);
            button2.Name = "button2";
            button2.Size = new Size(186, 63);
            button2.TabIndex = 1;
            button2.Text = "Select File Dialog...";
            button2.UseVisualStyleBackColor = true;
            button2.Click += SelectFileDialog_Click;
            // 
            // button3
            // 
            button3.Location = new Point(12, 150);
            button3.Name = "button3";
            button3.Size = new Size(186, 63);
            button3.TabIndex = 2;
            button3.Text = "Save File Dialog...";
            button3.UseVisualStyleBackColor = true;
            button3.Click += SaveFileDialog_Click;
            // 
            // SelectFolderDialogResult
            // 
            SelectFolderDialogResult.Location = new Point(204, 33);
            SelectFolderDialogResult.Name = "SelectFolderDialogResult";
            SelectFolderDialogResult.Size = new Size(363, 23);
            SelectFolderDialogResult.TabIndex = 3;
            // 
            // SelectFileDialogResult
            // 
            SelectFileDialogResult.Location = new Point(204, 102);
            SelectFileDialogResult.Name = "SelectFileDialogResult";
            SelectFileDialogResult.Size = new Size(363, 23);
            SelectFileDialogResult.TabIndex = 4;
            // 
            // SaveFileDialogResult
            // 
            SaveFileDialogResult.Location = new Point(204, 171);
            SaveFileDialogResult.Name = "SaveFileDialogResult";
            SaveFileDialogResult.Size = new Size(363, 23);
            SaveFileDialogResult.TabIndex = 5;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(580, 223);
            Controls.Add(SaveFileDialogResult);
            Controls.Add(SelectFileDialogResult);
            Controls.Add(SelectFolderDialogResult);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private Button button3;
        private TextBox SelectFolderDialogResult;
        private TextBox SelectFileDialogResult;
        private TextBox SaveFileDialogResult;
    }
}
