using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;
using Microsoft.VisualStudio.PlatformUI;

using Technostar.Models;
using Technostar.External;
using Technostar.DAL.Repositories;

namespace Technostar.ViewModels
{
    public class ViewModel : ViewModelBase
    {
        private readonly RecordRepository _repository = new RecordRepository();

        #region Bindings

        private ObservableCollection<Record> _models;
        public ObservableCollection<Record> Models
        {
            get => _models;
            set => Set(ref _models, value);
        }

        private Record _selectedModel;
        public Record SelectedModel
        {
            get => _selectedModel;
            set => Set(ref _selectedModel, value);
        }

        private string _recordText;
        public string RecordText
        {
            get => _recordText;
            set => Set(ref _recordText, value);
        }

        #endregion

        public ViewModel()
        {
            _models = new ObservableCollection<Record>(_repository.GetAllRecords());
        }

        public void OnSelectionChanged()
        {
            RecordText = _selectedModel is null 
                ? string.Empty
                : _selectedModel.Content;
        }

        #region SendCommand

        public ICommand SendCommand
            => new DelegateCommand(obj => OnSendPressed(),
                obj => !string.IsNullOrEmpty(_recordText) && (_selectedModel is null || _selectedModel.Content != _recordText));
        private void OnSendPressed()
        {
            if (_selectedModel is null)
                AddNewRecord();
            else
                UpdateSelectedRecord();

            RecordText = string.Empty;
        }
        private void AddNewRecord()
        {
            _models.Insert(0, _repository.AddRecord(_recordText));
        }
        private void UpdateSelectedRecord()
        {
            _models.Insert(0, _repository.UpdateRecord(_selectedModel.Id, _recordText));
            _models.Remove(_selectedModel);
        }

        #endregion

        #region ReverseCommand

        public ICommand ReverseCommand
            => new DelegateCommand(obj => ReverseString(),
                obj => !string.IsNullOrEmpty(_recordText));
        private void ReverseString()
        {
            var sb = new StringBuilder();

            if (ExternalFuncs.ReverseString(_recordText, sb))
                RecordText = sb.ToString();
            else
                MessageBox.Show("Reversing can't be done");
        }

        #endregion
    }
}
