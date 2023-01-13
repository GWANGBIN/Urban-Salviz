using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;



namespace imageStream_client
{

    public class Client_unity : MonoBehaviour
    {
        #region imshow
        public RawImage rawimage;
        public AspectRatioFitter fit;

        public static Client_unity viewInstance;
        byte[] image;
        byte[] received_image;
        public int check;
        public int count = 0;
        #endregion

        #region TCP
        IPHostEntry ipHostInfo;
        IPAddress ipAddress; // IPv4 
        TcpClient client_sock;
        const int PORT = 8080;
        NetworkStream stream;
        #endregion

        private IEnumerator coroutine;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log($"*****Unity frame started *****:{EditorApplication.isPlaying}");

            ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[1];

            client_sock = new TcpClient(ipAddress.ToString(), PORT);
            stream = client_sock.GetStream();
            Debug.Log("***** Client Connected to the server *****");
        }


        void Awake()
        {
            viewInstance = this;
        }

        #region TCP_ImageStream()
        byte[] TCP_ImageStream()
        {
            //Debug.Log("**** Buffer streaming ****");

            // _Client -> Server 
            string message = "1";
            byte[] buff = Encoding.ASCII.GetBytes(message);
            Debug.Log("**** Client -> Server **** ");
            stream.Write(buff, 0, buff.Length); // spend the byte stream into the Stream 
            Debug.Log("**** Client -> Server Done **** ");


            // _Client <- Server 

            if (stream.CanRead && client_sock.ReceiveBufferSize > 0)
            {
                Debug.Log("**** Client <- Server **** ");
                byte[] recvBuf = new byte[client_sock.ReceiveBufferSize]; // total receiveBuffer size     
                int readBytes = stream.Read(recvBuf, 0, recvBuf.Length);

                //Debug.Log($"total receiveBuffer length: {recvBuf.Length}");
                //Debug.Log($"Real-read byte length: {readBytes}");    

                // _Set display image                
                image = new byte[readBytes];
                Buffer.BlockCopy(recvBuf, 0, image, 0, readBytes);

                //Viewer.instance.SetImageToDisplay(image);
                Debug.Log("**** Client <-- Server Done **** ");
            }
            return image;
        }
        #endregion


        #region Display_texture2D() 
        public int Display_texture2D(byte[] received_image)
        {
            Debug.Log($"received-image byte: {received_image.Length}");
            if (received_image == null)
            {
                Debug.Log("**** No image byte... ****");
                return 0;
            }
            else
            {

                //이미지 크기 생성 이후 streaming 되는 이미지 크기에 맞게 수정 해야함
                Texture2D texture = new Texture2D(640, 480, TextureFormat.RGB24, false);



                //byte형식의 데이터를 texture2D 형식으로 읽음
                bool load = texture.LoadImage(received_image);
                //Debug.Log($"image byte size:{image.Length}");

                if (load)
                {
                    Debug.Log("**** Image byte loaded... **** ");
                    //이미지를 화면에 입힘(Canvas 아래 RawImage)                        
                    viewInstance.rawimage.texture = texture as Texture;
                    //이미지 가로 세로 비율
                    viewInstance.fit.aspectRatio = 640 / 480;

                    // _Save your images 
                    //byte[] _bytes = texture.EncodeToPNG();
                    //System.IO.File.WriteAllBytes("D:\\ImgStream_TCP\\Assets\\sciprt\\imagefile\\"+count.ToString()+".png", _bytes);
                    //count += 1;
                }

            }
            return 1;
        }
        #endregion


        //  called once per frame
        void Update()
        {
            // _Can be Application.IsRunning or whatever bool you need to receive data        
            received_image = TCP_ImageStream();

            Display_texture2D(received_image);
        }

        #region To close the TCP socket
        // _this function is called when unity play is off. 
        private void OnApplicationQuit()
        {
            Debug.Log($"*****Unity frame Off *****:{EditorApplication.isPlaying}");
            client_sock.Close(); // _Close TCP socket
        }
        #endregion

    }

}

