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
        App() { }

		public async void StartApp(object sender, StartupEventArgs e)
		{
            UserWindow user = new UserWindow("Research-Use","20-02",true); user.Show();
		}


	}


}
