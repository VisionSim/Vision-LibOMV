/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System.Collections.Generic;
using System.Text;

namespace OpenMetaverse.Voice
{
    /// <summary>
    /// Represents a single Voice Session to the Vivox service.
    /// </summary>
    public class VoiceSession
    {
        readonly string m_Handle;
        readonly VoiceGateway connector;
        static Dictionary<string, VoiceParticipant> knownParticipants;
        bool m_spatial;

        public string RegionName;
        public bool IsSpatial { get { return m_spatial; } }
        public VoiceGateway Connector { get { return connector; } }
        public string Handle { get { return m_Handle; } }

        public event System.EventHandler OnParticipantAdded;
        public event System.EventHandler OnParticipantUpdate;
        public event System.EventHandler OnParticipantRemoved;

        public VoiceSession (VoiceGateway conn, string handle)
        {
            m_Handle = handle;
            connector = conn;

            m_spatial = true;
            knownParticipants = new Dictionary<string, VoiceParticipant> ();
        }

        /// <summary>
        /// Close this session.
        /// </summary>
        internal void Close ()
        {

            knownParticipants.Clear ();
        }

        internal void ParticipantUpdate (string uri, bool isMuted, bool isSpeaking, int volume, float energy)
        {
            lock (knownParticipants) {
                // Locate in this session
                VoiceParticipant p = FindParticipant (uri);
                if (p == null) return;

                // Set properties
                p.SetProperties (isSpeaking, isMuted, energy);

                // Inform interested parties.
                if (OnParticipantUpdate != null)
                    OnParticipantUpdate (p, null);
            }
        }

        internal void AddParticipant (string uri)
        {
            lock (knownParticipants) {
                VoiceParticipant p = FindParticipant (uri);

                // We expect that to come back null.  If it is not
                // null, this is a duplicate
                if (p != null) {
                    return;
                }

                // It was not found, so add it.
                p = new VoiceParticipant (uri, this);
                knownParticipants.Add (uri, p);

                /* TODO
                           // Fill in the name.
                           if (p.Name == null || p.Name.StartsWith("Loading..."))
                                   p.Name = control.instance.getAvatarName(p.ID);
                               return p;
               */

                // Inform interested parties.
                if (OnParticipantAdded != null)
                    OnParticipantAdded (p, null);
            }
        }

        internal void RemoveParticipant (string uri)
        {
            lock (knownParticipants) {
                VoiceParticipant p = FindParticipant (uri);
                if (p == null) return;

                // Remove from list for this session.
                knownParticipants.Remove (uri);

                // Inform interested parties.
                if (OnParticipantRemoved != null)
                    OnParticipantRemoved (p, null);
            }
        }

        /// <summary>
        /// Look up an existing Participants in this session
        /// </summary>
        /// <param name="puri"></param>
        /// <returns></returns>
        private VoiceParticipant FindParticipant (string puri)
        {
            if (knownParticipants.ContainsKey (puri))
                return knownParticipants [puri];

            return null;
        }

        public void Set3DPosition (VoicePosition speakerPosition, VoicePosition listenerPosition)
        {
            connector.SessionSet3DPosition (m_Handle, speakerPosition, listenerPosition);
        }
    }

    public partial class VoiceGateway
    {
        /// <summary>
        /// Create a Session
        /// Sessions typically represent a connection to a media session with one or more
        /// participants. This is used to generate an ‘outbound’ call to another user or
        /// channel. The specifics depend on the media types involved. A session handle is
        /// required to control the local user functions within the session (or remote
        /// users if the current account has rights to do so). Currently creating a
        /// session automatically connects to the audio media, there is no need to call
        /// Session.Connect at this time, this is reserved for future use.
        /// </summary>
        /// <param name="accountHandle">Handle returned from successful Connector ‘create’ request</param>
        /// <param name="uri">This is the URI of the terminating point of the session (ie who/what is being called)</param>
        /// <param name="name">This is the display name of the entity being called (user or channel)</param>
        /// <param name="password">Only needs to be supplied when the target URI is password protected</param>
        /// <param name="passwordHashAlgorithm">This indicates the format of the password as passed in. This can either be
        /// “ClearText” or “SHA1UserName”. If this element does not exist, it is assumed to be “ClearText”. If it is
        /// “SHA1UserName”, the password as passed in is the SHA1 hash of the password and username concatenated together,
        /// then base64 encoded, with the final “=” character stripped off.</param>
        /// <param name="joinAudio"></param>
        /// <param name="joinText"></param>
        /// <returns></returns>
        public int SessionCreate (string accountHandle, string uri, string name, string password,
                                 bool joinAudio, bool joinText, string passwordHashAlgorithm)
        {
            StringBuilder sb = new StringBuilder ();
            sb.Append (MakeXML ("AccountHandle", accountHandle));
            sb.Append (MakeXML ("URI", uri));
            sb.Append (MakeXML ("Name", name));
            if (!string.IsNullOrEmpty (password)) {
                sb.Append (MakeXML ("Password", password));
                sb.Append (MakeXML ("PasswordHashAlgorithm", passwordHashAlgorithm));
            }
            sb.Append (MakeXML ("ConnectAudio", joinAudio ? "true" : "false"));
            sb.Append (MakeXML ("ConnectText", joinText ? "true" : "false"));
            sb.Append (MakeXML ("JoinAudio", joinAudio ? "true" : "false"));
            sb.Append (MakeXML ("JoinText", joinText ? "true" : "false"));
            sb.Append (MakeXML ("VoiceFontID", "0"));

            return Request ("Session.Create.1", sb.ToString ());
        }

