using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using SAV_Projekt.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SAV_Projekt.ViewModel
{
    public class AddTransactionViewModel : ViewModelBase
    {
        public ObservableCollection<EtfValue> Transactions { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public ICommand DeleteTransactionCommand {  get { return new RelayCommand<EtfValue>(DeleteTransaction); } }
        public ICommand AddTransactionCommand {  get { return new RelayCommand<EtfValue>(AddTransaction); } }
        public ICommand SelectionChangedCommand { get { return new RelayCommand(SelectionChanged); } }

        private void SelectionChanged()
        {
            Transactions.Add(null);
            Transactions.Remove(null);
        }

        private void AddTransaction(EtfValue obj)
        {
            Transactions.Add(new EtfValue()
            {
                Date = MinDate,
                Value = 10000,
            });
            RaisePropertyChanged("Transactions");
        }

        private void DeleteTransaction(EtfValue obj)
        {
            Transactions.Remove(obj);
        }

        public AddTransactionViewModel()
        {
            Messenger.Default.Register<NotificationMessage<ObservableCollection<EtfValue>>>(this, (c) => TransactionReceived(c.Notification, c.Content));
            Messenger.Default.Register<NotificationMessage<ObservableCollection<DateTime>>>(this, (c) => DateTimeReceived(c.Notification, c.Content));
        }

        private void DateTimeReceived(string notification, ObservableCollection<DateTime> content)
        {
            MinDate = content.First();
            MaxDate = content.Last();
        }

        private void TransactionReceived(string notification, ObservableCollection<EtfValue> content)
        {
            Transactions = content;
            
            RaisePropertyChanged("Transactions");
        }
    }
}
