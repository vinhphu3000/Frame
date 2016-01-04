using UnityEngine;
using ExitGames.Client.Photon;
using System.Collections.Generic;

namespace CS
{

    public class User
    {
        public int UserID;
        public string UserName;
    }


    public interface PhotonOperator
    {
        bool Connect( string ip, string appname);

        void Disconnect();

        void Service();

        void SendMsg(byte protoKey, Dictionary<byte, object> param);
    }


    public class CSPhotonPeer : IPhotonPeerListener, PhotonOperator
    {

        PhotonPeer peer;

        public bool Connect(string ip, string appname)
        {
            peer = new PhotonPeer(this, ConnectionProtocol.Udp);
            //"localhost:5055" "CSServer"
            if (peer.Connect(ip, appname)) 
            {
                return true;
            }
        //peer.OpCustom( )
            return false;
        }

        /// <summary>
        /// 主动短线
        /// </summary>

        public void Disconnect()
        {
            peer.Disconnect();
        }


        /// <summary>
        /// 广播服务
        /// </summary>

        public void Service()
        {
            peer.Service();
        }


        public void SendMsg(byte protoKey, Dictionary<byte, object> param)
        {
            peer.OpCustom(protoKey, param, true);
        }


        public void DebugReturn(DebugLevel level, string message)
        {
            Utility.Log(message);
        }


        public void OnEvent(EventData eventData)
        {
        }


        public void OnOperationResponse(OperationResponse operationResponse)
        {
            Utility.Log(operationResponse.ToString());
        }


        public void OnStatusChanged(StatusCode statusCode)
        {
            switch (statusCode)
            {
                    //  连接成功
                case StatusCode.Connect:
                    Utility.Log("Connect Success!");
                    break;

                    //  断线
                case StatusCode.Disconnect:
                    //  服务器未开启
                    Utility.Log("Disconnect!");
                    break;

                    //  人数上线
                case StatusCode.ExceptionOnConnect:
                    Utility.Log("超过Photon授权人数上线");
                    break;

                    //  服务器强制断开
                case StatusCode.DisconnectByServer:
                    Utility.Log("服务器强制断开");
                    break;

                    //  超时断线
                case StatusCode.TimeoutDisconnect:
                    Utility.Log("超时断线");
                    break;

                    //  其他原因
                case StatusCode.Exception:
                    Utility.Log("Exception");
                    break;

                    //  收到异常处理
                case StatusCode.ExceptionOnReceive:
                    //  服务器未开启
                    Utility.Log("ExceptionOnReceive");
                    break;
                default:
                    Utility.Log(statusCode.ToString());
                    break;
            }

        }


    
    }
}