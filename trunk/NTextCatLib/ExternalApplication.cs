using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace IvanAkcheurov.NTextCat.Lib
{
    public class ExternalApplication : IDisposable
    {
        private readonly string _filename;
        private readonly string _arguments;
        private Stream _inputStream;
        private Process _externalAppProc;
        private bool _launched;
        private volatile bool _disposed;

        public ExternalApplication(string filename, string arguments, Stream inputStream)
        {
            _filename = filename;
            _arguments = arguments;
            _inputStream = inputStream;
        }
        public Stream Launch()
        {
            if (_disposed)
                throw new ObjectDisposedException("Instance has been already disposed");
            if (_launched)
                throw new InvalidOperationException("Instance cannot be launched second time");
            _launched = true;
            ProcessStartInfo psi =
                new ProcessStartInfo
                    {
                        StandardOutputEncoding = Encoding.GetEncoding(1250),
                        FileName = _filename,
                        Arguments = _arguments,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
            _externalAppProc = new Process();
            _externalAppProc.StartInfo = psi;
            _externalAppProc.Start();

            ThreadPool.QueueUserWorkItem(
                parameter =>
                    {
                        ExternalApplication eap = (ExternalApplication) parameter;
                        if (eap._disposed)
                            return;
                        using (var reader = new StreamReader(_inputStream, Encoding.GetEncoding(1250)))
                        using (_externalAppProc.StandardInput)
                        {
                            char[] buffer = new char[64*1024];
                            int readBytes;
                            while ((readBytes = reader.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                if (eap._disposed)
                                    return;
                                _externalAppProc.StandardInput.Write(buffer, 0, readBytes);
                                if (eap._disposed)
                                    return;
                            }
                            _externalAppProc.StandardInput.Flush();
                            _externalAppProc.StandardInput.Close();
                        }
                    },
                this
                );

            return new TextReaderStream(_externalAppProc.StandardOutput, Encoding.GetEncoding(1250));
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _externalAppProc.Dispose();
        }

        //private void cmdExecute_Click(object sender, EventArgs e)
        //{
        //    string cmd = textInput.Text;
        //    _pythonProc.StandardInput.WriteLine(cmd);
        //    _pythonProc.StandardInput.Flush();
        //    textInput.Text = string.Empty;
        //}

        //private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        //{
        //    if (!_pythonProc.HasExited)
        //        _pythonProc.Kill();
        //}
    }
}
