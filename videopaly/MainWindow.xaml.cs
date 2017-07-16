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
using System.Windows.Threading;
using Microsoft.Win32;

//新添加命名空间    
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.ComponentModel;

namespace WpfApp1
{
    /// MainWindow.xaml 的交互逻辑
    public partial class MainWindow : Window
    {
        private static byte[] result = new byte[1024];
        private static int myProt = 54322;   //端口  
        static Socket serverSocket;
        public MainWindow()
        {
            InitializeComponent();
            // mePlayer.Source = new Uri("file:///./trailer.mp4", System.UriKind.Relative);
            mePlayer.Source = new Uri("concept.mp4", System.UriKind.Relative);
            mePlayer.MediaEnded += new RoutedEventHandler(Media_Ended);
            mePlayer.Play();

            IPAddress ip = IPAddress.Parse("192.168.1.220");
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, myProt));  //绑定IP地址：端口  
            serverSocket.Listen(10);    //设定最多10个排队连接请求  
            //通过Clientsoket发送数据  
            Thread myThread = new Thread(ListenClientConnect);
            myThread.IsBackground = true;
            myThread.Start();

            Closing += new CancelEventHandler(onWindowClosing);
        }

        // 循环播放视频
        private void Media_Ended(object sender, EventArgs e)
        {
            // mePlayer.Position = TimeSpan.Zero;
            mePlayer.Source = new Uri("concept.mp4", System.UriKind.Relative);
            mePlayer.Play();
        }

        // 关闭窗口时杀死后台进程
        private void onWindowClosing(object sender, EventArgs e) {
            Environment.Exit(0);
        }

        /// 监听客户端连接  
        private void ListenClientConnect()
        {
            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                clientSocket.Send(Encoding.ASCII.GetBytes("Server Say Hello\n"));
                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(clientSocket);
            }
        }

        /// 接收消息  
        private void ReceiveMessage(object clientSocket)
        {
            Socket myClientSocket = (Socket)clientSocket;
            while (true)
            {
                try
                {
                    //通过clientSocket接收数据  
                    int receiveNumber = myClientSocket.Receive(result);
                    String recv = Encoding.ASCII.GetString(result, 0, receiveNumber);
                    if (recv != null)
                    {
                        mePlayer.Dispatcher.Invoke(new Action(() =>
                        {
                            if (Int32.Parse(recv) == 1)
                            {
                                mePlayer.Source = new Uri("jam.mp4", System.UriKind.Relative);
                                mePlayer.Play();
                            }
                            else if (Int32.Parse(recv) == 2)
                            {
                                mePlayer.Source = new Uri("etc.mkv", System.UriKind.Relative);
                                mePlayer.Play();
                            }
                            else {
                                mePlayer.Source = new Uri("concept.mp4", System.UriKind.Relative);
                                mePlayer.Play();
                            }
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    myClientSocket.Shutdown(SocketShutdown.Both);
                    myClientSocket.Close();
                    break;
                }
            }
        }
    }
}
