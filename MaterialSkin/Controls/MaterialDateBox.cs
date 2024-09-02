using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                _Date = value.ToLocalTime();
                objDateControl.Date = _Date;
                Text = _Date.ToShortDateString();
            }
        }
        public MaterialDateBox()
        {
            ShowAssistiveText = true;   
            _Date = DateTime.UtcNow;
            objDateControl = new MaterialDatePicker(Date);
            objDateControl.onDateChanged += objDateControl_onDateChanged;

            TrailingIcon =Properties.Resources.calendar_24;
             TrailingIconClick+= (sender, e) => {
                 TryParceDate();
                objDateControl.StartPosition = FormStartPosition.CenterParent;
                objDateControl.ShowDialog();
            };

            LostFocus+=(sender, e) =>
            {
                TryParceDate();
            };
        }

        private void TryParceDate()
        {
            if (!string.IsNullOrWhiteSpace(Text))
            {
                if (DateTime.TryParse(Text, out DateTime dateTime))
                {
                    if (Date != dateTime)
                    {
                        Date = dateTime;
                        objDateControl.Date = Date;
                    }
                }
                else
                {
                    ErrorMessage =  "Невалиден формат за дата.";
                    SetErrorState(true);
                }
            }
            else
            {
                // Обработка на празен текст, ако е необходимо
                ErrorMessage     = "Полето за дата е празно.";
                SetErrorState(true);
            }
        }

        private void objDateControl_onDateChanged(DateTime newDateTime)
        {
            _Date = newDateTime;
            Text = newDateTime.ToShortDateString();
            this.objDateControl.Close();
        }
    }
}
