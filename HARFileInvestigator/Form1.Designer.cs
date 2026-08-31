namespace HARFileInvestigator
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer? components = null;
        private TableLayoutPanel rootLayout = null!;
        private MenuStrip mainMenuStrip = null!;
        private ToolStripMenuItem fileToolStripMenuItem = null!;
        private ToolStripMenuItem saveHarFileToolStripMenuItem = null!;
        private Panel filterPanel = null!;
        private Button openButton = null!;
        private Button exportButton = null!;
        private Button timelineButton = null!;
        private Button applyQueryButton = null!;
        private Button clearQueryButton = null!;
        private Button filterToggleButton = null!;
        private Button tagButton = null!;
        private Button clearSessionsButton = null!;
        private Button tagMenuButton = null!;
        private Button previousMatchButton = null!;
        private Button nextMatchButton = null!;
        private CheckBox darkThemeCheckBox = null!;
        private Label searchLabel = null!;
        private ComboBox queryComboBox = null!;
        private SplitContainer splitContainer = null!;
        private SplitContainer rightSplitContainer = null!;
        private DataGridView entriesGrid = null!;
        private GroupBox requestGroupBox = null!;
        private GroupBox responseGroupBox = null!;
        private TabControl requestTabControl = null!;
        private TabPage requestRawTabPage = null!;
        private TabPage requestJwtTabPage = null!;
        private TabControl responseTabControl = null!;
        private TabPage responseRawTabPage = null!;
        private TabPage responseJwtTabPage = null!;
        private RichTextBox requestTextBox = null!;
        private RichTextBox responseTextBox = null!;
        private RichTextBox requestJwtTextBox = null!;
        private RichTextBox responseJwtTextBox = null!;
        private StatusStrip mainStatusStrip = null!;
        private ToolStripStatusLabel rowCountStatusLabel = null!;

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
            rootLayout = new TableLayoutPanel();
            filterPanel = new Panel();
            darkThemeCheckBox = new CheckBox();
            nextMatchButton = new Button();
            previousMatchButton = new Button();
            clearQueryButton = new Button();
            applyQueryButton = new Button();
            tagMenuButton = new Button();
            tagButton = new Button();
            clearSessionsButton = new Button();
            filterToggleButton = new Button();
            timelineButton = new Button();
            exportButton = new Button();
            queryComboBox = new ComboBox();
            searchLabel = new Label();
            openButton = new Button();
            splitContainer = new SplitContainer();
            entriesGrid = new DataGridView();
            rightSplitContainer = new SplitContainer();
            requestGroupBox = new GroupBox();
            requestTabControl = new TabControl();
            requestRawTabPage = new TabPage();
            requestTextBox = new RichTextBox();
            requestJwtTabPage = new TabPage();
            requestJwtTextBox = new RichTextBox();
            responseGroupBox = new GroupBox();
            responseTabControl = new TabControl();
            responseRawTabPage = new TabPage();
            responseTextBox = new RichTextBox();
            responseJwtTabPage = new TabPage();
            responseJwtTextBox = new RichTextBox();
            mainStatusStrip = new StatusStrip();
            rowCountStatusLabel = new ToolStripStatusLabel();
            mainMenuStrip = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            saveHarFileToolStripMenuItem = new ToolStripMenuItem();
            rootLayout.SuspendLayout();
            filterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)entriesGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)rightSplitContainer).BeginInit();
            rightSplitContainer.Panel1.SuspendLayout();
            rightSplitContainer.Panel2.SuspendLayout();
            rightSplitContainer.SuspendLayout();
            requestGroupBox.SuspendLayout();
            requestTabControl.SuspendLayout();
            requestRawTabPage.SuspendLayout();
            requestJwtTabPage.SuspendLayout();
            responseGroupBox.SuspendLayout();
            responseTabControl.SuspendLayout();
            responseRawTabPage.SuspendLayout();
            responseJwtTabPage.SuspendLayout();
            mainStatusStrip.SuspendLayout();
            mainMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // rootLayout
            // 
            rootLayout.ColumnCount = 1;
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootLayout.Controls.Add(filterPanel, 0, 0);
            rootLayout.Controls.Add(splitContainer, 0, 1);
            rootLayout.Dock = DockStyle.Fill;
            rootLayout.Location = new Point(0, 64);
            rootLayout.Margin = new Padding(9, 10, 9, 10);
            rootLayout.Name = "rootLayout";
            rootLayout.RowCount = 2;
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 262F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rootLayout.Size = new Size(3771, 2305);
            rootLayout.TabIndex = 0;
            // 
            // filterPanel
            // 
            filterPanel.Controls.Add(darkThemeCheckBox);
            filterPanel.Controls.Add(nextMatchButton);
            filterPanel.Controls.Add(previousMatchButton);
            filterPanel.Controls.Add(clearQueryButton);
            filterPanel.Controls.Add(applyQueryButton);
            filterPanel.Controls.Add(tagMenuButton);
            filterPanel.Controls.Add(tagButton);
            filterPanel.Controls.Add(clearSessionsButton);
            filterPanel.Controls.Add(filterToggleButton);
            filterPanel.Controls.Add(timelineButton);
            filterPanel.Controls.Add(exportButton);
            filterPanel.Controls.Add(queryComboBox);
            filterPanel.Controls.Add(searchLabel);
            filterPanel.Controls.Add(openButton);
            filterPanel.Dock = DockStyle.Fill;
            filterPanel.Location = new Point(9, 10);
            filterPanel.Margin = new Padding(9, 10, 9, 10);
            filterPanel.Name = "filterPanel";
            filterPanel.Size = new Size(3753, 242);
            filterPanel.TabIndex = 0;
            // 
            // darkThemeCheckBox
            // 
            darkThemeCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            darkThemeCheckBox.AutoSize = true;
            darkThemeCheckBox.Location = new Point(3610, 42);
            darkThemeCheckBox.Margin = new Padding(9, 10, 9, 10);
            darkThemeCheckBox.Name = "darkThemeCheckBox";
            darkThemeCheckBox.Size = new Size(140, 52);
            darkThemeCheckBox.TabIndex = 12;
            darkThemeCheckBox.Text = "Dark";
            darkThemeCheckBox.UseVisualStyleBackColor = true;
            darkThemeCheckBox.CheckedChanged += darkThemeCheckBox_CheckedChanged;
            // 
            // nextMatchButton
            // 
            nextMatchButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nextMatchButton.Enabled = false;
            nextMatchButton.Location = new Point(3513, 32);
            nextMatchButton.Margin = new Padding(9, 10, 9, 10);
            nextMatchButton.Name = "nextMatchButton";
            nextMatchButton.Size = new Size(77, 86);
            nextMatchButton.TabIndex = 7;
            nextMatchButton.Text = ">";
            nextMatchButton.UseVisualStyleBackColor = true;
            nextMatchButton.Click += nextMatchButton_Click;
            // 
            // previousMatchButton
            // 
            previousMatchButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            previousMatchButton.Enabled = false;
            previousMatchButton.Location = new Point(3419, 32);
            previousMatchButton.Margin = new Padding(9, 10, 9, 10);
            previousMatchButton.Name = "previousMatchButton";
            previousMatchButton.Size = new Size(77, 86);
            previousMatchButton.TabIndex = 6;
            previousMatchButton.Text = "<";
            previousMatchButton.UseVisualStyleBackColor = true;
            previousMatchButton.Click += previousMatchButton_Click;
            // 
            // clearQueryButton
            // 
            clearQueryButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            clearQueryButton.Location = new Point(3216, 32);
            clearQueryButton.Margin = new Padding(9, 10, 9, 10);
            clearQueryButton.Name = "clearQueryButton";
            clearQueryButton.Size = new Size(186, 86);
            clearQueryButton.TabIndex = 5;
            clearQueryButton.Text = "Clear";
            clearQueryButton.UseVisualStyleBackColor = true;
            clearQueryButton.Click += clearQueryButton_Click;
            // 
            // applyQueryButton
            // 
            applyQueryButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            applyQueryButton.Location = new Point(3013, 32);
            applyQueryButton.Margin = new Padding(9, 10, 9, 10);
            applyQueryButton.Name = "applyQueryButton";
            applyQueryButton.Size = new Size(186, 86);
            applyQueryButton.TabIndex = 4;
            applyQueryButton.Text = "Apply";
            applyQueryButton.UseVisualStyleBackColor = true;
            applyQueryButton.Click += applyQueryButton_Click;
            // 
            // tagMenuButton
            // 
            tagMenuButton.Location = new Point(1529, 138);
            tagMenuButton.Margin = new Padding(9, 10, 9, 10);
            tagMenuButton.Name = "tagMenuButton";
            tagMenuButton.Size = new Size(69, 77);
            tagMenuButton.TabIndex = 16;
            tagMenuButton.Text = "▼";
            tagMenuButton.UseVisualStyleBackColor = true;
            tagMenuButton.Click += tagMenuButton_Click;
            // 
            // tagButton
            // 
            tagButton.Location = new Point(1337, 138);
            tagButton.Margin = new Padding(9, 10, 9, 10);
            tagButton.Name = "tagButton";
            tagButton.Size = new Size(186, 77);
            tagButton.TabIndex = 15;
            tagButton.Text = "Tag";
            tagButton.UseVisualStyleBackColor = true;
            tagButton.Click += tagButton_Click;
            // 
            // clearSessionsButton
            // 
            clearSessionsButton.Location = new Point(1607, 138);
            clearSessionsButton.Margin = new Padding(9, 10, 9, 10);
            clearSessionsButton.Name = "clearSessionsButton";
            clearSessionsButton.Size = new Size(260, 77);
            clearSessionsButton.TabIndex = 17;
            clearSessionsButton.Text = "Clear Sessions";
            clearSessionsButton.UseVisualStyleBackColor = true;
            clearSessionsButton.Click += clearSessionsButton_Click;
            // 
            // filterToggleButton
            // 
            filterToggleButton.Location = new Point(1020, 138);
            filterToggleButton.Margin = new Padding(9, 10, 9, 10);
            filterToggleButton.Name = "filterToggleButton";
            filterToggleButton.Size = new Size(300, 77);
            filterToggleButton.TabIndex = 13;
            filterToggleButton.Text = "Filter";
            filterToggleButton.UseVisualStyleBackColor = true;
            filterToggleButton.Click += filterToggleButton_Click;
            // 
            // timelineButton
            // 
            timelineButton.Location = new Point(623, 32);
            timelineButton.Margin = new Padding(9, 10, 9, 10);
            timelineButton.Name = "timelineButton";
            timelineButton.Size = new Size(234, 86);
            timelineButton.TabIndex = 2;
            timelineButton.Text = "Timeline";
            timelineButton.UseVisualStyleBackColor = true;
            timelineButton.Click += timelineButton_Click;
            // 
            // exportButton
            // 
            exportButton.Location = new Point(331, 32);
            exportButton.Margin = new Padding(9, 10, 9, 10);
            exportButton.Name = "exportButton";
            exportButton.Size = new Size(274, 86);
            exportButton.TabIndex = 1;
            exportButton.Text = "Export CSV...";
            exportButton.UseVisualStyleBackColor = true;
            exportButton.Click += exportButton_Click;
            // 
            // queryComboBox
            // 
            queryComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            queryComboBox.FormattingEnabled = true;
            queryComboBox.Location = new Point(1020, 32);
            queryComboBox.Margin = new Padding(9, 10, 9, 10);
            queryComboBox.Name = "queryComboBox";
            queryComboBox.Size = new Size(1969, 56);
            queryComboBox.TabIndex = 3;
            queryComboBox.SelectionChangeCommitted += queryHistoryComboBox_SelectionChangeCommitted;
            // 
            // searchLabel
            // 
            searchLabel.AutoSize = true;
            searchLabel.Location = new Point(874, 45);
            searchLabel.Margin = new Padding(9, 0, 9, 0);
            searchLabel.Name = "searchLabel";
            searchLabel.Size = new Size(124, 48);
            searchLabel.TabIndex = 6;
            searchLabel.Text = "Query:";
            // 
            // openButton
            // 
            openButton.Location = new Point(29, 32);
            openButton.Margin = new Padding(9, 10, 9, 10);
            openButton.Name = "openButton";
            openButton.Size = new Size(286, 86);
            openButton.TabIndex = 0;
            openButton.Text = "Open HAR...";
            openButton.UseVisualStyleBackColor = true;
            openButton.Click += openButton_Click;
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point(9, 272);
            splitContainer.Margin = new Padding(9, 10, 9, 10);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(entriesGrid);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(rightSplitContainer);
            splitContainer.Size = new Size(3753, 2023);
            splitContainer.SplitterDistance = 2170;
            splitContainer.SplitterWidth = 11;
            splitContainer.TabIndex = 1;
            // 
            // entriesGrid
            // 
            entriesGrid.AllowUserToAddRows = false;
            entriesGrid.AllowUserToDeleteRows = false;
            entriesGrid.AllowUserToOrderColumns = true;
            entriesGrid.BackgroundColor = SystemColors.Window;
            entriesGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            entriesGrid.Dock = DockStyle.Fill;
            entriesGrid.Location = new Point(0, 0);
            entriesGrid.Margin = new Padding(9, 10, 9, 10);
            entriesGrid.Name = "entriesGrid";
            entriesGrid.ReadOnly = true;
            entriesGrid.RowHeadersVisible = false;
            entriesGrid.RowHeadersWidth = 123;
            entriesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            entriesGrid.Size = new Size(2170, 2023);
            entriesGrid.TabIndex = 0;
            entriesGrid.SelectionChanged += entriesGrid_SelectionChanged;
            // 
            // rightSplitContainer
            // 
            rightSplitContainer.Dock = DockStyle.Fill;
            rightSplitContainer.Location = new Point(0, 0);
            rightSplitContainer.Margin = new Padding(9, 10, 9, 10);
            rightSplitContainer.Name = "rightSplitContainer";
            rightSplitContainer.Orientation = Orientation.Horizontal;
            // 
            // rightSplitContainer.Panel1
            // 
            rightSplitContainer.Panel1.Controls.Add(requestGroupBox);
            // 
            // rightSplitContainer.Panel2
            // 
            rightSplitContainer.Panel2.Controls.Add(responseGroupBox);
            rightSplitContainer.Size = new Size(1572, 2023);
            rightSplitContainer.SplitterDistance = 979;
            rightSplitContainer.SplitterWidth = 13;
            rightSplitContainer.TabIndex = 0;
            // 
            // requestGroupBox
            // 
            requestGroupBox.Controls.Add(requestTabControl);
            requestGroupBox.Dock = DockStyle.Fill;
            requestGroupBox.Location = new Point(0, 0);
            requestGroupBox.Margin = new Padding(9, 10, 9, 10);
            requestGroupBox.Name = "requestGroupBox";
            requestGroupBox.Padding = new Padding(9, 10, 9, 10);
            requestGroupBox.Size = new Size(1572, 979);
            requestGroupBox.TabIndex = 0;
            requestGroupBox.TabStop = false;
            requestGroupBox.Text = "Request";
            // 
            // requestTabControl
            // 
            requestTabControl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            requestTabControl.Controls.Add(requestRawTabPage);
            requestTabControl.Controls.Add(requestJwtTabPage);
            requestTabControl.Location = new Point(9, 95);
            requestTabControl.Margin = new Padding(9, 10, 9, 10);
            requestTabControl.Name = "requestTabControl";
            requestTabControl.SelectedIndex = 0;
            requestTabControl.Size = new Size(1554, 904);
            requestTabControl.TabIndex = 0;
            // 
            // requestRawTabPage
            // 
            requestRawTabPage.Controls.Add(requestTextBox);
            requestRawTabPage.Location = new Point(12, 69);
            requestRawTabPage.Margin = new Padding(9, 10, 9, 10);
            requestRawTabPage.Name = "requestRawTabPage";
            requestRawTabPage.Padding = new Padding(9, 10, 9, 10);
            requestRawTabPage.Size = new Size(1530, 823);
            requestRawTabPage.TabIndex = 0;
            requestRawTabPage.Text = "Raw";
            requestRawTabPage.UseVisualStyleBackColor = true;
            // 
            // requestTextBox
            // 
            requestTextBox.Dock = DockStyle.Fill;
            requestTextBox.Font = new Font("Consolas", 9F);
            requestTextBox.Location = new Point(9, 10);
            requestTextBox.Margin = new Padding(9, 10, 9, 10);
            requestTextBox.Name = "requestTextBox";
            requestTextBox.ReadOnly = true;
            requestTextBox.Size = new Size(1512, 803);
            requestTextBox.TabIndex = 0;
            requestTextBox.Text = "";
            requestTextBox.WordWrap = false;
            // 
            // requestJwtTabPage
            // 
            requestJwtTabPage.Controls.Add(requestJwtTextBox);
            requestJwtTabPage.Location = new Point(12, 69);
            requestJwtTabPage.Margin = new Padding(9, 10, 9, 10);
            requestJwtTabPage.Name = "requestJwtTabPage";
            requestJwtTabPage.Padding = new Padding(9, 10, 9, 10);
            requestJwtTabPage.Size = new Size(1530, 892);
            requestJwtTabPage.TabIndex = 1;
            requestJwtTabPage.Text = "JWT";
            requestJwtTabPage.UseVisualStyleBackColor = true;
            // 
            // requestJwtTextBox
            // 
            requestJwtTextBox.Dock = DockStyle.Fill;
            requestJwtTextBox.Font = new Font("Consolas", 9F);
            requestJwtTextBox.Location = new Point(9, 10);
            requestJwtTextBox.Margin = new Padding(9, 10, 9, 10);
            requestJwtTextBox.Name = "requestJwtTextBox";
            requestJwtTextBox.ReadOnly = true;
            requestJwtTextBox.Size = new Size(1512, 872);
            requestJwtTextBox.TabIndex = 0;
            requestJwtTextBox.Text = "";
            requestJwtTextBox.WordWrap = false;
            // 
            // responseGroupBox
            // 
            responseGroupBox.Controls.Add(responseTabControl);
            responseGroupBox.Dock = DockStyle.Fill;
            responseGroupBox.Location = new Point(0, 0);
            responseGroupBox.Margin = new Padding(9, 10, 9, 10);
            responseGroupBox.Name = "responseGroupBox";
            responseGroupBox.Padding = new Padding(9, 10, 9, 10);
            responseGroupBox.Size = new Size(1572, 1031);
            responseGroupBox.TabIndex = 0;
            responseGroupBox.TabStop = false;
            responseGroupBox.Text = "Response";
            // 
            // responseTabControl
            // 
            responseTabControl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            responseTabControl.Controls.Add(responseRawTabPage);
            responseTabControl.Controls.Add(responseJwtTabPage);
            responseTabControl.Location = new Point(9, 106);
            responseTabControl.Margin = new Padding(9, 10, 9, 10);
            responseTabControl.Name = "responseTabControl";
            responseTabControl.SelectedIndex = 0;
            responseTabControl.Size = new Size(1554, 947);
            responseTabControl.TabIndex = 0;
            // 
            // responseRawTabPage
            // 
            responseRawTabPage.Controls.Add(responseTextBox);
            responseRawTabPage.Location = new Point(12, 69);
            responseRawTabPage.Margin = new Padding(9, 10, 9, 10);
            responseRawTabPage.Name = "responseRawTabPage";
            responseRawTabPage.Padding = new Padding(9, 10, 9, 10);
            responseRawTabPage.Size = new Size(1530, 866);
            responseRawTabPage.TabIndex = 0;
            responseRawTabPage.Text = "Raw";
            responseRawTabPage.UseVisualStyleBackColor = true;
            // 
            // responseTextBox
            // 
            responseTextBox.Dock = DockStyle.Fill;
            responseTextBox.Font = new Font("Consolas", 9F);
            responseTextBox.Location = new Point(9, 10);
            responseTextBox.Margin = new Padding(9, 10, 9, 10);
            responseTextBox.Name = "responseTextBox";
            responseTextBox.ReadOnly = true;
            responseTextBox.Size = new Size(1512, 846);
            responseTextBox.TabIndex = 0;
            responseTextBox.Text = "";
            responseTextBox.WordWrap = false;
            // 
            // responseJwtTabPage
            // 
            responseJwtTabPage.Controls.Add(responseJwtTextBox);
            responseJwtTabPage.Location = new Point(12, 69);
            responseJwtTabPage.Margin = new Padding(9, 10, 9, 10);
            responseJwtTabPage.Name = "responseJwtTabPage";
            responseJwtTabPage.Padding = new Padding(9, 10, 9, 10);
            responseJwtTabPage.Size = new Size(1530, 854);
            responseJwtTabPage.TabIndex = 1;
            responseJwtTabPage.Text = "JWT";
            responseJwtTabPage.UseVisualStyleBackColor = true;
            // 
            // responseJwtTextBox
            // 
            responseJwtTextBox.Dock = DockStyle.Fill;
            responseJwtTextBox.Font = new Font("Consolas", 9F);
            responseJwtTextBox.Location = new Point(9, 10);
            responseJwtTextBox.Margin = new Padding(9, 10, 9, 10);
            responseJwtTextBox.Name = "responseJwtTextBox";
            responseJwtTextBox.ReadOnly = true;
            responseJwtTextBox.Size = new Size(1512, 834);
            responseJwtTextBox.TabIndex = 0;
            responseJwtTextBox.Text = "";
            responseJwtTextBox.WordWrap = false;
            // 
            // mainStatusStrip
            // 
            mainStatusStrip.ImageScalingSize = new Size(48, 48);
            mainStatusStrip.Items.AddRange(new ToolStripItem[] { rowCountStatusLabel });
            mainStatusStrip.Location = new Point(0, 2369);
            mainStatusStrip.Name = "mainStatusStrip";
            mainStatusStrip.Padding = new Padding(3, 0, 41, 0);
            mainStatusStrip.Size = new Size(3771, 63);
            mainStatusStrip.TabIndex = 2;
            // 
            // rowCountStatusLabel
            // 
            rowCountStatusLabel.Name = "rowCountStatusLabel";
            rowCountStatusLabel.Size = new Size(594, 48);
            rowCountStatusLabel.Text = "Rows: 0 | Selected: 0 | Highlighted: 0";
            // 
            // mainMenuStrip
            // 
            mainMenuStrip.ImageScalingSize = new Size(48, 48);
            mainMenuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            mainMenuStrip.Location = new Point(0, 0);
            mainMenuStrip.Name = "mainMenuStrip";
            mainMenuStrip.Padding = new Padding(17, 6, 0, 6);
            mainMenuStrip.Size = new Size(3771, 64);
            mainMenuStrip.TabIndex = 1;
            mainMenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { saveHarFileToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(103, 52);
            fileToolStripMenuItem.Text = "File";
            // 
            // saveHarFileToolStripMenuItem
            // 
            saveHarFileToolStripMenuItem.Name = "saveHarFileToolStripMenuItem";
            saveHarFileToolStripMenuItem.Size = new Size(461, 66);
            saveHarFileToolStripMenuItem.Text = "Save HAR File...";
            saveHarFileToolStripMenuItem.Click += saveHarFileToolStripMenuItem_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(20F, 48F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(3771, 2432);
            Controls.Add(rootLayout);
            Controls.Add(mainStatusStrip);
            Controls.Add(mainMenuStrip);
            MainMenuStrip = mainMenuStrip;
            Margin = new Padding(9, 10, 9, 10);
            MinimumSize = new Size(3076, 1853);
            Name = "Form1";
            Text = "HAR File Investigator";
            FormClosing += Form1_FormClosing;
            rootLayout.ResumeLayout(false);
            filterPanel.ResumeLayout(false);
            filterPanel.PerformLayout();
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)entriesGrid).EndInit();
            rightSplitContainer.Panel1.ResumeLayout(false);
            rightSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)rightSplitContainer).EndInit();
            rightSplitContainer.ResumeLayout(false);
            requestGroupBox.ResumeLayout(false);
            requestTabControl.ResumeLayout(false);
            requestRawTabPage.ResumeLayout(false);
            requestJwtTabPage.ResumeLayout(false);
            responseGroupBox.ResumeLayout(false);
            responseTabControl.ResumeLayout(false);
            responseRawTabPage.ResumeLayout(false);
            responseJwtTabPage.ResumeLayout(false);
            mainStatusStrip.ResumeLayout(false);
            mainStatusStrip.PerformLayout();
            mainMenuStrip.ResumeLayout(false);
            mainMenuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
