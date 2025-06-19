using System.Net.Sockets;
using System.Net;
using UnityEngine;


public class NeuroGuideUDPTest: MonoBehaviour
{
    public int sendPort = 50000;
    private UdpClient udpClient;

    void Start()
    {
        udpClient = new UdpClient();
    }

    void OnGUI()
    {
        if(GUI.Button( new Rect( 10, 10, 150, 30 ), "Send Reward (1)" ))
        {
            SendData( 1 );
        }
        if(GUI.Button( new Rect( 10, 50, 150, 30 ), "Send Non-Reward (0)" ))
        {
            SendData( 0 );
        }
    }

    private void SendData( byte data )
    {
        try
        {
            byte[ ] sendBytes = new byte[ ] { data };
            udpClient.Send( sendBytes, sendBytes.Length, new IPEndPoint( IPAddress.Parse( "127.0.0.1" ), sendPort ) );
            Debug.Log( $"Sent data: {data}" );
        }
        catch(System.Exception e)
        {
            Debug.LogError( "Error sending UDP data: " + e.Message );
        }
    }

    void OnDestroy()
    {
        if(udpClient != null)
        {
            udpClient.Close();
        }
    }

} //END NeuroGuideUDPTest Class
