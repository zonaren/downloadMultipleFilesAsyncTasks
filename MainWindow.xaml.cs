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

// Add a using directive and a reference for System.Net.Http.
using System.Net.Http;

// Add the following using directive.
using System.Threading;
using System.Net;
using System.IO;
using System.Collections.ObjectModel;

namespace ProcessTasksAsTheyFinish
{
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            SetUpURLList();
            try
            {
                ListViewCsv.ItemsSource = SetUpURLList();

                //csvCount.Text = SetUpURLList().Count.ToString();
                taskCount.Text = SetUpURLList().Count.ToString();


            }
            catch
            {
                ListViewCsv.ItemsSource = null;
            }
        }

        private async void startButton_Click(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                resultsTextBox.Clear();
                await AccessTheWebAsync();
            }




            
        }


        async Task AccessTheWebAsync()
        {
            HttpClient client = new HttpClient();

            // Make a list of web addresses.
            List<CsvList> urlList = SetUpURLList();

            // ***Create a query that, when executed, returns a collection of tasks.
            IEnumerable<Task<string>> downloadTasksQuery =
                from url in urlList select ProcessURL(url.Id, url.StasjonNr,url.Navn, url.IpAdresse, client);

            // ***Use ToList to execute the query and start the tasks. 
            List<Task<string>> downloadTasks = downloadTasksQuery.ToList();

            //Create an observable collection of downloadTask to update the Listview dynamically
            ObservableCollection<Task<string>> taskList = new ObservableCollection<Task<string>>(downloadTasks);

            RunningTasks.ItemsSource = taskList;

            // ***Add a loop to process the tasks one at a time until none remain.
            while (taskList.Count > 0)
            {
                //Get all task that are running

                
                // Identify the first task that completes.
                Task<string> firstFinishedTask = await Task.WhenAny(taskList);
                
                    
                
                // ***Remove the selected task from the list so that you don't
                // process it more than once.
                taskList.Remove(firstFinishedTask);

                    // Await the completed task.
                    string returnedValue = await firstFinishedTask;
                    taskCount.Text = taskList.Count.ToString();
                    resultsTextBox.Text += "\r\nTask: " + firstFinishedTask.Id + " TaskStatus: " + firstFinishedTask.Status + " - " + returnedValue + "\r\n";
            }
        }


        private List<CsvList> SetUpURLList()
        {
            var urls = File.ReadAllLines("D:\\DownloadTest\\test.csv")
                .Skip(1)
                .Select(v => v.Split(','))
                .Select(v => new CsvList()
                {
                    Id = v[0].Replace("\"", ""),
                    StasjonNr = v[1].Replace("\"", ""),
                    Navn = v[2].Replace("\"", ""),
                    IpAdresse = v[3].Replace("\"", "")
                    //Progress = AccessTheWebAsync().Status.ToString()
                }).ToList();

            return urls;
        }


        async Task<string> ProcessURL(string id,string nr,string navn,string url, HttpClient client)
        {
            await Task.Delay(5000);

            string filename = id+"_"+nr+"_"+navn;
            string path = @"D:\DownloadTest\" + filename + ".zip";
            // GetAsync returns a Task<HttpResponseMessage>. 
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Read response asynchronously and save asynchronously to file
                using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    //copy the content from response to filestream
                    await response.Content.CopyToAsync(fileStream);


                }
                
               
            }
            return filename + " - Statuscode: " + response.StatusCode;



        }

    }
}
