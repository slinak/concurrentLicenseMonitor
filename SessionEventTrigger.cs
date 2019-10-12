using System;
using ScreenConnect;
using System.Linq;

public class SessionEventTriggerAccessor : IDynamicSessionEventTrigger
{
	public Proc GetDeferredActionIfApplicable(SessionEventTriggerEvent sessionEventTriggerEvent)
	{
		if(sessionEventTriggerEvent.SessionEvent.EventType == SessionEventType.Connected && 
			sessionEventTriggerEvent.SessionConnection.ProcessType == ProcessType.Host) 
		{
			int licenseLimit = Convert.ToInt32(ExtensionContext.Current.GetSettingValue("ConcurrentLicenseTargetCount"));
			
			SessionActiveConnection[] activeSessions = SessionManagerPool.Demux.GetSessions()
				.Where(session => session.ActiveConnections.Length > 0)
				.SelectMany(session => session.ActiveConnections)
				.Where(ac => ac.ProcessType == ProcessType.Host)
				.ToArray();
				
			if(activeSessions.Length >= licenseLimit)
				SendEmail(ExtensionContext.Current);
		}
		return null;
	}
	
	public static void SendEmail(ExtensionContext context)
	{
		try
		{
			using (var mailMessage = ServerToolkit.Instance.CreateMailMessage())
			{
				if (!string.IsNullOrEmpty(context.GetSettingValue("ToEmailAddress")))
					mailMessage.To.Add(context.GetSettingValue("ToEmailAddress"));
				else
					mailMessage.To.Add(AppSettingsCache.DefaultMailToAddress);
				
				mailMessage.Subject = context.GetSettingValue("EmailSubject");
				
				mailMessage.Body = context.GetSettingValue("EmailBody");
	
				mailMessage.IsBodyHtml = false;
				new ScreenConnect.SmtpClient().Send(mailMessage);
			}
		} 
		catch (Exception)
		{
			//not important
		}
	}
}