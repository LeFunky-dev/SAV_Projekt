using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SAV_Projekt.View
{
    /// <summary>
    /// Interaction logic for AddEditPortfolioWindow.xaml
    /// </summary>
    public partial class AddEditPortfolioWindow : Window
    {
        public AddEditPortfolioWindow()
        {
            InitializeComponent();
        }
        protected override void OnClosed(EventArgs e)
        {
            Messenger.Default.Send(OperatingCommandsEnum.ClosePortfolioAddEdit);
            this.Close();
        }
    }
}
