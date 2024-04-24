using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace MaterialSkin.Controls
{
    public partial class MaterialDataTable : MaterialListView, IMaterialControl
    {
        private ListViewItem[] _items;
        [Browsable(false)]
        public ListViewItem[] DataItems {
            set {
                _items = value;
                Items.AddRange(_items);
                ItemsCounterText();
            }
        }

        private MaterialLabel _itemCountLabel, _headerLabel;
        private FlowLayoutPanel _headerPanel, _footerPanel;
        private MaterialTextBox2 _searchTextBox;
        private string _heading;

        [DefaultValue("")]
        [Category("Behavior"), Browsable(true)]
        [Description("Set header title")]
        public string Heading { get { return _heading; } set { _heading=value; } }


        public MaterialDataTable()
        {
            InitializeHeader();
            InitializeFooter();
        }

        private void InitializeHeader()
        {
            _headerPanel = new FlowLayoutPanel();
            _headerPanel.Dock = DockStyle.Top;
            _headerPanel.Height = 50;
            _headerPanel.AutoSize = true;
            _headerPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.HandleCreated += (sender, e) =>
            {
                var parentForm = this.FindForm();
                if (parentForm != null)
                {
                    parentForm.Controls.Add(_headerPanel);

                    _headerLabel = new MaterialLabel()
                    {
                        Margin = new Padding(10, 0, 0, 0),
                        Text = _heading,
                         Anchor = AnchorStyles.None,
                        AutoSize =true,
                        FontType = MaterialSkinManager.fontType.H5
                    };
                    _headerPanel.Controls.Add(_headerLabel);

                    _searchTextBox = new MaterialTextBox2()
                    {
                        Margin = new Padding(10, 0, 0, 0),
                        Hint="Търсене..."
                    };
                    _searchTextBox.TextChanged += ColumnSearchTextBox_TextChanged;
                    _headerPanel.Controls.Add(_searchTextBox);
                }
            };
        }

        private void InitializeFooter()
        {
            _footerPanel = new FlowLayoutPanel();
            _footerPanel.Dock = DockStyle.Bottom;
            _footerPanel.Height = 50;
            _footerPanel.AutoSize = true;
            _footerPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.HandleCreated += (sender, e) =>
            {
                var parentForm = this.FindForm();
                if (parentForm != null)
                {
                    parentForm.Controls.Add(_footerPanel); // Добавяме го към формата

                    _itemCountLabel = new MaterialLabel()
                    {
                        Text = $"Заредени: {Items.Count} бр.",
                        Anchor = AnchorStyles.None,
                        AutoSize =true,
                        Margin = new Padding(10, 0, 0, 0),
                        FontType = MaterialSkinManager.fontType.H6
                    };
                    _footerPanel.Controls.Add(_itemCountLabel);
                }
            };
        }
        private void ItemsCounterText()
        {
            if (_itemCountLabel!=null)
                _itemCountLabel.Text=$"Заредени: {Items.Count} бр.";
        }
        private void ColumnSearchTextBox_TextChanged(object sender, EventArgs e)
        {
            this.Items.Clear();
            if (!string.IsNullOrEmpty(_searchTextBox.Text))
            {
                foreach (ListViewItem item in _items)
                {
                    bool found = false;
                    foreach (ColumnHeader column in Columns)
                    {
                        if (column.Index >= 0 && column.Index < item.SubItems.Count &&
                            item.SubItems[column.Index].Text.ToLower().Contains(_searchTextBox.Text.ToLower()))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        Items.Add(item);
                    }
                }
            }
            else
            {
                Items.AddRange(_items);
            }

        }

      

    }
}
