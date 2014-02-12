using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;
namespace highprefnet1._0
{
    
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        List<Packet> packets;
        
        int time_count;
        int leak_rate; //row
        int average_input_rate; //lambda_in
        int average_output_rate;//lambda_out
        int timeInterval;
        int maxFrmes;

        int bytes_in_the_file;
        

        private void cmdStart_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

       

        Queue<Packet> byteMarkerqueue;

        Double nextPacktOutId;
        Double nextPacketOutByte;
        int nextPacketInputTime;
        List<Double> delayList;
        List<Double> inputDataList;
        List<Double> outputDataList;

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            if ((packets.Count > time_count)||(Bucket.currentTotalByte>0))
            {
                if (packets.Count > time_count)
                {
                    //Packet gets in to the Bucket
                    Packet p = packets.ElementAt(time_count);
                    p.PacketId = time_count;

                    p.Input_time = time_count * timeInterval;
                    Bucket.currentTotalByte = Bucket.currentTotalByte + p.PacketSize;
                    Bucket.bytesIn = Bucket.bytesIn+p.PacketSize;
                    p.Byte_marker = Bucket.bytesIn;
                    byteMarkerqueue.Enqueue(p);
                }

                while (Bucket.bytesOut >= nextPacketOutByte)
                {
                    Packet temp_p = byteMarkerqueue.Dequeue();
                    nextPacktOutId = temp_p.PacketId;
                    nextPacketOutByte = temp_p.Byte_marker;
                    nextPacketInputTime = temp_p.Input_time;
                    
                }
                if (Bucket.currentTotalByte > leak_rate)//enough in the bucket
                {
                    //leak the constant amount(leak rate)
                    Bucket.currentTotalByte = Bucket.currentTotalByte - leak_rate;
                    outputDataList.Add(leak_rate);
                    Bucket.bytesOut =Bucket.bytesOut+ leak_rate;
                    if (maxFrmes <= 1000)
                    {
                        chart2.Series["Series1"].Points.AddXY(time_count, leak_rate);
                    }

                }
                else if (Bucket.currentTotalByte > 0) //something less then the leak rate in the bucket
                {
                    //leak everything
                    Bucket.bytesOut = Bucket.bytesOut + Bucket.currentTotalByte;
                    outputDataList.Add(Bucket.currentTotalByte);
                    if (maxFrmes <= 1000)
                    {
                        chart2.Series["Series1"].Points.AddXY(time_count, Bucket.currentTotalByte);
                    }
                    Bucket.currentTotalByte =0;
                    
                }
                if (Bucket.bytesOut >= nextPacketOutByte)
                {
                    int delay = (time_count * timeInterval) - nextPacketInputTime;
                    if (maxFrmes <= 1000)
                    {
                        chart3.Series["Series1"].Points.AddXY(nextPacktOutId, delay);
                    }
                    delayList.Add(delay);
                    
                }
                
                

                progressBar1.Value = Convert.ToInt32(Bucket.currentTotalByte / (Bucket.bucketSize /100));
                label1.Text = "Current Packet " + time_count+". Bytes In so far: "+Bucket.bytesIn+". Bytes Out so far: "+Bucket.bytesOut;;
                //Form1.ActiveForm.Refresh();
                time_count++;
                
            }
            else
            {
                timer1.Enabled = false;
                getReslut();
            }
        }

        void getReslut()
        {
            richTextBox1.AppendText(Environment.NewLine + Environment.NewLine + "--- Input ---");
            richTextBox1.AppendText(Environment.NewLine + "Input Average Packet Size: "+ inputDataList.Average());
            richTextBox1.AppendText(Environment.NewLine + "Input Peak Packet Size: "+ inputDataList.Max());
            richTextBox1.AppendText(Environment.NewLine + "Input Brust Rate: " + (1-(inputDataList.Average()/inputDataList.Max())).ToString());

            richTextBox1.AppendText(Environment.NewLine + Environment.NewLine + "--- Output ---");
            richTextBox1.AppendText(Environment.NewLine + "Output Average Packet Size: " + outputDataList.Average());
            richTextBox1.AppendText(Environment.NewLine + "Output Peak Packet Size: " + outputDataList.Max());
            richTextBox1.AppendText(Environment.NewLine + "Input Brust Rate: " + (1 - (outputDataList.Average() / outputDataList.Max())).ToString());


            richTextBox1.AppendText(Environment.NewLine + Environment.NewLine + "--- Delay ---");
            richTextBox1.AppendText(Environment.NewLine + "Total Delay: " + delayList.Sum());
            richTextBox1.AppendText(Environment.NewLine + "Average Delay: " + delayList.Average());
            richTextBox1.AppendText(Environment.NewLine + Environment.NewLine );

        }
        private void cmdLoad_Click(object sender, EventArgs e)
        {
            average_input_rate = 0;
            bytes_in_the_file = 0;
           
            try
            {
                String filePath = @"C:\Documents and Settings\Jayati\My Documents\Visual Studio 2008\Projects\highprefnet1.0\highprefnet1.0\t2.txt";
                using (StreamReader sr = new StreamReader(filePath))
                {
                    String line;
                    int lenght = 0;
                    int totalBytes = 0;
                    packets = new List<Packet>();
                    int pIndex = 0;
                    while (((line = sr.ReadLine()) != null) && (pIndex <=maxFrmes))
                    {

                        if (line.Length > 0)
                        {
                            lenght = line.IndexOf("\t");
                            if (lenght > 1)
                            {
                                totalBytes = Int32.Parse(line.Substring(0, lenght));

                            }
                            else
                            {
                                totalBytes = Int32.Parse(line);
                            }
                            packets.Add(new Packet(pIndex,totalBytes));
                            inputDataList.Add(totalBytes);
                        }
                        pIndex++;
                        bytes_in_the_file = bytes_in_the_file + totalBytes;
                        totalBytes = 0;
                    }
                    average_input_rate = Convert.ToInt32(bytes_in_the_file / (pIndex - 1));
                }
                richTextBox1.Text = "Total Bytes in the flie: " + bytes_in_the_file + Environment.NewLine +
                    "Number of packet: " + packets.Count + Environment.NewLine +
                    "Average Input rate: " + average_input_rate + Environment.NewLine +
                    "Leak rate: " + leak_rate+ Environment.NewLine +
                    "Bucket size: "+Bucket.bucketSize+ Environment.NewLine;

               
                if ((null != packets && packets.Count > 0) && (maxFrmes <=1000))
                {
                    for (int i = 0; i < packets.Count; i++)
                    {
                        chart1.Series["Series1"].Points.AddXY(packets.ElementAt(i).PacketId, packets.ElementAt(i).PacketSize);
                    }
                }
                 
   
            }
            catch (Exception e2)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e2.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Bucket.currentTotalByte = 0;
            Bucket.bytesIn = 0;
            Bucket.bytesOut = 0;
            time_count = 0;
            updateParm();
           
           
            byteMarkerqueue = new Queue<Packet>();


            nextPacktOutId = 0;
            nextPacketOutByte = 0;
            nextPacketInputTime = 0;

            delayList = new List<Double>();
            inputDataList = new List<Double>();
            outputDataList = new List<Double>();
            
            timer1.Interval = timeInterval;
            


        }
        void updateParm()
        {
            timeInterval = int.Parse(textTimeInterval.Text);
            timer1.Interval = timeInterval;
            leak_rate = int.Parse(txtLeakrate.Text);
            Bucket.bucketSize = int.Parse(txtBucketSize.Text);
            timeInterval = int.Parse(textTimeInterval.Text);
            leak_rate = int.Parse(txtLeakrate.Text);
            Bucket.bucketSize = int.Parse(txtBucketSize.Text);
            maxFrmes = int.Parse(txtFrames.Text);

            if (maxFrmes <= 1000)
            {
                chart1.ChartAreas[0].AxisX.Minimum = 0;
                chart1.ChartAreas[0].AxisX.Maximum = maxFrmes+100;
                chart1.Series["Series1"].ChartType = SeriesChartType.Line;

                chart2.ChartAreas[0].AxisX.Minimum = 0;
                chart2.ChartAreas[0].AxisX.Maximum = maxFrmes+100;
                chart2.Series["Series1"].ChartType = SeriesChartType.Line;

                chart3.ChartAreas[0].AxisX.Minimum = 0;
                chart3.ChartAreas[0].AxisX.Maximum = maxFrmes+100;
                chart3.Series["Series1"].ChartType = SeriesChartType.Line;
            }
        }
        private void cmdStop_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void cmdExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void cmeUpdate_Click(object sender, EventArgs e)
        {
            updateParm();
        }


    }
}