        /// <summary>
        /// Used to accept a call
        /// </summary>
        /// <param name="sessionHandle">SessionHandle such as received from SessionNewEvent</param>
        /// <param name="audioMedia">"default"</param>
        /// <returns></returns>
        public int SessionConnect (string sessionHandle, string audioMedia)
        {
            StringBuilder sb = new StringBuilder ();
            sb.Append (MakeXML ("SessionHandle", sessionHandle));
            sb.Append (MakeXML ("AudioMedia", audioMedia));
            return Request ("Session.Connect.1", sb.ToString ());
        }

        /// <summary>
        /// This command is used to start the audio render process, which will then play
        /// the passed in file through the selected audio render device. This command
        /// should not be issued if the user is on a call.
        /// </summary>
        /// <param name="soundFilePath">The fully qualified path to the sound file.</param>
        /// <param name="loop">True if the file is to be played continuously and false if it is should be played once.</param>
        /// <returns></returns>
        public int SessionRenderAudioStart (string soundFilePath, bool loop)
        {
            StringBuilder sb = new StringBuilder ();
            sb.Append (MakeXML ("SoundFilePath", soundFilePath));
            sb.Append (MakeXML ("Loop", loop ? "1" : "0"));
            return Request ("Session.RenderAudioStart.1", sb.ToString ());
        }

        /// <summary>
        /// This command is used to stop the audio render process.
        /// </summary>
        /// <param name="soundFilePath">The fully qualified path to the sound file issued in the start render command.</param>
        /// <returns></returns>
        public int SessionRenderAudioStop (string soundFilePath)
        {
            string RequestXML = MakeXML ("SoundFilePath", soundFilePath);
            return Request ("Session.RenderAudioStop.1", RequestXML);
        }

        /// <summary>
        /// This is used to ‘end’ an established session (i.e. hang-up or disconnect).
        /// </summary>
        /// <param name="sessionHandle">Handle returned from successful Session ‘create’ request or a SessionNewEvent</param>
        /// <returns></returns>
        public int SessionTerminate (string sessionHandle)
        {
            string RequestXML = MakeXML ("SessionHandle", sessionHandle);
            return Request ("Session.Terminate.1", RequestXML);
        }

