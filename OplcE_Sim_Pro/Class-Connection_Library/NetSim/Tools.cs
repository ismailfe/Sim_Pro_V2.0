using System;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;

namespace OplcE_Sim_Pro
{
    public class Tools
    {
        public static bool StopService(string serviceName, int timeoutMilliseconds, bool dontShowMessageBoxes)
        {
            ServiceController service = new ServiceController(serviceName);
            bool retry = false;
            do
            {
                try
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                    if (retry == false)
                    {
                        service.Stop();
                    }
                    service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                    if (!dontShowMessageBoxes)
                    {
                        MessageBox.Show("Service '" + serviceName + "' was stopped successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    retry = false;
                }
                catch (System.ServiceProcess.TimeoutException)
                {
                    DialogResult result = MessageBox.Show("A timeout occured stopping the service '" + serviceName + "'.\n" +
                        "Would you like to try again?", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    retry = (result == DialogResult.Retry);
                }
                catch (Exception ex)
                {
                    if (!dontShowMessageBoxes)
                    {
                        MessageBox.Show("Could not stop service '" + serviceName + "'\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    retry = false;
                }
                // Maybe the service was stopped after waiting time in dialog
                service.Refresh();
                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    retry = false;
                }
            } while (retry);
            return (service.Status == ServiceControllerStatus.Stopped);
        }

        public static bool StartService(string serviceName, int timeoutMilliseconds, bool dontShowMessageBoxes, bool restartIfRunning)
        {
            ServiceController service = new ServiceController(serviceName);
            bool retry = false;

            if (restartIfRunning && service.Status == ServiceControllerStatus.Running)
            {
                do
                {
                    try
                    {
                        TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                        if (retry == false)
                        {
                            service.Stop();
                        }
                        service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                        retry = false;
                    }
                    catch (System.ServiceProcess.TimeoutException)
                    {
                        DialogResult result = MessageBox.Show("A timeout occured stopping the service '" + serviceName + "'.\n" +
                            "Would you like to try again?", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                        retry = (result == DialogResult.Retry);

                    }
                    catch (Exception ex)
                    {
                        if (!dontShowMessageBoxes)
                        {
                            MessageBox.Show("Could not stop service '" + serviceName + "' before starting.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        retry = false;
                    }
                    service.Refresh();
                    if (service.Status == ServiceControllerStatus.Stopped)
                    {
                        retry = false;
                    }
                } while (retry);
            }
            do
            {
                try
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                    if (retry == false)
                    {
                        service.Start();
                    }
                    service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                    if (!dontShowMessageBoxes)
                    {
                        MessageBox.Show("Service '" + serviceName + "' was started successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    retry = false;
                }
                catch (System.ServiceProcess.TimeoutException)
                {
                    DialogResult result = MessageBox.Show("A timout occured starting the service '" + serviceName + "'.\n" +
                        "Would you like to try again?", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                    retry = (result == DialogResult.Retry);
                }
                catch (Exception ex)
                {
                    if (!dontShowMessageBoxes)
                    {
                        MessageBox.Show("Could not start service '" + serviceName + "'\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    retry = false;
                }
                service.Refresh();
                if (service.Status == ServiceControllerStatus.Running)
                {
                    retry = false;
                }
            } while (retry);

            return (service.Status == ServiceControllerStatus.Running);
        }

        public static string GetS7DOSHelperServiceName()
        {
            string machineName = ".";   // local
            ServiceController[] services = null;
            try
            {
                services = ServiceController.GetServices(machineName);
            }
            catch
            {
                return String.Empty;
            }
            for (int i = 0; i < services.Length; i++)
            {
                if (services[i].ServiceName == "s7oiehsx")
                {
                    return services[i].ServiceName;
                }
                else if (services[i].ServiceName == "s7oiehsx64")
                {
                    return services[i].ServiceName;
                }
            }
            return String.Empty;
        }

        public static bool IsTcpPortAvailable(int port)
        {
            bool isAvailable = true;

            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endpoint in tcpConnInfoArray)
            {
                if (endpoint.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }
            return isAvailable;
        }
    }
}
