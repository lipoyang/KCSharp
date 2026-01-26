namespace KCSharp
{
    partial class FormMain
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
            pictureBoard = new PictureBox();
            label1 = new Label();
            comboMove = new ComboBox();
            label2 = new Label();
            textDepth = new TextBox();
            buttonStart = new Button();
            label3 = new Label();
            textTurn = new TextBox();
            comboGameType = new ComboBox();
            label4 = new Label();
            textGameNumber = new TextBox();
            labelGameNumber = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBoard).BeginInit();
            SuspendLayout();
            // 
            // pictureBoard
            // 
            pictureBoard.Location = new Point(20, 20);
            pictureBoard.Name = "pictureBoard";
            pictureBoard.Size = new Size(501, 501);
            pictureBoard.TabIndex = 0;
            pictureBoard.TabStop = false;
            pictureBoard.MouseClick += pictureBoard_MouseClick;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Yu Gothic UI", 16F);
            label1.Location = new Point(544, 43);
            label1.Name = "label1";
            label1.Size = new Size(125, 45);
            label1.TabIndex = 1;
            label1.Text = "あなたは";
            // 
            // comboMove
            // 
            comboMove.DropDownStyle = ComboBoxStyle.DropDownList;
            comboMove.Font = new Font("Yu Gothic UI", 16F);
            comboMove.FormattingEnabled = true;
            comboMove.Items.AddRange(new object[] { "先手", "後手" });
            comboMove.Location = new Point(710, 40);
            comboMove.Name = "comboMove";
            comboMove.Size = new Size(182, 53);
            comboMove.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Yu Gothic UI", 16F);
            label2.Location = new Point(544, 213);
            label2.Name = "label2";
            label2.Size = new Size(160, 45);
            label2.TabIndex = 3;
            label2.Text = "読みの深さ";
            // 
            // textDepth
            // 
            textDepth.Font = new Font("Yu Gothic UI", 16F);
            textDepth.Location = new Point(710, 214);
            textDepth.Name = "textDepth";
            textDepth.Size = new Size(182, 50);
            textDepth.TabIndex = 4;
            // 
            // buttonStart
            // 
            buttonStart.Font = new Font("Yu Gothic UI", 16F);
            buttonStart.Location = new Point(710, 294);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(182, 53);
            buttonStart.TabIndex = 5;
            buttonStart.Text = "対局開始";
            buttonStart.UseVisualStyleBackColor = true;
            buttonStart.Click += buttonStart_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Yu Gothic UI", 16F);
            label3.Location = new Point(544, 406);
            label3.Name = "label3";
            label3.Size = new Size(84, 45);
            label3.TabIndex = 6;
            label3.Text = "手番";
            // 
            // textTurn
            // 
            textTurn.BackColor = SystemColors.Control;
            textTurn.Font = new Font("Yu Gothic UI", 16F);
            textTurn.Location = new Point(710, 403);
            textTurn.Name = "textTurn";
            textTurn.ReadOnly = true;
            textTurn.Size = new Size(182, 50);
            textTurn.TabIndex = 7;
            // 
            // comboGameType
            // 
            comboGameType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboGameType.Font = new Font("Yu Gothic UI", 16F);
            comboGameType.FormattingEnabled = true;
            comboGameType.Items.AddRange(new object[] { "10番勝負", "ランダム" });
            comboGameType.Location = new Point(710, 99);
            comboGameType.Name = "comboGameType";
            comboGameType.Size = new Size(182, 53);
            comboGameType.TabIndex = 8;
            comboGameType.SelectedIndexChanged += comboGameType_SelectedIndexChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Yu Gothic UI", 16F);
            label4.Location = new Point(544, 99);
            label4.Name = "label4";
            label4.Size = new Size(84, 45);
            label4.TabIndex = 9;
            label4.Text = "配置";
            // 
            // textGameNumber
            // 
            textGameNumber.Font = new Font("Yu Gothic UI", 16F);
            textGameNumber.Location = new Point(710, 158);
            textGameNumber.Name = "textGameNumber";
            textGameNumber.Size = new Size(182, 50);
            textGameNumber.TabIndex = 11;
            // 
            // labelGameNumber
            // 
            labelGameNumber.AutoSize = true;
            labelGameNumber.Font = new Font("Yu Gothic UI", 16F);
            labelGameNumber.Location = new Point(628, 161);
            labelGameNumber.Name = "labelGameNumber";
            labelGameNumber.Size = new Size(63, 45);
            labelGameNumber.TabIndex = 10;
            labelGameNumber.Text = "No";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(928, 544);
            Controls.Add(textGameNumber);
            Controls.Add(labelGameNumber);
            Controls.Add(label4);
            Controls.Add(comboGameType);
            Controls.Add(textTurn);
            Controls.Add(label3);
            Controls.Add(buttonStart);
            Controls.Add(textDepth);
            Controls.Add(label2);
            Controls.Add(comboMove);
            Controls.Add(label1);
            Controls.Add(pictureBoard);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "FormMain";
            Text = "KCSharp";
            ((System.ComponentModel.ISupportInitialize)pictureBoard).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBoard;
        private Label label1;
        private ComboBox comboMove;
        private Label label2;
        private TextBox textDepth;
        private Button buttonStart;
        private Label label3;
        private TextBox textTurn;
        private ComboBox comboGameType;
        private Label label4;
        private TextBox textGameNumber;
        private Label labelGameNumber;
    }
}
