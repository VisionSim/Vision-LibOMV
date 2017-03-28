/*
 * Copyright (c) Contributors, http://virtual-planets.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Vision-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Globalization;
using System.IO;

namespace OpenMetaverse
{
    public class ConsoleLogger : IDisposable
    {
        static readonly CultureInfo m_cultureInfo = new CultureInfo ("en-US", false);
        static readonly ConsoleColor [] Colors = {
                // the dark colors don't seem to be visible on some black background terminals like putty :(
                //ConsoleColor.DarkBlue,
                //ConsoleColor.DarkGreen,
                //ConsoleColor.Gray, 
                //ConsoleColor.DarkGray,
                ConsoleColor.DarkCyan,
                ConsoleColor.DarkMagenta,
                ConsoleColor.DarkYellow,
                ConsoleColor.Green,
                ConsoleColor.Blue,
                ConsoleColor.Magenta,
                ConsoleColor.Red,
                ConsoleColor.Yellow,
                ConsoleColor.Cyan
        };

        protected TextWriter m_logFile;
        protected string m_logPath = "./";
        protected string m_logName = "LibOMV";
        protected DateTime m_logDate;
        public Helpers.LogLevel Threshold { get; set; }


        public ConsoleLogger ()
        {
            // some additional stuff in case names need to be modified later 
            string logName = "LibOMV";
            string logPath = "./";

            // check the logPath to ensure correct format
            if (!logPath.EndsWith ("/", StringComparison.Ordinal))
                logPath = logPath + "/";
            m_logPath = logPath;

            // make sure the directory exists
            if (!Directory.Exists (m_logPath))
                Directory.CreateDirectory (m_logPath);

            if (m_logName == "")
                m_logName = logName;

            OpenLog ();

        }

        void OpenLog ()
        {
            var logtime = DateTime.Now.AddMinutes (1);          // just a bit of leeway for rotation
            string timestamp = logtime.ToString ("yyyyMMdd");

            // opens the logfile using the system process names
            //string runFilename = System.Diagnostics.Process.GetCurrentProcess ().MainModule.FileName;
            //string runProcess = Path.GetFileNameWithoutExtension(runFilename);
            //m_logFile = StreamWriter.Synchronized(new StreamWriter(m_logPath + runProcess + m_logName + "_" + timestamp + ".log", true));

            m_logFile = TextWriter.Synchronized (new StreamWriter (m_logPath + m_logName + timestamp + ".log", true));
            m_logDate = logtime.Date;
        }

        void RotateLog ()
        {
            m_logFile.Close ();          // close the current log
            OpenLog ();                  // start a new one
        }

        public void Dispose ()
        {
            m_logFile.Close ();
        }

        #region ILog Members
        public void Debug (object message)
        {
            Output (message.ToString (), Helpers.LogLevel.Debug);
        }

        public void Error (object message)
        {
            Output (message.ToString (), Helpers.LogLevel.Error);
        }

        public void Info (object message)
        {
            Output (message.ToString (), Helpers.LogLevel.Info);
        }

        public void Log (object message, Helpers.LogLevel level)
        {
            Output (message.ToString (), level);
        }

        public void Warn (object message)
        {
            Output (message.ToString (), Helpers.LogLevel.Warning);
        }

        #endregion

        #region logoutput
        static ConsoleColor DeriveColor (string input)
        {
            // it is important to do Abs, hash values can be negative
            return Colors [(Math.Abs (input.ToUpper ().Length) % Colors.Length)];
        }

        /// <summary>
        /// Returns a formatted date time string depending upon the system Locale.
        /// Used for logging
        /// </summary>
        /// <returns>Local date time string.</returns>
        static string LocaleLogStamp ()
        {
            string ts = DateTime.Now.ToString ("MMM dd hh:mm:ss", m_cultureInfo);
            return ts;
        }


        void WriteColorText (ConsoleColor color, string sender)
        {
            try {
                lock (this) {
                    try {
                        Console.ForegroundColor = color;
                        Console.Write (sender);
                        Console.ResetColor ();
                    } catch (ArgumentNullException) {
                        // Some older systems don't support colored text.
                        Console.WriteLine (sender);
                    }
                }
            } catch (ObjectDisposedException) {
            }
        }

        void WriteLocalText (string text, Helpers.LogLevel level)
        {
            string logtext = "";
            if (text != "") {
                int CurrentLine = 0;
                string [] Lines = text.Split (new char [] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                //This exists so that we don't have issues with multiline stuff, since something is messed up with the Regex
                foreach (string line in Lines) {
                    string [] split = line.Split (new string [] { "[", "]" }, StringSplitOptions.None);
                    int currentPos = 0;
                    int boxNum = 0;
                    foreach (string s in split) {
                        if (line [currentPos] == '[') {
                            if (level >= Helpers.LogLevel.Error)
                                WriteColorText (ConsoleColor.Red, "[");
                            else if (level >= Helpers.LogLevel.Warning)
                                WriteColorText (ConsoleColor.Yellow, "[");
                            else
                                WriteColorText (ConsoleColor.Gray, "[");
                            boxNum++;
                            currentPos++;
                        } else if (line [currentPos] == ']') {
                            if (level == Helpers.LogLevel.Error)
                                WriteColorText (ConsoleColor.Red, "]");
                            else if (level == Helpers.LogLevel.Warning)
                                WriteColorText (ConsoleColor.Yellow, "]");
                            else
                                WriteColorText (ConsoleColor.Gray, "]");
                            boxNum--;
                            currentPos++;
                        }
                        if (boxNum == 0) {
                            if (level == Helpers.LogLevel.Error)
                                WriteColorText (ConsoleColor.Red, s);
                            else if (level == Helpers.LogLevel.Warning)
                                WriteColorText (ConsoleColor.Yellow, s);
                            else
                                WriteColorText (ConsoleColor.Gray, s);
                        } else //We're in a box
                            WriteColorText (DeriveColor (s), s);
                        currentPos += s.Length; //Include the extra 1 for the [ or ]
                    }

                    CurrentLine++;
                    if (Lines.Length - CurrentLine != 0)
                        Console.WriteLine ();

                    logtext += line;
                }
            }

            Console.WriteLine ();
        }

        public void Output (string text, Helpers.LogLevel level)
        {
            if (Threshold <= level) {
                string ts = LocaleLogStamp () + " - ";
                string fullText = string.Format ("{0} {1}", ts, text);
                if (m_logFile != null) {
                    if (m_logDate != DateTime.Now.Date)
                        RotateLog ();

                    m_logFile.WriteLine (fullText);
                    m_logFile.Flush ();
                }

                WriteColorText (ConsoleColor.DarkCyan, ts);
                WriteLocalText (text, level);
            }
        }

        #endregion
    }
}
