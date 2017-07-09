﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Forms;
using System.Net.Http;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Management;
using MaterialSkin.Controls;
using MaterialSkin;

namespace TwitterBot {
    public partial class Form1 : Form {
        static int accountsCount = 0;
        static int totalAccountsCount = 0;
        public Form1() {
            InitializeComponent();
        }

        private void accountCreatorTimer_Tick(Object sender, EventArgs e) {

            // Create random email
            string randomEmail = Randomizer.RandomEmail(8);

            // Create a background worker
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler((send, args) => makeRequest(passwordText.Text, randomEmail, Randomizer.RandomName(), Randomizer.RandomName()));

            // run in another thread
            bw.RunWorkerAsync();
            Debug.WriteLine("Making request");
        }

        private string getRandomProxy() {

            // Select random line from proxy file and return that proxy
            string path = @"C:\twitterbot\proxies.txt";
            var lines = File.ReadAllLines(path);
            if (lines.Length != 0) {
                var r = new Random();
                var randomLineNumber = r.Next(0, lines.Length - 1);
                string line = lines[randomLineNumber];
                return line;
            }
            return "0";
        }
        
        public async void makeRequest(string password, string email, string firstName, string lastName) {

            // Instantiate new cookie aware web client
            var cookieJar = new CookieContainer();
            CookieAwareWebClient client = new CookieAwareWebClient(cookieJar);

            // Grab random proxy from list
            string proxy = getRandomProxy();
            Debug.WriteLine(proxy);

            // Create new WebProxy and send Web client proxy new newly created proxy
            WebProxy wp = new WebProxy(proxy);
            client.Proxy = wp;

            // Make request to signup form to grab authenticity token
            string response = client.DownloadString("https://twitter.com/signup");
            string token = Regex.Match(response, "authenticity_token.+?value=\"(.+?)\"").Groups[1].Value;
            string postData =
                string.Format("authenticity_token={0}&signup_ui_metrics=%7B%22rf%22%3A%7B%22a329c5b5bc6c102ef9c1d63f316f58ac8fb4532446a1e4c83a11d61dac0f0d88%22%3A128%2C%22ee55fdfa42bd4572ff3a3690c7d2e50796a0b4d842b51b2130eb6fa7818ca273%22%3A-1%2C%22a52e852b39ea535cd0dfdc5f0b421766719588a7600ffc992fc224f920e92a81%22%3A158%2C%22fdd9b9308177fabcd24ea3310770605522530c56e0c3884906198e2a81c793f7%22%3A170%7D%2C%22s%22%3A%22oVgQBK7u_ZBbokRa-9tVVDtYQIBFf5ndDt-8Juqa-0i4-X5KwVsPwEUYqDkfB24YH5hsr_tmqPm6769FZvg-eYDljhWCc3tvyTno6Ts_QeAFb8mMS-13wvcxJkrKTUNyb5ac_-kzalx_WKsR0BD-Q4gM9YQ52IYwu4X0Sj2qubjIpjygiZH-vZgMJTkwQZP4jK8q4LoC37liW1gbSM4kv9NNK5g-c-2VR79yDC92moRaEqS9XOpzP1zYEfDdGLZDLCNW0C8E8qaz_b-l1M5mV04ih7WHbRE5QjV1bZohkiFF5Xa2nteIoSCHnZjD41JA0M9wntp66gLjuZUrBJLX0QAAAV0WYV14%22%7D&m_metrics=thMqEh0A8SkAthM9EhMA9ykAthNZEhwA%2FSkAthNsEhMAByoAthOIEhwAGioAthObEhMALCoAwRObEgsAMCoAwRO4Eh0APCoAwRPLEhMARioAwRPnEhwATioAwRP6EhMAVioAwRMWExwAXCoAthMWEwsAXioAthMpExMAYioAthNGEx0AbioAwRNGEwsAmioAwRNZExMAsioA0BNZEw8AwCoA0BN1ExwAwSoA0BOIExMAzyoA%3AuQQJALkEtg0AXBQPDB4AUicA&d_metrics=&user%5Bname%5D=" + firstName + "+" + lastName + "&user%5Bemail%5D=" + email + "&user%5Buser_password%5D=" + password + "&user%5Buse_cookie_personalization%5D=1&asked_cookie_personalization_setting=1&ad_ref=&user%5Bdiscoverable_by_email%5D=1&asked_discoverable_by_email=1&user%5Bdiscoverable_by_mobile_phone%5D=1&asked_discoverable_by_mobile_phone=1", token);
            client.Method = "POST";
            
            // Set URI for POST
            Uri uri = new Uri("https://twitter.com/account/create");

            // Make post request passing in our postdata containing our referer string
            string res = await client.UploadStringTaskAsync(uri, postData);
            
            // Save account to file and increase counter
            saveAccountToFile(email);
        }
  
        private void saveAccountToFile(string account) {

            // Increase global variable
            accountsCount++;
            totalAccountsCount++;

            this.Invoke(new MethodInvoker(delegate {

                // Set our labels equal to the variables
                accountsCreated.Text = accountsCount.ToString();
                totalAccounts.Text = totalAccountsCount.ToString();

                // Add account to listbox
                accountsList.Items.Add(account);
            }));


            // Write account to file
            string path = @"C:\twitterbot\accounts.txt";
            using (StreamWriter sw = File.AppendText(path)) {              
                sw.WriteLine(account);             
            }
        }
        private void Form1_Load(Object sender, EventArgs e) {

            // On load grab number of lines in account file and set our total account label equal to that
            var lineCount = File.ReadLines(@"C:\twitterbot\accounts.txt").Count();
            totalAccounts.Text = lineCount.ToString();
            totalAccountsCount = lineCount;
        }

        private void button1_Click(Object sender, EventArgs e) {
            accountCreatorTimer.Start();
        }

        private void button2_Click(Object sender, EventArgs e) {
            accountCreatorTimer.Stop();
        }

        private void passwordText_TextChanged(Object sender, EventArgs e) {
            if (this.Text.Length > 0) {
                startBot.Enabled = true;
            }
        }

        private void button1_Click_1(Object sender, EventArgs e) {
            string HWID = string.Empty;//creating a empty string
            ManagementClass Management = new ManagementClass("win32_processor");//declaring the system management calss
            ManagementObjectCollection MObject = Management.GetInstances();//decalring the system management object collection 
            foreach (ManagementObject mob in MObject)//having a foreach loop
            {
                if (string.IsNullOrEmpty(HWID)) {
                    HWID = mob.GetPropertyValue("processorID").ToString();//converting the HWID to string
                    break;
                }
            }
            MessageBox.Show("Your HWID : " + HWID, " HWID", MessageBoxButtons.OK, MessageBoxIcon.Information);//displaying the HWID from Message Box

        }
    }
}
