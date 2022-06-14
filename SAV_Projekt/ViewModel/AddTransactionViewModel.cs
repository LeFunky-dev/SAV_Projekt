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
        
        /// <summary>
        /// Method to handle the event, when a transaction changed
        /// </summary>
        private void SelectionChanged()
        {
            Transactions.Add(null);
            Transactions.Remove(null);
        }
        /// <summary>
        /// Method to handle the AddTransactionCommand
        /// </summary>
        /// <param name="obj">Transaction to add</param>
        private void AddTransaction(EtfValue obj)
        {
            Transactions.Add(new EtfValue()
            {
                Date = MinDate,
                Value = 10000,
            });
            RaisePropertyChanged("Transactions");
        }
        /// <summary>
        /// Method to handle the DeleteTransactionCommand
        /// </summary>
        /// <param name="obj">Transaction to delete</param>
        private void DeleteTransaction(EtfValue obj)
        {
            Transactions.Remove(obj);
        }
        /// <summary>
        /// Constructor of the AddTransactionViewModel
        /// </summary>
        public AddTransactionViewModel()
        {
            //Register to incoming messages
            Messenger.Default.Register<NotificationMessage<ObservableCollection<EtfValue>>>(this, (c) => TransactionReceived(c.Notification, c.Content));
            Messenger.Default.Register<NotificationMessage<ObservableCollection<DateTime>>>(this, (c) => DateTimeReceived(c.Notification, c.Content));
        }
        /// <summary>
        /// Method to handle the received date time from MainViewModel
        /// </summary>
        /// <param name="notification">Notification message</param>
        /// <param name="content">Collection of datetime</param>
        private void DateTimeReceived(string notification, ObservableCollection<DateTime> content)
        {
            //First element is MinDate
            MinDate = content.First();
            //Last element is MaxDate
            MaxDate = content.Last();
        }
        /// <summary>
        /// Method to handle a received collection of transaction
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="content"></param>
        private void TransactionReceived(string notification, ObservableCollection<EtfValue> content)
        {
            Transactions = content;
            RaisePropertyChanged("Transactions");
        }
    }
}
