using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace datvreceiver
{
    class socket
    {
        public Action<ushort[]> callback;

        private WebSocket ws;       //websocket client
        public bool connected;
        private ushort[] fft_data;

        public DateTime lastdata;

        public socket()
        {
            connected = false;

        }

        private const int maxRetries = 5; // or any desired value

        public void start()
        {
            if (!connected)
            {
                int retryCount = 0; // Initialize retryCount here

                while (!connected && retryCount < maxRetries)
                {
                    Console.WriteLine(connected);
                    Console.WriteLine("Try connect..\n");

                    try
                    {
                        ws = new WebSocket("wss://eshail.batc.org.uk/wb/fft", "fft_m0dtslivetune");
                        ws.OnMessage += (ss, ee) => NewData(ee.RawData);
                        ws.OnOpen += (ss, ee) => { connected = true; Console.WriteLine("Connected.\n"); };
                        ws.OnClose += (ss, ee) => { connected = false; };
                        ws.Connect();
                        lastdata = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("WebSocket initialization failed: " + ex.Message);
                    }

                    // Increase retry count
                    retryCount++;

                    // Display waiting message with remaining time
                    Console.WriteLine($"Waiting for {10 * retryCount} seconds before the next retry...");

                    // Sleep for 10 seconds before the next retry
                    System.Threading.Thread.Sleep(10000);
                }
            }
        }

        public void stop()
        {
            if (connected)
            {
                ws.Close();
                connected = false;
            }

        }



        private void NewData(byte[] data)
        {
            lastdata = DateTime.Now;
            fft_data = new UInt16[data.Length / 2];

            int n = 0;
            byte[] buf = new byte[2];

            for (int i = 0; i < data.Length; i += 2)
            {
                buf[0] = data[i];
                buf[1] = data[i + 1];
                fft_data[n] = BitConverter.ToUInt16(buf, 0);
                n++;
            }

            // Print WebSocket connection status
            Console.WriteLine("WebSocket Connected: " + connected);

            // Ensure that the callback is not null before invoking it
            callback?.Invoke(fft_data);
        }


    }
}
