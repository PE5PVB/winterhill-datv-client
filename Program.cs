using System;
using System.IO;
using System.Windows.Forms;

namespace datvreceiver
{
    /// <summary>
    /// Application entry point for the Winterhill DATV Receiver client.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// Sets up global exception handlers and starts the main form.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Set up global exception handlers to log crashes to file
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (sender, e) => LogException("ThreadException", e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
                LogException("UnhandledException", e.ExceptionObject as Exception);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new mainForm());
        }

        /// <summary>
        /// Logs an unhandled exception to crash.log and shows error dialog.
        /// Log file is stored in %LocalAppData%\datvreceiver\crash.log
        /// </summary>
        /// <param name="source">Exception source (ThreadException or UnhandledException)</param>
        /// <param name="ex">The exception that occurred</param>
        private static void LogException(string source, Exception ex)
        {
            try
            {
                // Ensure log directory exists
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "datvreceiver");
                if (!Directory.Exists(appDataPath))
                    Directory.CreateDirectory(appDataPath);

                // Append exception details to crash log
                string logPath = Path.Combine(appDataPath, "crash.log");
                string message = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " [" + source + "]\r\n" +
                    (ex != null ? ex.ToString() : "Unknown exception") + "\r\n\r\n";
                File.AppendAllText(logPath, message);

                // Show error dialog to user
                MessageBox.Show(
                    "An error has occurred. Details have been saved to:\r\n" + logPath + "\r\n\r\n" +
                    (ex != null ? ex.Message : "Unknown error"),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                // Ignore errors during crash logging
            }
        }
    }
}
