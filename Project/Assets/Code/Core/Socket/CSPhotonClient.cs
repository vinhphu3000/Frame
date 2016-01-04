using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OperationCode;
using ProtoBuf;

namespace CS
{
    public class CSPhotonClient : MonoBehaviour
    {

        PhotonOperator photonClient;
        public bool IS_CONNECT_SERVER;

        void Start()
        {
            Application.runInBackground = true;

            IS_CONNECT_SERVER = false;
            photonClient = new CSPhotonPeer();
        }


        void Update()
        {
            if (IS_CONNECT_SERVER)
                photonClient.Service();
        }

        public void OnDestroy()
        {
            if (IS_CONNECT_SERVER)
                photonClient.Disconnect();
        }


        public void _Connect()
        {
            IS_CONNECT_SERVER = photonClient.Connect("localhost:5055", "CSServer");
        }


        public void _Disconnect()
        {
            if (IS_CONNECT_SERVER)
            {
                photonClient.Disconnect();
                IS_CONNECT_SERVER = false;
            }
        }


        void OnGUI()
        {

            if (GUI.Button(new Rect(0, 0, 70, 30), "Connect"))
            {
                _Connect();
            }

            if (GUI.Button(new Rect(0, 30, 70, 30), "Disconnect"))
            {
                _Disconnect();
            }

            if (GUI.Button(new Rect(0, 60, 70, 30), "Send"))
            {
                Dictionary<byte, object> p = new Dictionary<byte, object>();

                CSLoginGame req = new CSLoginGame();
                req.m_nAccount = "1234";
                req.m_nPassword = "qwer";
                byte[] datas = PBSerialize.Serialize<CSLoginGame>(req);
                p.Add((byte)ModelCode.Login, datas);

                Utility.Log(string.Format("Send Data Size {0}.byte", datas.Length));
                photonClient.SendMsg((byte)ModelCode.Login, p);
            }
        }

    }
}


