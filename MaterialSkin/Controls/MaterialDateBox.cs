using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaterialSkin.Controls
{
    public partial class MaterialDateBox : MaterialMaskedTextBox
    {
        private MaterialDatePicker objDateControl;
        private DateTime _Date;
        public DateTime Date
        {
            get { return _Date; }
            set
            {
                _Date = value; objDateControl.Date = _Date;
                Text = _Date.ToShortDateString();
            }
        }
        public MaterialDateBox()
        {
            objDateControl = new MaterialDatePicker();
            TrailingIcon =Properties.Resources.calendar_24;
             TrailingIconClick+= (sender, e) => {
                objDateControl.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                objDateControl.ShowDialog();
            };
            Date = DateTime.Now;
            objDateControl.onDateChanged += objDateControl_onDateChanged;
        }

        private void objDateControl_onDateChanged(DateTime newDateTime)
        {
            _Date = newDateTime;
            Text = newDateTime.ToShortDateString();
            this.objDateControl.Close();
        }
    }
}
