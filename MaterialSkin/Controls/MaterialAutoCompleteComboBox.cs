using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MaterialSkin.Controls
{
    public partial class MaterialAutoCompleteComboBox : MaterialTextBox2
    {
        private MaterialListBox _materialListView;
        private MaterialListBoxItem[]_items;
        public event SelectedItemChangedEventHandler SelectedItemChanged;
        public int AutoCompleteBoxHeight { get; set; } = 240;
        public bool ShowAutoCompleteBox { get; set; } = true;
        public MaterialListBoxItem SelectedItem {  get; set; }   
        public delegate void SelectedItemChangedEventHandler(object sender, MaterialListBoxItem selectedItem);
        public MaterialAutoCompleteComboBox()
        {
            this.KeyUp += MaterialAutoCompleteComboBox_KeyUp;
            this.LostFocus += MaterialAutoCompleteComboBox_LostFocus;

        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (!ShowAutoCompleteBox)
                return;
            InitializeMaterialListBox();

          
        }
        private int _currentSelectedIndex = -1;
        private CancellationTokenSource _cancellationTokenSource;

        private async void MaterialAutoCompleteComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!ShowAutoCompleteBox)
                return;
           
            // Обработка на клавишите нагоре и надолу
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                
                if (!_materialListView.Visible) _materialListView.Visible=true;
                if ( _materialListView.Items.Count > 0)
                {
                    if (e.KeyCode == Keys.Up)
                    {
                        _currentSelectedIndex = Math.Max(_currentSelectedIndex - 1, 0);
                    }
                    else if (e.KeyCode == Keys.Down)
                    {
                        _currentSelectedIndex = Math.Min(_currentSelectedIndex + 1, _materialListView.Items.Count - 1);
                    }
                    _materialListView.HoveredItem = _currentSelectedIndex;

                    e.Handled = true;

                }
                return;
            }

            // Обработка на клавиша Enter
            if (e.KeyCode == Keys.Enter)
            {
                if (_materialListView.Visible && _currentSelectedIndex >= 0 && _currentSelectedIndex < _materialListView.Items.Count)
                {
                    var selectedItem = _materialListView.Items[_currentSelectedIndex];
                    if (selectedItem != null)
                    {
                        Text = selectedItem.Text;
                        _materialListView.Visible = false;
                        _materialListView.Items.Clear();
                        _currentSelectedIndex = -1;
                        SelectedItem = selectedItem;
                        SelectedItemChanged.Invoke(sender, SelectedItem);
                    }
                }
                return;
            }

            if (string.IsNullOrEmpty(Text))
            {
                _materialListView.Visible = false;
                _materialListView.Items.Clear();
                return;
            }

            // Отмяна на предишния токен, ако има такъв
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            try
            {
                // Дебоунсинг
                await Task.Delay(300, token);

                string searchText = Text.Trim();
                if (string.IsNullOrEmpty(searchText))
                {
                    _materialListView.Visible = false;
                    _materialListView.Items.Clear();
                    return;
                }

                string[] searchWords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Асинхронно търсене с подреждане по релевантност и позиция
                var foundItems = await Task.Run(() =>
                {
                    return _items
                        .Select(x => new
                        {
                            Item = x,
                            // Изчисляване на релевантността
                            Relevance = searchWords.Sum(word => x.Text.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : 0),
                            // Изчисляване на минималната позиция на съвпадение
                            Position = searchWords
                                .Select(word => x.Text.IndexOf(word, StringComparison.OrdinalIgnoreCase))
                                .Where(index => index >= 0)
                                .DefaultIfEmpty(int.MaxValue)
                                .Min()
                        })
                        .Where(x => x.Relevance > 0)
                        .OrderBy(x => x.Position) // Първо по позиция на първото съвпадение
                        .ThenByDescending(x => x.Relevance) // После по релевантност
                        .Select(x => x.Item)
                        .ToArray();
                }, token);

                if (token.IsCancellationRequested)
                    return;

                // Актуализиране на потребителския интерфейс
                _materialListView.Invoke(new Action(() =>
                {
                    _materialListView.Visible = true;
                    _materialListView.Items.Clear();
                    if (foundItems.Length > 0)
                        _materialListView.AddItems(foundItems);
                    else
                        _materialListView.AddItem(new MaterialListBoxItem { Text = "Няма открити съвпадения" });
                    _materialListView.BringToFront();
                }));
            }
            catch (TaskCanceledException)
            {
                // Заявката е била отменена
            }

            base.OnKeyUp(e);

        }
       
        private void MaterialAutoCompleteComboBox_LostFocus(object sender, EventArgs e)
        {
            if (!ShowAutoCompleteBox)
                return;
            if (!_materialListView.Focused)
            {
                _materialListView.Visible = false;
            }
        }

        public void AddAutoCompleteItems(MaterialListBoxItem[] items) { _items = items.OrderBy(_ => _.Text).ToArray(); }



        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (!ShowAutoCompleteBox)
                return;

            if (_materialListView != null)
            {
                _materialListView.Width = this.Width;
                UpdateMaterialListBoxPosition(); // Обновява позицията при промяна на размера
            }
        }
        private void UpdateMaterialListBoxPosition()
        {
            if (_materialListView != null && Parent != null)
            {
                _materialListView.Location = new System.Drawing.Point(Location.X, Location.Y + Height + 1);
            }
        }


        private void InitializeMaterialListBox()
        {
           
          
                _materialListView = new MaterialListBox
                {
                    Location = new System.Drawing.Point(this.Location.X, this.Location.Y + this.Height + 1), // Позиция под текста
                    Width = this.Width,
                    Height = AutoCompleteBoxHeight, // Можеш да настроиш височината според нуждите
                    Visible = false,
                };
                _materialListView.SelectedIndexChanged+=_materialListView_SelectedIndexChanged;

                if (Parent != null&&!DesignMode)
                    Parent.Controls.Add(_materialListView);

            UpdateMaterialListBoxPosition(); // Обновява позицията при инициализация

        }

        private void _materialListView_SelectedIndexChanged(object sender, MaterialListBoxItem selectedItem)
        {
            
            _materialListView.Visible=false;
            SelectedItem=selectedItem;
            this.Text = selectedItem.Text;
            SelectedItemChanged?.Invoke(sender, selectedItem);

        }
    }
}
