using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Windows.Forms.DataVisualization.Charting;
using System.Globalization;
using System.IO;

namespace MultiPingWindowsForms {
  public partial class Form1 : Form {

    class Reading {
      private DateTime time;
      private double val;
      public DateTime Time {
        get {
          return time;
        }
      }
      public double Val {
        get {
          return val;
        }
        set {
          time = DateTime.Now;
          val = value;
        }
      }
      public
      Reading(double d) {
        time = DateTime.Now;
        val = d;
      }

    }

    UdpClient Client;
    public bool continous;
    Dictionary<string, Reading> series = new Dictionary<string, Reading>();


    public Form1() {
      System.Threading.Thread.CurrentThread.CurrentCulture =
        System.Globalization.CultureInfo.InvariantCulture;
      InitializeComponent();

      //chart1.Legends[0].

      button1_Click(null, null);

      Timer timer = new Timer();
      timer.Interval = 1000;
      timer.Tick += Timer_Tick;
      timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e) {
      List<string> list = new List<string>();
      foreach (var reading in series) {
        if (reading.Value.Time < DateTime.Now.AddMinutes(-5))
          list.Add(reading.Key);
      }
      foreach (var r in list)
        series.Remove(r);
      this.Invalidate();
    }

    public void DrawString(BufferedGraphics myBuffer, float x, float y, float y2, double d, string drawString) {
      System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(0x44, 0x44, 0x88));
      myBuffer.Graphics.FillRectangle(myBrush,
        new Rectangle(10, (int)y,
        (int)(d / series.Select(linq=>linq.Value.Val).Max() * this.Width),
        (int)y2));

      System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 16);
      System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
      System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat();
      //myBuffer.Graphics.DrawString(d + " " + drawString, drawFont, drawBrush, x, y, drawFormat);
      myBuffer.Graphics.DrawString(drawString+": "+ d, drawFont, drawBrush, x, y, drawFormat);

      drawFont.Dispose();
      drawBrush.Dispose();


      //this.Invalidate();

      myBrush.Dispose();

      //Form1_Paint(null, null);

    }

    protected override void OnPaint(PaintEventArgs e) {
      // If there is an image and it has a location, 
      // paint it when the Form is repainted.
      base.OnPaint(e);

      BufferedGraphicsContext currentContext;
      currentContext = BufferedGraphicsManager.Current;
      BufferedGraphics myBuffer;
      myBuffer = currentContext.Allocate(this.CreateGraphics(),
        this.DisplayRectangle);


      for (int i = 0; i < series.Count(); i++) {
        var y = this.Height / (series.Count) * i+5;
        var h = this.Height / (series.Count) - 10;
        DrawString(myBuffer, 10, y, h, series.ElementAt(i).Value.Val, series.ElementAt(i).Key);
      }

      myBuffer.Render();
      // Renders the contents of the buffer to the specified drawing surface.
      myBuffer.Render(this.CreateGraphics());
      myBuffer.Dispose();

    }

    private void Form1_Paint(System.Object sender,
    System.Windows.Forms.PaintEventArgs e) {
      e.Graphics.FillEllipse(Brushes.DarkBlue, new
          Rectangle(10, 10, 60, 60));
      e.Graphics.FillRectangle(Brushes.Khaki, new
          Rectangle(20, 30, 60, 10));
      e.Graphics.CopyFromScreen(new Point(10, 10), new Point(100, 100),
          new Size(70, 70));
    }

    private void button1_Click(object sender, EventArgs e) {
      try {

        WebServer ws = new WebServer(SendResponse, 
          "http://*:8080/");        
        ws.Run();

        //PingButton.IsEnabled = !continuous;
        continous = !continous;
        /*if (continous)
          PingButton.Text = "Stop";
        else
          PingButton.Text = "Listen";*/

        if (!continous) {
          return;
        }

        if (Client == null)
          Client = new UdpClient(4444);

        //Creates an IPEndPoint to record the IP Address and port number of the sender.
        // The IPEndPoint will allow you to read datagrams sent from any source.
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

        Client.BeginReceive(new AsyncCallback(CallBack), null);
      }
      catch (Exception ex) {
        MessageBox.Show(ex.ToString());
      }

    }
  

    private void CallBack(IAsyncResult res) {
      IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 4444);
      byte[] received = Client.EndReceive(res, ref RemoteIpEndPoint);
      string s = Encoding.ASCII.GetString(received);

      Console.WriteLine(RemoteIpEndPoint.Address + " " + s);

      if (!s.Contains("garbage")) {
        string[] lines = s.Split('\n');
        foreach (var line in lines) {
          string[] split = line.Split(':');
          if (split.Length > 1)
             this.BeginInvoke(
              new Action(() => {
                double d = 0;
                d = Double.Parse(split[1]);
                var name = split[0];
                series[name] = new Reading(d);
               //this.Invalidate();
              }));         
        }
      }

      if (continous)
        Client.BeginReceive(new AsyncCallback(CallBack), null);
    }

    public string SendResponse(HttpListenerRequest request) {

      if (request.RawUrl == "/last") {
        DateTimeFormatInfo myDTFI = new CultureInfo("en-us", false).DateTimeFormat;
        string s = "";
        foreach (var ser in series)
          s += ",\"" + ser.Key + "\":" + ser.Value.Val;
        return "{" + s.Substring(1) + "}";
      }
      else
        if (request.Url.AbsolutePath=="/") {
        return File.ReadAllText(@"index.html");
      } else
        if (request.RawUrl == "/graf.js") {
        return File.ReadAllText(@"graf.js");
      }
      return "";

    }

  }
}
