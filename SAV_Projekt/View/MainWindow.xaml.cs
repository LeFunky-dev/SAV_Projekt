using GalaSoft.MvvmLight.Messaging;
using SAV_Projekt.View;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SAV_Projekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// moffa
    /// </summary>
    public partial class MainWindow : Window
    {
        public Dictionary<string, Window> WindowDictionary { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            WindowDictionary = new Dictionary<string, Window>();

            Messenger.Default.Register<OperatingCommandsEnum>(this, (command) =>
            {
                switch (command)
                {
                    case OperatingCommandsEnum.OpenEtfDetail:
                        {
                            if (!WindowDictionary.ContainsKey("OpenEtfDetailWindow"))
                            {
                                //open Window and store in Dictionary
                                WindowDictionary.Add("OpenEtfDetailWindow", new EtfDetailWindow());
                                WindowDictionary["OpenEtfDetailWindow"].Show();
                            }
                            break;
                        }
                    case OperatingCommandsEnum.OpenPortfolioAddEdit:
                        {
                            if (!WindowDictionary.ContainsKey("OpenPortfolioAddEdit"))
                            {
                                //open Window and store in Dictionary
                                WindowDictionary.Add("OpenPortfolioAddEdit", new AddEditPortfolioWindow());
                                WindowDictionary["OpenPortfolioAddEdit"].Show();
                            }
                            break;
                        }
                    case OperatingCommandsEnum.CloseEtfDetail:
                        {
                            WindowDictionary["OpenEtfDetailWindow"].Close();
                            WindowDictionary.Remove("OpenEtfDetailWindow");
                            break;
                        }
                    case OperatingCommandsEnum.ClosePortfolioAddEdit:
                        {
                            WindowDictionary["OpenPortfolioAddEdit"].Close();
                            WindowDictionary.Remove("OpenPortfolioAddEdit");
                            break;
                        }
                    default: break;
                };
            });
        }

    }
}
