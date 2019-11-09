using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LungMorphApp
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
        App() { System.Threading.Thread.Sleep(1000); } // Pause to show the splash screen; delete to load immediately        

        public async void StartApp(object sender, StartupEventArgs e)
		{
            string ExpirationDate="20-06";            
            string UserName="Researcher";
            bool EnableBatch=UtilityIO.ValidateInternetTimeNtp(ExpirationDate)
                ||UtilityIO.ValidateInternetTimeHttp(ExpirationDate) ; //|| UtilityIO.ValidateSystemTime(ExpirationDate)
            
            UserWindow user = new UserWindow(UserName,ExpirationDate,EnableBatch); user.Show();
			
		}


	}


}
