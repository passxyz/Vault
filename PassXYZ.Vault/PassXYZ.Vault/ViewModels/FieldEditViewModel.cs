using System;
using System.Collections.Generic;
using System.Text;

using PassXYZLib;

namespace PassXYZ.Vault.ViewModels
{
    public class FieldEditViewModel : BaseViewModel
    {
        private Field _field = ItemDetailViewModel.SelectedField;
        private string _key = string.Empty;
        private string _value = string.Empty;

        public string Key
        {
            get
            {
                return string.IsNullOrEmpty(_key) ? _field.Key : _key;
            }
            set
            {
                _ = SetProperty(ref _key, value);
                _field.Key = _key;
            }
        }

        public string Value
        {
            get
            {
                return string.IsNullOrEmpty(_key) ? _field.Value : _value;
            }
            set
            {
                _ = SetProperty(ref _value, value);
                _field.Value = _value;
            }
        }

        public FieldEditViewModel()
        {
            // If the SelectedField is null, this is a case when we want to create a new field.
            if (_field == null)
            {
                ItemDetailViewModel.SelectedField = new Field("", "", false);
            }
        }
    }
}
