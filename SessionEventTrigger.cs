using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ScreenConnect;

public class SessionEventTriggerAccessor : IAsyncDynamicEventTrigger<SessionEventTriggerEvent>
{
	public async Task ProcessEventAsync(SessionEventTriggerEvent sessionEventTriggerEvent)
	{
		if (sessionEventTriggerEvent.SessionEvent.EventType == SessionEventType.Connected && 
			sessionEventTriggerEvent.Connection.ProcessType == ProcessType.Host) 
		{
			int licenseLimit = Convert.ToInt32(ExtensionContext.Current.GetSettingValue("ConcurrentLicenseTargetCount"));
			
			var activeSessions = await SessionManagerPool.Demux.GetSessionsAsync("HostConnectedCount > 0");
				
			if(activeSessions.Count >= licenseLimit)
				await SendEmail(ExtensionContext.Current);
		}
	}
	
	async Task SendEmail(ExtensionContext context)
	{
		try
		{
			using (var mailMessage = ServerToolkit.Instance.CreateMailMessage())
			{
				if (!string.IsNullOrEmpty(context.GetSettingValue("EmailToAddress")))
					mailMessage.To.Add(context.GetSettingValue("EmailToAddress"));
				else
					mailMessage.To.Add(AppSettingsCache.DefaultMailToAddress);
				
				mailMessage.Subject = context.GetSettingValue("EmailSubject");
				
				mailMessage.Body = context.GetSettingValue("EmailBody");
	
				mailMessage.IsBodyHtml = false;
				await new ScreenConnect.SmtpClient().SendMailAsync(mailMessage);
			}
		} 
		catch (Exception)
		{
			//not important
		}
	}
}