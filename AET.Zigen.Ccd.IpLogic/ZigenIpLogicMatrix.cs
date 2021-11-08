using AET.Ccd;
using Crestron.RAD.Common.Interfaces;
using Crestron.RAD.DeviceTypes.AudioVideoSwitcher;
using Crestron.SimplSharp; 

namespace AET.Zigen.Ccd.IpLogic {
  public class ZigenIpLogicMatrix : AAudioVideoSwitcher, ITcp {
    public void Initialize(IPAddress ipAddress, int port) {
      var httpRestTransport = new HttpRestTransport {
        EnableLogging = InternalEnableLogging,
        CustomLogger = InternalCustomLogger,
        EnableRxDebug = InternalEnableRxDebug,
        EnableTxDebug = InternalEnableTxDebug,
        IsConnected = true
      };      
      httpRestTransport.Initialize(ipAddress, port, null);      
      ConnectionTransport = httpRestTransport;
      AudioVideoSwitcherProtocol = new ZigenIpLogicMatrixProtocol(ConnectionTransport, Id) {
        EnableLogging = InternalEnableLogging,
        CustomLogger = InternalCustomLogger        
      };      
      AudioVideoSwitcherProtocol.RxOut += SendRxOut;
      AudioVideoSwitcherProtocol.Initialize(AudioVideoSwitcherData);
    }
  }
}