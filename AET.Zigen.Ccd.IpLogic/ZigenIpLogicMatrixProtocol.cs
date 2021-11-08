using System;
using System.Collections.Generic;
using Crestron.RAD.Common;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Transports;
using Crestron.RAD.DeviceTypes.AudioVideoSwitcher;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

namespace AET.Zigen.Ccd.IpLogic {
  public class ZigenIpLogicMatrixProtocol : AAudioVideoSwitcherProtocol {
    private StandardCommandsEnum command;
    private Dictionary<int, string> sourceAddresses = new Dictionary<int, string>();
    private Dictionary<int, string> destinationAddresses = new Dictionary<int, string>();

    public ZigenIpLogicMatrixProtocol(ISerialTransport transport, byte id) : base(transport, id) {
      //PollingInterval = 20000;
      PollingEnabled = false;
      PowerIsOn = true;
    }

		public override void DataHandler(string response)
		{
			try
			{
				StandardCommandsEnum standardCommand = this.command;
        ErrorLog.Info(string.Format("Command: {0}. Response: {1}.", standardCommand, response));

/*				switch (standardCommandsEnum)
				{
				case 6601:
				case 6602:
					JsonConvert.DeserializeObject<HdaPowerCmdResponse>(response);
					break;
				case 6603:
				{
					HdaPowerPollResponse hdaPowerPollResponse = JsonConvert.DeserializeObject<HdaPowerPollResponse>(response);
					base.PowerIsOn = hdaPowerPollResponse.Data.PowerState;
					break;
				}
				default:
					if (standardCommandsEnum == 50001)
					{
						HdaRoutePollResponse hdaRoutePollResponse = JsonConvert.DeserializeObject<HdaRoutePollResponse>(response);
						foreach (HdaZoneData hdaZoneData in hdaRoutePollResponse.Data.Zones)
						{
							foreach (HdaZoneState hdaZoneState in hdaZoneData.State)
							{
								AudioVideoExtender extenderByApiIdentifier = base.GetExtenderByApiIdentifier(hdaZoneState.OutputId);
								AudioVideoExtender extenderByApiIdentifier2 = base.GetExtenderByApiIdentifier(hdaZoneState.InputId);
								if (extenderByApiIdentifier != null)
								{
									extenderByApiIdentifier.VideoSourceExtenderId = ((extenderByApiIdentifier2 == null) ? null : extenderByApiIdentifier2.Id);
								}
							}
						}
					}
					break;
				}
 */
			}
			catch (Exception ex)
			{
				Log("Unable to parse Ip-Logic response: " + ex.Message);
				Log("Response was: " + response);
			}
			base.DataHandler(response);
		}    

    public override void PowerOff() {}

    public override void ExtenderRouteVideoInput(Crestron.RAD.DeviceTypes.AudioVideoSwitcher.Extender.AudioVideoExtender targetExtender, string inputId, string outputId) {
      base.ExtenderRouteVideoInput(targetExtender, inputId, outputId);
    }

    protected override bool PrepareStringThenSend(CommandSet commandSet) {
      command = commandSet.StandardCommand;
      if (commandSet.StandardCommand == StandardCommandsEnum.AudioVideoSwitcherRoute) {
        CreateAvRouteCommand(commandSet);
      }
      return base.PrepareStringThenSend(commandSet);
    }

    private void CreateAvRouteCommand(CommandSet commandSet) {
      int input, output;
      string sourceAddr = string.Empty;
      string destinationAddr = string.Empty;
      if (EnableLogging) Log(string.Format("Zigen HXL CreateAVRouteCommand({0})", commandSet.Command));
      try {
        var route = commandSet.Command.Split(',');
        input = route[0].SafeParseInt();
        output = route[1].SafeParseInt();
        if (!sourceAddresses.TryGetValue(input, out sourceAddr)) if (EnableLogging) Log(string.Format("Zigen HXL CreateAVRouteCommand: Error parsing looking up address for input {0}", input));
        if (!destinationAddresses.TryGetValue(output, out destinationAddr)) if (EnableLogging) Log(string.Format("Zigen HXL CreateAVRouteCommand: Error parsing looking up address for output {0}", output));
      } catch (Exception ex) {
        if (EnableLogging) Log(string.Format("Zigen HXL CreateAVRouteCommand: Error parsing input and output numbers from '{1}' {0}", ex.Message, commandSet.Command));
        input = 0;
        output = 0;
      }      
      commandSet.Command = "RouteHDMI";
      var json = string.Format("{{\"source\":\"{0}\",\"destination\":\"{1}\"}}", sourceAddr, destinationAddr);
      if (EnableLogging) Log(string.Format("Zigen HXL CreateAVRouteCommand({0}): Input = {1}, Output = {2}. New command = RouteHDMI | {3}.", commandSet.Command, input, output, json));
      commandSet.Parameters = new object[] { "application/json", json };
    }
    
   
    public override void SetUserAttribute(string attributeId, string attributeValue) {
      if (attributeId == null) return;
      var value = string.IsNullOrEmpty(attributeValue) ? "" : attributeValue;
      if (attributeId.StartsWith("Input")) {
        var inputNumber = attributeId.Substring(5).SafeParseInt();
        sourceAddresses.Add(inputNumber, value);
      }
      else if (attributeId.StartsWith("Output")) {
        var outputNumber = attributeId.Substring(6).SafeParseInt();
        destinationAddresses.Add(outputNumber, value);        
      }
    }
  }
}