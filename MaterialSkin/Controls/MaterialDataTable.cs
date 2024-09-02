using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace MaterialSkin.Controls
{
    public partial class MaterialDataTable : MaterialListView, IMaterialControl
    {
        private ListViewItem[] _items;
        private int _sortColumn = -1;
        private SortOrder _sortOrder = SortOrder.None;
        [Browsable(false)]
        public ListViewItem[] DataItems {
            set {
                _items = value;
                Items.AddRange(_items);
                ItemsCounterText();
                if (!string.IsNullOrEmpty(_searchTextBox?.Text))
                    ColumnSearchTextBox_TextChanged(this, EventArgs.Empty);
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
            this.ColumnClick += new ColumnClickEventHandler(OnColumnClick);
            this.KeyDown += new KeyEventHandler(OnKeyDown);
            this.MultiSelect = true;

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
                var parentControl = this.Parent;
                if (parentControl != null&&!DesignMode )
                {

                    parentControl.Controls.Add(_headerPanel);

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
                var parentControl = this.Parent;
                if (parentControl != null &&!DesignMode)
                {
                    parentControl.Controls.Add(_footerPanel); // Добавяме го към формата

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
            string searchText = _searchTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(searchText))
            {
                var searchWords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var foundItems = _items
                    .Select(item => new
                    {
                        Item = item,
                        // Изчисляване на релевантността
                        Relevance = searchWords.Sum(word => item.SubItems.Cast<ListViewItem.ListViewSubItem>()
                            .Count(subItem => subItem.Text.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)),
                        // Минимална позиция на съвпадение в текста
                        Position = searchWords.Select(word => item.SubItems.Cast<ListViewItem.ListViewSubItem>()
                            .Select(subItem => subItem.Text.IndexOf(word, StringComparison.OrdinalIgnoreCase))
                            .Where(index => index >= 0)
                            .DefaultIfEmpty(int.MaxValue)
                            .Min()).Min()
                    })
                    .Where(x => x.Relevance > 0)
                    .OrderBy(x => x.Position) // По позиция на съвпадение
                    .ThenByDescending(x => x.Relevance) // След това по релевантност
                    .Select(x => x.Item)
                    .ToArray();

                Items.AddRange(foundItems);

            }
            else
            {
                Items.AddRange(_items);
            }

            ItemsCounterText();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                CopySelectedItemsToClipboard();
            }
        }

        private void CopySelectedItemsToClipboard()
        {
            if (SelectedItems.Count > 0)
            {
                var sb = new System.Text.StringBuilder();
                foreach (ListViewItem item in SelectedItems)
                {
                    for (int i = 0; i < item.SubItems.Count; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append("\t");
                        }
                        sb.Append(item.SubItems[i].Text);
                    }
                    sb.AppendLine();
                }
                Clipboard.SetText(sb.ToString());
            }
        }

        private void OnColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == _sortColumn)
            {
                // Ако колоната е същата, сменете реда на сортиране
                _sortOrder = _sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                // Ако колоната е различна, задайте нова колона и ред на сортиране
                _sortColumn = e.Column;
                _sortOrder = SortOrder.Ascending;
            }

            // Задаване на сортиращия обект
            this.ListViewItemSorter = new ListViewItemComparer(e.Column, _sortOrder);
            this.Sort();
        }
    }

    public class ListViewItemComparer : IComparer
    {
        private int _column;
        private SortOrder _order;

        public ListViewItemComparer(int column, SortOrder order)
        {
            _column = column;
            _order = order;
        }

        public int Compare(object x, object y)
        {
            ListViewItem itemX = x as ListViewItem;
            ListViewItem itemY = y as ListViewItem;

            int result = String.Compare(itemX.SubItems[_column].Text, itemY.SubItems[_column].Text);

            if (_order == SortOrder.Descending)
                result = -result;

            return result;
        }
    }
}
