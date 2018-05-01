using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.Res;
using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using static cec.DaoGpu;
using System.Json;

namespace cec
{
    [Activity(Label = "cec", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private String GpuText;
        private String AlgoText;
        private ArrayAdapter<String> DataAdapter;
        private ArrayAdapter<String> DataAdapter2;
        private List<string> listA = new List<string>();
        private List<string> listG = new List<string>();
        private List<string> listS = new List<string>();
        private DaoGpu dg = new DaoGpu();
        private XmlDocument document = new XmlDocument();
        private XmlNode topNode;
        private GpuCollect gc = new GpuCollect();
        private string hash;
        private int i = 0;
        JsonValue jsonDoc;

        private void StartXml()
        {
            AssetManager assets = this.Assets;
            document.Load(assets.Open("gpu.xml"));
            topNode = document.GetElementsByTagName("root")[0];
            

            foreach (XmlNode node in topNode.ChildNodes)
            {
                dg = new DaoGpu(node.ChildNodes[0].InnerText,
                   node.ChildNodes[1].InnerText, node.ChildNodes[2].InnerText,
                   node.ChildNodes[3].InnerText);

                gc.Add(dg);
                listG.Add(node.ChildNodes[0].InnerText);
            }

            listA.Add("Ethereum");
            listA.Add("Ethereum Classic");
            listA.Add("Equihash");

            this.DataAdapter = new ArrayAdapter<string>(this,
                        Android.Resource.Layout.SimpleSpinnerItem, listG.Distinct().ToList());
            this.DataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

            this.DataAdapter2 = new ArrayAdapter<string>(this,
                        Android.Resource.Layout.SimpleSpinnerItem, listA.Distinct().ToList());
            this.DataAdapter2.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
           
           

        }

        private void Spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            
            if (spinner.GetItemAtPosition(e.Position).Equals("minute")) i = 0;
            else if (spinner.GetItemAtPosition(e.Position).Equals("hour")) i = 1;
            else if (spinner.GetItemAtPosition(e.Position).Equals("day")) i = 2;
            else if (spinner.GetItemAtPosition(e.Position).Equals("week")) i = 3;
            else i = 4;

            if(jsonDoc != null)
            {
                switch (i)
                {
                    case 0:
                        ParseAndDisplay(jsonDoc,i);
                        break;
                    case 1:
                        ParseAndDisplay(jsonDoc,i);
                        break;
                    case 2:
                        ParseAndDisplay(jsonDoc,i);
                        break;
                    case 3:
                        ParseAndDisplay(jsonDoc,i);
                        break;
                    case 4:
                        ParseAndDisplay(jsonDoc,i);
                        break;
                    default:
                        Toast.MakeText(ApplicationContext, "Something worng", ToastLength.Long).Show();
                        break;
                }         
            }
            else
            {
                string message = "No DATA";
                Toast.MakeText(ApplicationContext, message, ToastLength.Long).Show();
            }

          
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            StartXml();
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            Spinner spinner = FindViewById<Spinner>(Resource.Id.spinner1);
            spinner.Adapter = this.DataAdapter;

            Spinner spinner2 = FindViewById<Spinner>(Resource.Id.spinner2);
            spinner2.Adapter = this.DataAdapter2;

       
            Button button = FindViewById<Button>(Resource.Id.Check);
            

            button.Click += async (o, e) =>
            {
                GpuText = spinner.SelectedItem.ToString();
                AlgoText = spinner2.SelectedItem.ToString();
                string _AlgoText;

                if (AlgoText == "Ethereum" || AlgoText == "Ethereum Classic")
                {
                    _AlgoText = "etHash";
                    await CalculateAsync(GpuText, _AlgoText);
                }
                else
                {
                    _AlgoText = "equiHash";
                    await CalculateAsync(GpuText, _AlgoText);
                }

                Spinner spinner3 = FindViewById<Spinner>(Resource.Id.spinner3);
                spinner3.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(Spinner_ItemSelected);
                var adapter3 = ArrayAdapter.CreateFromResource(
                        this, Resource.Array.resultArray, Android.Resource.Layout.SimpleSpinnerItem);

                adapter3.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                spinner3.Adapter = adapter3;

            };
        }

        private async Task CalculateAsync(string gpu, string algo)
        {
            
            for (int i =0; i<gc.Count; i++)
            {
                if (gc.Search(i).VGA == gpu && gc.Search(i).Algo == algo)
                {
                    hash = gc.Search(i).Hash;
                }                       
            }

            Double newHash = Double.Parse(hash);
            int newhash1 = (int)(Math.Round(newHash));

            if (newhash1 == 0)
            {
                string message = "no information";
                Toast.MakeText(ApplicationContext, message, ToastLength.Long).Show();
            }
            else if (algo.Equals("etHash") && hash!=null)
            {
                string url = "https://api.nanopool.org/v1/eth/approximated_earnings/";
                JsonValue json = await FetchData(url+""+ newHash);
                ParseAndDisplay(json,i);
            }
            else if(algo.Equals("equiHash") && hash != null)
            {
                string url = "https://api.nanopool.org/v1/etc/approximated_earnings/";
                JsonValue json = await FetchData(url+""+ newHash);
                ParseAndDisplay(json,i);
            }
            
            else
            {
                string message = "no information";
                Toast.MakeText(ApplicationContext, message, ToastLength.Long).Show();
            }
        }

        private async Task<JsonValue> FetchData(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
            request.ContentType = "application/json";
            request.Method = "GET";
            using (WebResponse response = await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    jsonDoc = await Task.Run(() => JsonObject.Load(stream));
                    return jsonDoc;
                }
            }
        }



        private void ParseAndDisplay(JsonValue json,int i)
        {      
            TextView usd = FindViewById<TextView>(Resource.Id.usdText);
            TextView btc = FindViewById<TextView>(Resource.Id.btcText);

            JsonValue Result = json["data"];

            if (i == 0)
            {
                string status = "minute";
                JsonValue s = Result[status];
                usd.Text = s["dollars"].ToString();
                btc.Text = s["bitcoins"].ToString();
            }

            else if (i == 1)
            {
                string status = "hour";
                JsonValue s = Result[status];
                usd.Text = s["dollars"].ToString();
                btc.Text = s["bitcoins"].ToString();
            }
            else if (i == 2)
            {
                string status = "day";
                JsonValue s = Result[status];
                usd.Text = s["dollars"].ToString();
                btc.Text = s["bitcoins"].ToString();
            }
            else if (i == 3)
            {
                string status = "week";
                JsonValue s = Result[status];
                usd.Text = s["dollars"].ToString();
                btc.Text = s["bitcoins"].ToString();
            }

            else if (i == 4)
            {
                string status = "month";
                JsonValue s = Result[status];
                usd.Text = s["dollars"].ToString();
                btc.Text = s["bitcoins"].ToString();
            }



        }
    }
}