        /// <summary>
        /// Set the combined speaking and listening position in 3D space.
        /// </summary>
        /// <param name="sessionHandle">Handle returned from successful Session ‘create’ request or a SessionNewEvent</param>
        /// <param name="speakerPosition">Speaking position</param>
        /// <param name="listenerPosition">Listening position</param>
        /// <returns></returns>
        public int SessionSet3DPosition (string sessionHandle, VoicePosition speakerPosition, VoicePosition listenerPosition)
        {
            StringBuilder sb = new StringBuilder ();
            sb.Append (MakeXML ("SessionHandle", sessionHandle));
            sb.Append ("<SpeakerPosition>");

            sb.Append ("<Position>");
            sb.Append (MakeXML ("X", speakerPosition.Position.X.ToString ()));
            sb.Append (MakeXML ("Y", speakerPosition.Position.Y.ToString ()));
            sb.Append (MakeXML ("Z", speakerPosition.Position.Z.ToString ()));
            sb.Append ("</Position>");

            sb.Append ("<Velocity>");
            sb.Append (MakeXML ("X", speakerPosition.Velocity.X.ToString ()));
            sb.Append (MakeXML ("Y", speakerPosition.Velocity.Y.ToString ()));
            sb.Append (MakeXML ("Z", speakerPosition.Velocity.Z.ToString ()));
            sb.Append ("</Velocity>");

            sb.Append ("<AtOrientation>");
            sb.Append (MakeXML ("X", speakerPosition.AtOrientation.X.ToString ()));
            sb.Append (MakeXML ("Y", speakerPosition.AtOrientation.Y.ToString ()));
            sb.Append (MakeXML ("Z", speakerPosition.AtOrientation.Z.ToString ()));
            sb.Append ("</AtOrientation>");

            sb.Append ("<UpOrientation>");
            sb.Append (MakeXML ("X", speakerPosition.UpOrientation.X.ToString ()));
            sb.Append (MakeXML ("Y", speakerPosition.UpOrientation.Y.ToString ()));
            sb.Append (MakeXML ("Z", speakerPosition.UpOrientation.Z.ToString ()));
            sb.Append ("</UpOrientation>");

            sb.Append ("<LeftOrientation>");
            sb.Append (MakeXML ("X", speakerPosition.LeftOrientation.X.ToString ()));
            sb.Append (MakeXML ("Y", speakerPosition.LeftOrientation.Y.ToString ()));
            sb.Append (MakeXML ("Z", speakerPosition.LeftOrientation.Z.ToString ()));
            sb.Append ("</LeftOrientation>");

            sb.Append ("</SpeakerPosition>");

            sb.Append ("<ListenerPosition>");

            sb.Append ("<Position>");
            sb.Append (MakeXML ("X", listenerPosition.Position.X.ToString ()));
            sb.Append (MakeXML ("Y", listenerPosition.Position.Y.ToString ()));
            sb.Append (MakeXML ("Z", listenerPosition.Position.Z.ToString ()));
            sb.Append ("</Position>");

            sb.Append ("<Velocity>");
            sb.Append (MakeXML ("X", listenerPosition.Velocity.X.ToString ()));
            sb.Append (MakeXML ("Y", listenerPosition.Velocity.Y.ToString ()));
            sb.Append (MakeXML ("Z", listenerPosition.Velocity.Z.ToString ()));
            sb.Append ("</Velocity>");

            sb.Append ("<AtOrientation>");
            sb.Append (MakeXML ("X", listenerPosition.AtOrientation.X.ToString ()));
            sb.Append (MakeXML ("Y", listenerPosition.AtOrientation.Y.ToString ()));
            sb.Append (MakeXML ("Z", listenerPosition.AtOrientation.Z.ToString ()));
            sb.Append ("</AtOrientation>");

            sb.Append ("<UpOrientation>");
            sb.Append (MakeXML ("X", listenerPosition.UpOrientation.X.ToString ()));
            sb.Append (MakeXML ("Y", listenerPosition.UpOrientation.Y.ToString ()));
            sb.Append (MakeXML ("Z", listenerPosition.UpOrientation.Z.ToString ()));
            sb.Append ("</UpOrientation>");

            sb.Append ("<LeftOrientation>");
            sb.Append (MakeXML ("X", listenerPosition.LeftOrientation.X.ToString ()));
            sb.Append (MakeXML ("Y", listenerPosition.LeftOrientation.Y.ToString ()));
            sb.Append (MakeXML ("Z", listenerPosition.LeftOrientation.Z.ToString ()));
            sb.Append ("</LeftOrientation>");

            sb.Append ("</ListenerPosition>");

            return Request ("Session.Set3DPosition.1", sb.ToString ());
        }

        /// <summary>
        /// Set User Volume for a particular user. Does not affect how other users hear that user.
        /// </summary>
        /// <param name="sessionHandle">Handle returned from successful Session ‘create’ request or a SessionNewEvent</param>
        /// <param name="participantURI"></param>
        /// <param name="volume">The level of the audio, a number between -100 and 100 where 0 represents ‘normal’ speaking volume</param>
        /// <returns></returns>
        public int SessionSetParticipantVolumeForMe (string sessionHandle, string participantURI, int volume)
        {
            StringBuilder sb = new StringBuilder ();
            sb.Append (MakeXML ("SessionHandle", sessionHandle));
            sb.Append (MakeXML ("ParticipantURI", participantURI));
            sb.Append (MakeXML ("Volume", volume.ToString ()));
            return Request ("Session.SetParticipantVolumeForMe.1", sb.ToString ());
        }
    }
}
