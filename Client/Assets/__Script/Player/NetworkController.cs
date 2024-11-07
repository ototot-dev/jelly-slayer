using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Game
{

    /// <summary>
    /// 네트워크 (세션) 컨트롤러
    /// </summary>
    public class NetworkController : MonoBehaviour
    {

        /// <summary>
        /// 패킷 송신용
        /// </summary>
        public void SendPacket(Packets.Header pktHeader, Packets.Body pktBody)
        {
            __sendPacketQueue.Enqueue(new Tuple<Packets.Header, Packets.Body>(pktHeader, pktBody));
        }

        /// <summary>
        /// 
        /// </summary>
        public void DispatchRecvPacket()
        {
            Packets.Body pktBody;

            while (__recvPacketQueue.TryDequeue(out pktBody))
            {
                var pktType = pktBody.GetType();

                if (pktType == typeof(Packets.PingPong))
                {

                }
                else if (pktType == typeof(Packets.MovePawn))
                {

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Address;

        /// <summary>
        /// 
        /// </summary>
        public int port;

        /// <summary>
        /// 서버 접속
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="port"></param>
        public IDisposable ConnectToServer(string addr, int port)
        {
            IPAddress tempAddr;

            if (!IPAddress.TryParse(addr, out tempAddr))
                return null;

            var endPoint = new IPEndPoint(tempAddr, port);

            __socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            return __socket.ConnectAsync(endPoint).ToObservable()
                .DoOnError(e =>
                {
                    Debug.LogWarning($"Connect failed!! => {e.ToString()}");
                })
                .Subscribe(_ =>
                {
                    if (__socket.Connected)
                    {
                        Debug.Log("Connected~~");
                    }
                });
        }

        Socket __socket;
        const int __MAX_PACKET_SIZE = 1024;
        ConcurrentQueue<Tuple<Packets.Header, Packets.Body>> __sendPacketQueue = new ConcurrentQueue<Tuple<Packets.Header, Packets.Body>>();
        ConcurrentQueue<Packets.Body> __recvPacketQueue = new ConcurrentQueue<Packets.Body>();

        void Start()
        {
            ConnectToServer("127.0.0.1", 8000);

            StartRecvPakcetHandler();
            StartSendPacketHandler();

            Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(_ => {
                var pkt = new Packets.MovePawn();
                pkt.pos = Vector3.one;
                pkt.velocity = Vector3.forward;

                SendPacket(new Packets.Header(Packets.Types.MovePawn, 1), pkt);
            });
        }

        void StartRecvPakcetHandler()
        {
            Observable.Start(() =>
           {
               var recvBuff = new Byte[Packets.Header.MAX_FRAME_SIZE];
               int recvSize = 0;
               var errorCode = System.Net.Sockets.SocketError.Success;
               var pktHeader = new Packets.Header();

               do
               {
                   if (!__socket.Connected)
                       System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));

                   recvSize += __socket.Receive(recvBuff, recvSize, recvBuff.Length, SocketFlags.None, out errorCode);

                   if (errorCode != System.Net.Sockets.SocketError.Success)
                   {
                       Debug.LogError($"Failed to recv a packet => Error: {errorCode.ToString()}");
                       break;
                   }


                   if (!Packets.Encoding.GetHeaderFromBytes(recvBuff, out pktHeader))
                       continue;
                       
                   if (recvSize == pktHeader.pktSize)
                   {
                       switch (pktHeader.pktId)
                       {
                           case Packets.Types.PingPong:
                               __recvPacketQueue.Enqueue(Packets.Encoding.GetPacketFromBytes<Packets.PingPong>(recvBuff, ref pktHeader));
                               break;

                           case Packets.Types.MovePawn:
                               __recvPacketQueue.Enqueue(Packets.Encoding.GetPacketFromBytes<Packets.MovePawn>(recvBuff, ref pktHeader));
                               break;

                           default:
                               break;
                       }

                       recvSize = 0;
                   }
               }
               while (true);
           }).Subscribe().AddTo(this);
        }

        void StartSendPacketHandler()
        {
            Observable.Start(() =>
            {
                Byte[] __sendBuff = new Byte[Packets.Header.MAX_FRAME_SIZE];
                int __sendSize = 0;

                do
                {
                    if (!__socket.Connected)
                        System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));

                    Tuple<Packets.Header, Packets.Body> pkt;

                    if (__sendPacketQueue.Count > 0 && __sendPacketQueue.TryDequeue(out pkt))
                    {
                        var sendSize = Packets.Encoding.ToBytes(pkt.Item1.sessionId, pkt.Item1.pktId, pkt.Item2, __sendBuff);

                        if (sendSize > 0)
                        {
                            __sendSize = __socket.Send(__sendBuff, sendSize, SocketFlags.None);
                            Debug.Log($"SendSize => {sendSize}");
                        }
                    }
                }
                while (true);
            }).Subscribe().AddTo(this);
        }

    }

}
